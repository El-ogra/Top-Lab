using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientBilling.Commands.RecordRefund;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Billing;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using Xunit;

namespace TopLab.Application.Tests.Features.PatientBilling;

/// <summary>
/// R1 Round 3 B4: the refund handler (canonical reference 1.3.2.5, «استرداد»).
/// The refund is a new <c>OperationType.Refund</c> row carrying <c>RefundAmount</c> with
/// <c>Amount = 0</c>; nothing stored is overwritten (OD-12 option b).
/// </summary>
public class RecordRefundCommandHandlerTests
{
    private const int PatientIdValue = 1;
    private const int UserIdValue = 7;

    private static FakeApplicationDbContext SeedPatient(int id = PatientIdValue, bool deleted = false)
    {
        var db = new FakeApplicationDbContext();
        var patient = Patient.Create(PatientId.Create(id), "Ahmed", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow);
        if (deleted)
        {
            patient.SoftDelete();
        }

        db.Patients.Add(patient);
        return db;
    }

    private static void SeedPayment(FakeApplicationDbContext db, decimal amount, int patientId = PatientIdValue, bool voided = false)
    {
        var operation = PaymentOperation.Create(
            PaymentOperationId.Create(0), PatientId.Create(patientId), amount, UserIdValue, DateTime.UtcNow);
        if (voided)
        {
            operation.Void();
        }

        db.PaymentOperations.Add(operation);
    }

    private static void SeedRefund(FakeApplicationDbContext db, decimal amount, int patientId = PatientIdValue)
    {
        db.PaymentOperations.Add(PaymentOperation.Create(
            PaymentOperationId.Create(0), PatientId.Create(patientId), 0m, UserIdValue, DateTime.UtcNow,
            null, false, OperationType.Refund, amount));
    }

    private static RecordRefundCommandHandler Handler(FakeApplicationDbContext db)
        => new(db, new FakeCurrentUserService { UserId = UserIdValue }, new FakeDateTimeProvider());

