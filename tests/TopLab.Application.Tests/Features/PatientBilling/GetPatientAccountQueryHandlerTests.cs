using TopLab.Application.Features.PatientBilling.Queries.GetPatientAccount;
using TopLab.Application.Common.Results;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Billing;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;
using TopLab.Domain.Users;
using Xunit;

namespace TopLab.Application.Tests.Features.PatientBilling;

public class GetPatientAccountQueryHandlerTests
{
    private static Patient MakePatient(int id, string name = "Ahmed")
    {
        return Patient.Create(PatientId.Create(id), name, Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow);
    }

    private static void AddTest(FakeApplicationDbContext db, int id, string name, string receiptName)
    {
        db.Tests.Add(Test.Create(TestId.Create(id), name, name, receiptName, $"T{id}", 30, 100m));
    }

    private static PaymentOperation MakeOp(
        int id, int patientId, decimal amount, int userId = 7,
        decimal? discount = null, bool extra = false,
        OperationType opType = OperationType.Payment, bool voided = false)
    {
        var op = PaymentOperation.Create(
            PaymentOperationId.Create(id), PatientId.Create(patientId), amount, userId,
            DateTime.UtcNow, discount, extra, opType);
        if (voided)
            op.Void();
        return op;
    }

    private static void AddUser(FakeApplicationDbContext db, int id, string userName)
    {
        db.Users.Add(User.Create(UserId.Create(id), userName, "hash", "winhash"));
    }

    [Fact]
    public async Task HappyPath_MixedRows_MatchesDomainFormulaExactly()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient(1));
        AddTest(db, 10, "CBC", "CBC-RECEIPT");
        AddTest(db, 11, "Glucose", "GLU-RECEIPT");
        db.PatientTests.Add(PatientTest.Create(PatientTestId.Create(100), PatientId.Create(1), TestId.Create(10), 100m));
        db.PatientTests.Add(PatientTest.Create(PatientTestId.Create(101), PatientId.Create(1), TestId.Create(11), 50m));
        AddUser(db, 7, "cashier1");
        db.PaymentOperations.Add(MakeOp(1, 1, 20m, extra: true));
        db.PaymentOperations.Add(MakeOp(2, 1, 80m, discount: 10m));
        db.PaymentOperations.Add(MakeOp(3, 1, 999m, voided: true));

        var handler = new GetPatientAccountQueryHandler(db);
        var result = await handler.Handle(new GetPatientAccountQuery(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var account = result.Value!;
        Assert.Equal(1, account.PatientId);
        Assert.Equal("Ahmed", account.PatientFullName);
        // Charged 100+50+20 = 170; Paid 80+10 = 90; Discount 10; Balance 80.
        Assert.Equal(170m, account.TotalCharged);
        Assert.Equal(90m, account.TotalPaid);
        Assert.Equal(10m, account.TotalDiscount);
        Assert.Equal(80m, account.Balance);
        Assert.Equal(
            PatientAccountCalculator.Balance(
                new List<decimal> { 100m, 50m },
                db.PaymentOperations.ToList()),
            account.Balance);
        Assert.Equal(2, account.ChargedTests.Count);
        Assert.Equal("CBC-RECEIPT", account.ChargedTests[0].ReceiptName);
        Assert.Equal("T10", account.ChargedTests[0].TestCode);
        // History shows all three rows, voided one flagged.
        Assert.Equal(3, account.Operations.Count);
        Assert.Contains(account.Operations, o => o.IsVoided && o.Amount == 999m);
        Assert.Equal("Payment", account.Operations.First(o => o.PaymentOperationId == 2).OperationType);
        Assert.Equal("cashier1", account.Operations.First(o => o.PaymentOperationId == 2).ReceivedByUserName);
    }

    [Fact]
    public async Task UnknownPatient_ReturnsNotFound()
    {
        var db = new FakeApplicationDbContext();

        var handler = new GetPatientAccountQueryHandler(db);
        var result = await handler.Handle(new GetPatientAccountQuery(99), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
        Assert.Equal("المريض غير موجود.", result.Error!.Message);
    }

    [Fact]
    public async Task SoftDeletedPatient_ReturnsNotFound()
    {
        var db = new FakeApplicationDbContext();
        var patient = MakePatient(1);
        patient.SoftDelete();
        db.Patients.Add(patient);

        var handler = new GetPatientAccountQueryHandler(db);
        var result = await handler.Handle(new GetPatientAccountQuery(1), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
        Assert.Equal("المريض غير موجود.", result.Error!.Message);
    }

    [Fact]
    public async Task DeletedUser_FallsBackToRawIdString()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient(1));
        // No user row with id 42 exists.
        db.PaymentOperations.Add(MakeOp(1, 1, 50m, userId: 42));

        var handler = new GetPatientAccountQueryHandler(db);
        var result = await handler.Handle(new GetPatientAccountQuery(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("42", result.Value!.Operations.Single().ReceivedByUserName);
    }

    [Fact]
    public async Task Correction_ReducesBalance_AndExtraCharge_IncreasesCharged()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient(1));
        AddTest(db, 10, "CBC", "CBC-R");
        db.PatientTests.Add(PatientTest.Create(PatientTestId.Create(100), PatientId.Create(1), TestId.Create(10), 100m));
        AddUser(db, 7, "cashier1");
        db.PaymentOperations.Add(MakeOp(1, 1, 60m));
        db.PaymentOperations.Add(MakeOp(2, 1, 30m, opType: OperationType.Correction));
        db.PaymentOperations.Add(MakeOp(3, 1, 20m, extra: true));

        var handler = new GetPatientAccountQueryHandler(db);
        var result = await handler.Handle(new GetPatientAccountQuery(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(120m, result.Value!.TotalCharged);
        Assert.Equal(90m, result.Value.TotalPaid);
        Assert.Equal(30m, result.Value.Balance);
        Assert.Equal("Correction", result.Value.Operations.First(o => o.PaymentOperationId == 2).OperationType);
    }
}
