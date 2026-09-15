using TopLab.Application.Features.InventoryAndAccounting.Queries.GetCashDrawerInventory;
using TopLab.Application.Features.InventoryAndAccounting.Queries.GetElementInventory;
using TopLab.Application.Features.InventoryAndAccounting.Queries.GetPatientSamplesDetail;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Accounting;
using TopLab.Domain.Billing;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.ExternalEntities;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.SentOutSamples;
using Xunit;

namespace TopLab.Application.Tests.Features.InventoryAndAccounting;

public class GetCashDrawerInventoryQueryHandlerTests
{
    private static readonly DateTime Day1 = new(2026, 3, 1, 10, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Day15 = new(2026, 3, 15, 9, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Day20 = new(2026, 3, 20, 11, 0, 0, DateTimeKind.Utc);

    private static FakeApplicationDbContext NewDb() => new();

    private static GetCashDrawerInventoryQueryHandler Handler(FakeApplicationDbContext db, DateTime? utcNow = null)
    {
        var clock = new FakeDateTimeProvider();
        if (utcNow.HasValue)
        {
            clock.UtcNow = utcNow.Value;
        }

        return new GetCashDrawerInventoryQueryHandler(db, clock);
    }

    private static Patient MakePatient(int id, int? referralEntityId = null, bool deleted = false)
    {
        var p = Patient.Create(
            PatientId.Create(id),
            $"Patient {id}",
            Sex.Male,
            30,
            AgeUnit.Year,
            Day1,
            referralEntityId: referralEntityId.HasValue ? ExternalEntityId.Create(referralEntityId.Value) : null);
        if (deleted)
        {
            p.SoftDelete();
        }

        return p;
    }

    private static PatientTest MakeOrder(int id, int patientId, decimal price, DateTime atUtc, int? enteredByUserId = 1)
    {
        var pt = PatientTest.Create(
            PatientTestId.Create(id),
            PatientId.Create(patientId),
            TestId.Create(id),
            price);
        pt.CreatedAtUtc = atUtc;
        if (enteredByUserId.HasValue)
        {
            pt.EnterResult(price.ToString("F2"), null, enteredByUserId.Value, atUtc);
        }

        return pt;
    }

    private static PaymentOperation MakeOp(
        int id,
        int patientId,
        decimal amount,
        decimal? discount = null,
        bool extraCharge = false,
        bool voided = false,
        DateTime? atUtc = null)
    {
        var op = PaymentOperation.Create(
            PaymentOperationId.Create(id),
            PatientId.Create(patientId),
            amount,
            receivedByUserId: 1,
            atUtc ?? Day15,
            discount,
            extraCharge);
        if (voided)
        {
            op.Void();
        }

        return op;
    }

    private static CashMovement MakeCash(int id, MovementType type, decimal amount, DateTime atUtc)
    {
        return CashMovement.Create(
            CashMovementId.Create(id),
            type,
            amount,
            performedByUserId: 1,
            atUtc);
    }

    private static SentOutSample MakeSentOut(int id, decimal cost, DateTime atUtc)
    {
        return SentOutSample.Create(
            SentOutSampleId.Create(id),
            PatientTestId.Create(id),
            ExternalEntityId.Create(50),
            cost,
            cost + 10m,
            atUtc);
    }

    private static SentOutSamplePayment MakeSentOutPayment(int id, int sampleId, decimal amount)
    {
        return SentOutSamplePayment.Create(
            SentOutSamplePaymentId.Create(id),
            SentOutSampleId.Create(sampleId),
            amount,
            Day20,
            1);
    }

    private static ExternalEntity MakeEntity(int id, string name, decimal? percent = null)
    {
        return ExternalEntity.Create(
            ExternalEntityId.Create(id),
            EntityType.TreatingDoctor,
            name,
            discountOrCommissionPercent: percent);
    }

    [Fact]
    public async Task WorkedExample_EveryFigure_NumberForNumber()
    {
        var db = NewDb();
        db.ExternalEntities.Add(MakeEntity(10, "د. خالد", percent: 10m));
        db.Patients.Add(MakePatient(1, referralEntityId: 10));
        db.PatientTests.Add(MakeOrder(1, 1, 100m, Day1));
        db.PatientTests.Add(MakeOrder(2, 1, 50m, Day1));
        db.PaymentOperations.Add(MakeOp(1, 1, 80m, discount: 10m));
        db.PaymentOperations.Add(MakeOp(2, 1, 20m, extraCharge: true));
        db.PaymentOperations.Add(MakeOp(3, 1, 999m, voided: true));
        db.CashMovements.Add(MakeCash(1, MovementType.Deposit, 200m, Day15));
        db.CashMovements.Add(MakeCash(2, MovementType.Disbursement, 30m, Day20));
        db.SentOutSamples.Add(MakeSentOut(1, 60m, Day15));
        db.SentOutSamplePayments.Add(MakeSentOutPayment(1, 1, 25m));

        var result = await Handler(db).Handle(
            new GetCashDrawerInventoryQuery(new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31)),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        var dto = result.Value!;
        Assert.Equal(2, dto.TotalSamplesCount);
        Assert.Equal(150m, dto.TotalSamplesAmount);
        Assert.Equal(10m, dto.DiscountsValue);
        // TotalCharged = 150 + extra 20 = 170; TotalAfterDiscount = 170 - 10 = 160
        Assert.Equal(160m, dto.TotalAfterDiscount);
        // Collected = Amount of non-voided non-extra-charge = 80
        Assert.Equal(80m, dto.CollectedAmount);
        // Uncollected = Balance = 170 - (80+10) = 80
        Assert.Equal(80m, dto.UncollectedAmount);
        Assert.Equal(200m, dto.CashSupplies);
        Assert.Equal(30m, dto.Disbursements);
        // SafeCash = Collected + Deposits - Disbursements = 80 + 200 - 30 = 250
        Assert.Equal(250m, dto.SafeCash);
        Assert.Equal(1, dto.SentOutCount);
        Assert.Equal(60m, dto.SentOutTotalCost);
        Assert.Equal(25m, dto.SentOutTotalPaid);
        Assert.Equal(35m, dto.SentOutRemaining);
        var commission = dto.CommissionsAndShares.Single();
        Assert.Equal(10, commission.EntityId);
        Assert.Equal(10m, commission.Percent);
        Assert.Equal(150m, commission.ReferredChargeBase);
        Assert.Equal(15m, commission.CommissionAmount);
        Assert.Equal(80m, dto.RemainingToLab);
        // NetProfit = Collected - SentOutPaid - Disbursements = 80 - 25 - 30 = 25
        Assert.Equal(25m, dto.NetProfit);
    }

    [Fact]
    public async Task SoftDeletedPatients_AreExcluded()
    {
        var db = NewDb();
        db.Patients.Add(MakePatient(1));
        db.Patients.Add(MakePatient(2, deleted: true));
        db.PatientTests.Add(MakeOrder(1, 1, 50m, Day1));
        db.PatientTests.Add(MakeOrder(2, 2, 999m, Day1));

        var result = await Handler(db).Handle(
            new GetCashDrawerInventoryQuery(new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31)),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value!.TotalSamplesCount);
        Assert.Equal(50m, result.Value.TotalSamplesAmount);
    }

    [Fact]
    public async Task EmptyPeriod_ReturnsZeros()
    {
        var db = NewDb();
        db.Patients.Add(MakePatient(1));
        db.PatientTests.Add(MakeOrder(1, 1, 50m, Day1));

        var result = await Handler(db).Handle(
            new GetCashDrawerInventoryQuery(new DateOnly(2026, 5, 1), new DateOnly(2026, 5, 31)),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value!.TotalSamplesCount);
        Assert.Equal(0m, result.Value.SafeCash);
        Assert.Empty(result.Value.CommissionsAndShares);
    }

    [Fact]
    public async Task DefaultPeriod_IsTodayUtc()
    {
        var db = NewDb();
        var today = new DateTime(2026, 6, 15, 14, 0, 0, DateTimeKind.Utc);
        var patient = Patient.Create(
            PatientId.Create(1),
            "Today",
            Sex.Male,
            30,
            AgeUnit.Year,
            new DateTime(2026, 6, 15, 1, 0, 0, DateTimeKind.Utc));
        db.Patients.Add(patient);
        db.PatientTests.Add(MakeOrder(1, 1, 50m, new DateTime(2026, 6, 15, 1, 0, 0, DateTimeKind.Utc)));
        db.PatientTests.Add(MakeOrder(2, 1, 50m, new DateTime(2026, 6, 14, 23, 0, 0, DateTimeKind.Utc)));

        var result = await Handler(db, today).Handle(
            new GetCashDrawerInventoryQuery(null, null),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(new DateOnly(2026, 6, 15), result.Value!.From);
        Assert.Equal(1, result.Value.TotalSamplesCount);
    }

    [Fact]
    public void Validator_RejectsInvertedPeriod_WithFrozenMessage()
    {
        var validator = new GetCashDrawerInventoryQueryValidator();
        var outcome = validator.Validate(new GetCashDrawerInventoryQuery(
            new DateOnly(2026, 3, 10),
            new DateOnly(2026, 3, 1)));

        Assert.False(outcome.IsValid);
        Assert.Contains(outcome.Errors, e => e.ErrorMessage == "بداية الفترة يجب ألا تتجاوز نهايتها.");
    }
}