    [Fact]
    public async Task Success_RecordsARefundWithAmountZeroAndTheRefundAmount()
    {
        var db = SeedPatient();
        SeedPayment(db, 100m);

        var result = await Handler(db).Handle(new RecordRefundCommand(PatientIdValue, 40m), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var refund = Assert.Single(db.PaymentOperations, o => o.OperationType == OperationType.Refund);
        Assert.Equal(0m, refund.Amount);
        Assert.Equal(40m, refund.RefundAmount);
        Assert.Equal(OperationType.Refund, refund.OperationType);
        Assert.Equal(UserIdValue, refund.ReceivedByUserId);
        Assert.Equal(1, db.SaveChangesCallCount);
    }

    [Fact]
    public async Task RefundExactlyEqualToNetPaid_IsAccepted()
    {
        var db = SeedPatient();
        SeedPayment(db, 100m);

        var result = await Handler(db).Handle(new RecordRefundCommand(PatientIdValue, 100m), CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task OverRefund_IsRejected_WithTheNetPaidInTheMessage()
    {
        var db = SeedPatient();
        SeedPayment(db, 100m);

        var result = await Handler(db).Handle(new RecordRefundCommand(PatientIdValue, 100.01m), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error!.Type);
        Assert.Contains("100", result.Error.Message, StringComparison.Ordinal);
        Assert.DoesNotContain(db.PaymentOperations, o => o.OperationType == OperationType.Refund);
    }

    [Fact]
    public async Task EarlierRefunds_ReduceWhatMayStillBeRefunded()
    {
        var db = SeedPatient();
        SeedPayment(db, 100m);
        SeedRefund(db, 60m);

        // Net paid is 40, so 41 must be refused and 40 accepted.
        Assert.False((await Handler(db).Handle(new RecordRefundCommand(PatientIdValue, 41m), CancellationToken.None)).IsSuccess);
        Assert.True((await Handler(db).Handle(new RecordRefundCommand(PatientIdValue, 40m), CancellationToken.None)).IsSuccess);
    }

    [Fact]
    public async Task VoidedOperations_AreIgnoredWhenComputingTheCeiling()
    {
        var db = SeedPatient();
        SeedPayment(db, 100m);
        SeedPayment(db, 500m, voided: true);

        // Only the 100 survives, so 150 is refused.
        var result = await Handler(db).Handle(new RecordRefundCommand(PatientIdValue, 150m), CancellationToken.None);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task RefundWithNothingPaid_IsRejected()
    {
        var db = SeedPatient();

        var result = await Handler(db).Handle(new RecordRefundCommand(PatientIdValue, 1m), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error!.Type);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task ZeroOrNegativeAmount_IsRejectedByTheDomain(decimal amount)
    {
        var db = SeedPatient();
        SeedPayment(db, 100m);

        var result = await Handler(db).Handle(new RecordRefundCommand(PatientIdValue, amount), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error!.Type);
    }

    [Fact]
    public async Task UnknownPatient_IsNotFound()
    {
        var db = SeedPatient();

        var result = await Handler(db).Handle(new RecordRefundCommand(999, 10m), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
        Assert.Equal("المريض غير موجود.", result.Error.Message);
    }

    [Fact]
    public async Task SoftDeletedPatient_IsNotFound()
    {
        var db = SeedPatient(deleted: true);

        var result = await Handler(db).Handle(new RecordRefundCommand(PatientIdValue, 10m), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
    }

    [Fact]
    public async Task OtherPatientsOperations_DoNotRaiseThisPatientsCeiling()
    {
        var db = SeedPatient();
        SeedPayment(db, 50m);
        SeedPayment(db, 1000m, patientId: 2);

        var result = await Handler(db).Handle(new RecordRefundCommand(PatientIdValue, 60m), CancellationToken.None);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void RecordRefund_IsNotGated_TheReferenceAddsNoPermissionCode()
    {
        Assert.IsNotAssignableFrom<TopLab.Application.Common.Authorization.IAuthorizedRequest>(
            new RecordRefundCommand(1, 10m));
    }

    [Fact]
    public void BalanceFormula_ReconcilesWithRefundsVoidsDiscountsAndSurchargesTogether()
    {
        var db = SeedPatient();
        var prices = new List<decimal> { 200m };

        // Payment of 100 with a 20 discount, a surcharge of 30, a voided payment of 500,
        // and a refund of 40.
        var payment = PaymentOperation.Create(
            PaymentOperationId.Create(1), PatientId.Create(PatientIdValue), 100m, UserIdValue, DateTime.UtcNow, 20m);
        var surcharge = PaymentOperation.Create(
            PaymentOperationId.Create(2), PatientId.Create(PatientIdValue), 30m, UserIdValue, DateTime.UtcNow, null, true);
        var voided = PaymentOperation.Create(
            PaymentOperationId.Create(3), PatientId.Create(PatientIdValue), 500m, UserIdValue, DateTime.UtcNow);
        voided.Void();
        var refund = PaymentOperation.Create(
            PaymentOperationId.Create(4), PatientId.Create(PatientIdValue), 0m, UserIdValue, DateTime.UtcNow,
            null, false, OperationType.Refund, 40m);

        var operations = new List<PaymentOperation> { payment, surcharge, voided, refund };

        // Charged = 200 + 30 = 230; Paid = (100 + 20) - 40 = 80; Balance = 150.
        Assert.Equal(230m, PatientAccountCalculator.TotalCharged(prices, operations));
        Assert.Equal(80m, PatientAccountCalculator.TotalPaid(operations));
        Assert.Equal(150m, PatientAccountCalculator.Balance(prices, operations));

        // The same figures come out of the read model the screens use.
        var account = TopLab.Application.Features.PatientBilling.Queries.GetPatientAccount.PatientBillingReader.ReadAccount(db, db.Patients[0]);
        Assert.Equal(0m, account.TotalPaid);

        db.PaymentOperations.AddRange(operations);
        db.PatientTests.Add(TopLab.Domain.Results.PatientTest.Create(
            TopLab.Domain.Common.Ids.PatientTestId.Create(1), PatientId.Create(PatientIdValue),
            TopLab.Domain.Common.Ids.TestId.Create(1), 200m));

        var loaded = TopLab.Application.Features.PatientBilling.Queries.GetPatientAccount.PatientBillingReader.ReadAccount(db, db.Patients[0]);
        Assert.Equal(230m, loaded.TotalCharged);
        Assert.Equal(80m, loaded.TotalPaid);
        Assert.Equal(150m, loaded.Balance);
    }

    // ================================================================
    // R1 Round 5 Part B — the ceiling is net CASH, not net "paid including discount"
    // ================================================================

    [Fact]
    public async Task Discount_DoesNotRaiseTheCeiling()
    {
        var db = SeedPatient();
        db.PaymentOperations.Add(PaymentOperation.Create(
            PaymentOperationId.Create(0), PatientId.Create(PatientIdValue), 800m, UserIdValue,
            DateTime.UtcNow, 200m));

        // Cash handed over is 800; TotalPaid is 1000 because the 200 discount counts there.
        var accepted = await Handler(db).Handle(new RecordRefundCommand(PatientIdValue, 800m), CancellationToken.None);
        Assert.True(accepted.IsSuccess);

        var refused = await Handler(db).Handle(new RecordRefundCommand(PatientIdValue, 800.01m), CancellationToken.None);
        Assert.False(refused.IsSuccess);
        Assert.Equal(ErrorType.Validation, refused.Error!.Type);
    }

    [Fact]
    public async Task OverRefundMessage_QuotesTheCashCeiling()
    {
        var db = SeedPatient();
        db.PaymentOperations.Add(PaymentOperation.Create(
            PaymentOperationId.Create(0), PatientId.Create(PatientIdValue), 800m, UserIdValue,
            DateTime.UtcNow, 200m));

        var result = await Handler(db).Handle(new RecordRefundCommand(PatientIdValue, 900m), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("800", result.Error!.Message, StringComparison.Ordinal);
        Assert.Contains("النقدي", result.Error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Surcharge_DoesNotRaiseTheCeiling()
    {
        var db = SeedPatient();
        SeedPayment(db, 500m);
        db.PaymentOperations.Add(PaymentOperation.Create(
            PaymentOperationId.Create(0), PatientId.Create(PatientIdValue), 300m, UserIdValue,
            DateTime.UtcNow, null, true));

        Assert.True((await Handler(db).Handle(new RecordRefundCommand(PatientIdValue, 500m), CancellationToken.None)).IsSuccess);
        Assert.False((await Handler(db).Handle(new RecordRefundCommand(PatientIdValue, 501m), CancellationToken.None)).IsSuccess);
    }

    [Fact]
    public async Task Correction_CountsAsPaidCash()
    {
        var db = SeedPatient();
        SeedPayment(db, 500m);
        db.PaymentOperations.Add(PaymentOperation.Create(
            PaymentOperationId.Create(0), PatientId.Create(PatientIdValue), 50m, UserIdValue,
            DateTime.UtcNow, null, false, OperationType.Correction));

        Assert.True((await Handler(db).Handle(new RecordRefundCommand(PatientIdValue, 550m), CancellationToken.None)).IsSuccess);
    }

    [Fact]
    public async Task Mixed_RefundIsCappedByCashAfterDiscountsVoidsAndEarlierRefunds()
    {
        var db = SeedPatient();
        db.PaymentOperations.Add(PaymentOperation.Create(
            PaymentOperationId.Create(0), PatientId.Create(PatientIdValue), 800m, UserIdValue,
            DateTime.UtcNow, 200m));
        SeedRefund(db, 250m);
        SeedPayment(db, 400m, voided: true);

        // Cash = 800 - 250 = 550.
        Assert.True((await Handler(db).Handle(new RecordRefundCommand(PatientIdValue, 550m), CancellationToken.None)).IsSuccess);
        Assert.False((await Handler(db).Handle(new RecordRefundCommand(PatientIdValue, 550.01m), CancellationToken.None)).IsSuccess);
    }
}
