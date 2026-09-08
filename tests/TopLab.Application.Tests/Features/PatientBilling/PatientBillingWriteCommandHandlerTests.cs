using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientBilling.Commands.RecordCorrection;
using TopLab.Application.Features.PatientBilling.Commands.RecordExtraCharge;
using TopLab.Application.Features.PatientBilling.Commands.SettleAccountInFull;
using TopLab.Application.Features.PatientBilling.Commands.VoidPaymentOperation;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Billing;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using Xunit;

namespace TopLab.Application.Tests.Features.PatientBilling;

public class RecordExtraChargeCommandHandlerTests
{
    [Fact]
    public async Task HappyPath_SavesExtraCharge_OnChargedSide()
    {
        var db = new FakeApplicationDbContext();
        var user = new FakeCurrentUserService { UserId = 7 };
        var clock = new FakeDateTimeProvider();
        db.Patients.Add(Patient.Create(PatientId.Create(1), "Ahmed", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow));

        var handler = new RecordExtraChargeCommandHandler(db, user, clock);
        var result = await handler.Handle(new RecordExtraChargeCommand(1, 20m), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var saved = Assert.Single(db.PaymentOperations);
        Assert.True(saved.IsExtraCharge);
        Assert.Equal(OperationType.Payment, saved.OperationType);
        Assert.Equal(20m, saved.Amount);
        Assert.Equal(
            20m,
            PatientAccountCalculator.TotalCharged(new List<decimal>(), db.PaymentOperations.ToList()));
        Assert.Equal(1, db.SaveChangesCallCount);
    }

    [Fact]
    public async Task UnknownPatient_ReturnsNotFound()
    {
        var db = new FakeApplicationDbContext();
        var handler = new RecordExtraChargeCommandHandler(db, new FakeCurrentUserService(), new FakeDateTimeProvider());

        var result = await handler.Handle(new RecordExtraChargeCommand(42, 20m), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
        Assert.Equal("المريض غير موجود.", result.Error!.Message);
    }
}

public class RecordCorrectionCommandHandlerTests
{
    [Fact]
    public async Task HappyPath_SavesCorrection_CreditingPaidSide()
    {
        var db = new FakeApplicationDbContext();
        var user = new FakeCurrentUserService { UserId = 7 };
        var clock = new FakeDateTimeProvider();
        db.Patients.Add(Patient.Create(PatientId.Create(1), "Ahmed", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow));

        var handler = new RecordCorrectionCommandHandler(db, user, clock);
        var result = await handler.Handle(new RecordCorrectionCommand(1, 30m), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var saved = Assert.Single(db.PaymentOperations);
        Assert.Equal(OperationType.Correction, saved.OperationType);
        Assert.False(saved.IsExtraCharge);
        Assert.Equal(
            30m,
            PatientAccountCalculator.TotalPaid(db.PaymentOperations.ToList()));
        Assert.Equal(1, db.SaveChangesCallCount);
    }

    [Fact]
    public async Task SoftDeletedPatient_ReturnsNotFound()
    {
        var db = new FakeApplicationDbContext();
        var patient = Patient.Create(PatientId.Create(1), "Ahmed", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow);
        patient.SoftDelete();
        db.Patients.Add(patient);
        var handler = new RecordCorrectionCommandHandler(db, new FakeCurrentUserService(), new FakeDateTimeProvider());

        var result = await handler.Handle(new RecordCorrectionCommand(1, 30m), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
    }
}

public class SettleAccountInFullCommandHandlerTests
{
    private static void SeedAccount(FakeApplicationDbContext db)
    {
        db.Patients.Add(Patient.Create(PatientId.Create(1), "Ahmed", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow));
        db.PatientTests.Add(PatientTest.Create(PatientTestId.Create(100), PatientId.Create(1), TestId.Create(10), 100m));
        db.PaymentOperations.Add(PaymentOperation.Create(
            PaymentOperationId.Create(5), PatientId.Create(1), 40m, 7, DateTime.UtcNow));
    }

    [Fact]
    public async Task HappyPath_SettlementAmountEqualsComputedBalance_AndNetsToZero()
    {
        var db = new FakeApplicationDbContext();
        SeedAccount(db);
        var handler = new SettleAccountInFullCommandHandler(db, new FakeCurrentUserService(), new FakeDateTimeProvider());

        var result = await handler.Handle(new SettleAccountInFullCommand(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, db.PaymentOperations.Count);
        var settlement = db.PaymentOperations.Last();
        Assert.Equal(OperationType.FullSettlement, settlement.OperationType);
        Assert.Equal(60m, settlement.Amount);
        Assert.Equal(
            0m,
            PatientAccountCalculator.Balance(
                new List<decimal> { 100m },
                db.PaymentOperations.ToList()));
    }

    [Fact]
    public async Task ZeroBalance_ReturnsConflict()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(Patient.Create(PatientId.Create(1), "Ahmed", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow));
        var handler = new SettleAccountInFullCommandHandler(db, new FakeCurrentUserService(), new FakeDateTimeProvider());

        var result = await handler.Handle(new SettleAccountInFullCommand(1), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
        Assert.Equal("لا يوجد رصيد مستحق للتسوية.", result.Error!.Message);
    }

    [Fact]
    public async Task CreditBalance_ReturnsConflict()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(Patient.Create(PatientId.Create(1), "Ahmed", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow));
        db.PatientTests.Add(PatientTest.Create(PatientTestId.Create(100), PatientId.Create(1), TestId.Create(10), 100m));
        db.PaymentOperations.Add(PaymentOperation.Create(
            PaymentOperationId.Create(5), PatientId.Create(1), 150m, 7, DateTime.UtcNow));
        var handler = new SettleAccountInFullCommandHandler(db, new FakeCurrentUserService(), new FakeDateTimeProvider());

        var result = await handler.Handle(new SettleAccountInFullCommand(1), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
        Assert.Equal("لا يوجد رصيد مستحق للتسوية.", result.Error!.Message);
    }

    [Fact]
    public async Task UnknownPatient_ReturnsNotFound()
    {
        var db = new FakeApplicationDbContext();
        var handler = new SettleAccountInFullCommandHandler(db, new FakeCurrentUserService(), new FakeDateTimeProvider());

        var result = await handler.Handle(new SettleAccountInFullCommand(9), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
    }
}

public class VoidPaymentOperationCommandHandlerTests
{
    private static FakeApplicationDbContext SeedWithPayment()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(Patient.Create(PatientId.Create(1), "Ahmed", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow));
        db.PaymentOperations.Add(PaymentOperation.Create(
            PaymentOperationId.Create(11), PatientId.Create(1), 200m, 7, DateTime.UtcNow));
        return db;
    }

    [Fact]
    public async Task HappyPath_VoidsRow_ExcludingItFromTotals()
    {
        var db = SeedWithPayment();
        var handler = new VoidPaymentOperationCommandHandler(db);

        var result = await handler.Handle(new VoidPaymentOperationCommand(11), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(db.PaymentOperations.Single().IsVoided);
        Assert.Equal(0m, PatientAccountCalculator.TotalPaid(db.PaymentOperations.ToList()));
        Assert.Equal(1, db.SaveChangesCallCount);
    }

    [Fact]
    public async Task UnknownOperation_ReturnsNotFound()
    {
        var db = new FakeApplicationDbContext();
        var handler = new VoidPaymentOperationCommandHandler(db);

        var result = await handler.Handle(new VoidPaymentOperationCommand(404), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
        Assert.Equal("العملية غير موجودة.", result.Error!.Message);
    }

    [Fact]
    public async Task DoubleVoid_ReturnsConflict_WithFriendlyMessage()
    {
        var db = SeedWithPayment();
        var handler = new VoidPaymentOperationCommandHandler(db);
        var first = await handler.Handle(new VoidPaymentOperationCommand(11), CancellationToken.None);
        Assert.True(first.IsSuccess);

        var second = await handler.Handle(new VoidPaymentOperationCommand(11), CancellationToken.None);

        Assert.False(second.IsSuccess);
        Assert.Equal(ErrorType.Conflict, second.Error!.Type);
        Assert.Equal("العملية ملغاة بالفعل.", second.Error!.Message);
    }
}
