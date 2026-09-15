using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientBilling.Queries.GetPatientAccount;
using TopLab.Application.Features.ResultDelivery.Queries.GetDeliveryAccount;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Billing;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Application.Tests.Features.ResultDelivery;

public class GetDeliveryAccountQueryHandlerTests
{
    private static Patient MakePatient(int id = 1)
    {
        return Patient.Create(PatientId.Create(id), $"P{id}", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow);
    }

    private static void AddTest(FakeApplicationDbContext db, int id, decimal catalogPrice = 100m)
    {
        db.Tests.Add(Test.Create(TestId.Create(id), $"T{id}", $"T{id}", $"T{id}-R", $"T{id}", 30, catalogPrice));
    }

    private static PaymentOperation MakeOp(int id, int patientId, decimal amount, decimal? discount = null)
    {
        return PaymentOperation.Create(
            PaymentOperationId.Create(id), PatientId.Create(patientId), amount, 7,
            DateTime.UtcNow, discount, false, OperationType.Payment);
    }

    [Fact]
    public async Task Matches_GetPatientAccount_NumberForNumber()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient());
        AddTest(db, 10);
        AddTest(db, 11);
        db.PatientTests.Add(PatientTest.Create(PatientTestId.Create(100), PatientId.Create(1), TestId.Create(10), 100m));
        db.PatientTests.Add(PatientTest.Create(PatientTestId.Create(101), PatientId.Create(1), TestId.Create(11), 50m));
        db.PaymentOperations.Add(MakeOp(1, 1, 80m, discount: 10m));

        var handler = new GetDeliveryAccountQueryHandler(db);
        var result = await handler.Handle(new GetDeliveryAccountQuery(1), CancellationToken.None);

        var expected = PatientBillingReader.ReadAccount(
            db, db.Patients.Single(p => p.Id.Value == 1));

        Assert.True(result.IsSuccess);
        Assert.Equal(expected.TotalCharged, result.Value!.TotalCharged);
        Assert.Equal(expected.TotalPaid, result.Value.TotalPaid);
        Assert.Equal(expected.Balance, result.Value.Balance);
        Assert.Equal(150m, result.Value.TotalCharged);
        Assert.Equal(90m, result.Value.TotalPaid);
        Assert.Equal(60m, result.Value.Balance);
    }

    [Theory]
    [InlineData(100, 20, 80, 0)]   // positive balance: remaining to the lab
    [InlineData(100, 100, 0, 0)]   // zero balance: settled
    [InlineData(100, 150, 0, 50)]  // negative balance (credit): remaining to the patient
    public async Task SignDerivationMatrix(decimal charged, decimal paid, decimal toLab, decimal toPatient)
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient());
        AddTest(db, 10);
        db.PatientTests.Add(PatientTest.Create(PatientTestId.Create(100), PatientId.Create(1), TestId.Create(10), charged));
        db.PaymentOperations.Add(MakeOp(1, 1, paid));

        var handler = new GetDeliveryAccountQueryHandler(db);
        var result = await handler.Handle(new GetDeliveryAccountQuery(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(charged - paid, result.Value!.Balance);
        Assert.Equal(toLab, result.Value.RemainingToLab);
        Assert.Equal(toPatient, result.Value.RemainingToPatient);
    }

    [Fact]
    public async Task UnknownPatient_ReturnsNotFound()
    {
        var db = new FakeApplicationDbContext();

        var handler = new GetDeliveryAccountQueryHandler(db);
        var result = await handler.Handle(new GetDeliveryAccountQuery(99), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
        Assert.Equal("المريض غير موجود.", result.Error!.Message);
    }

    [Fact]
    public async Task SoftDeletedPatient_ReturnsNotFound()
    {
        var db = new FakeApplicationDbContext();
        var patient = MakePatient();
        patient.SoftDelete();
        db.Patients.Add(patient);

        var handler = new GetDeliveryAccountQueryHandler(db);
        var result = await handler.Handle(new GetDeliveryAccountQuery(1), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
        Assert.Equal("المريض غير موجود.", result.Error!.Message);
    }
}
