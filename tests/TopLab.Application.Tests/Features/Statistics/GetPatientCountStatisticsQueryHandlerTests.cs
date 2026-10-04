using TopLab.Application.Features.Statistics.Queries.GetPatientCountStatistics;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Billing;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.ExternalEntities;
using TopLab.Domain.Patients;
using Xunit;

namespace TopLab.Application.Tests.Features.Statistics;

public class GetPatientCountStatisticsQueryHandlerTests
{
    private static readonly DateTime Day1 = new(2026, 3, 1, 10, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Day15 = new(2026, 3, 15, 8, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Day20 = new(2026, 3, 20, 12, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime April10 = new(2026, 4, 10, 9, 0, 0, DateTimeKind.Utc);

    private static FakeApplicationDbContext NewDb() => new();

    private static GetPatientCountStatisticsQueryHandler Handler(FakeApplicationDbContext db, DateTime? utcNow = null)
    {
        var clock = new FakeDateTimeProvider();
        if (utcNow.HasValue)
        {
            clock.UtcNow = utcNow.Value;
        }

        return new GetPatientCountStatisticsQueryHandler(db, clock);
    }

    private static Patient MakePatient(
        int id,
        Sex sex,
        DateTime registered,
        AccountType accountType = AccountType.Individual,
        int? referralEntityId = null)
    {
        return Patient.Create(
            PatientId.Create(id),
            $"Patient {id}",
            sex,
            30,
            AgeUnit.Year,
            registered,
            accountType,
            false,
            null,
            null,
            null,
            null,
            null,
            referralEntityId.HasValue ? ExternalEntityId.Create(referralEntityId.Value) : null);
    }

    private static ExternalEntity MakeReferral(int id, string name)
    {
        // TreatingDoctor avoids the ReferralOrContract PriceListId requirement;
        // dictionary name-resolution only needs a row with the id.
        return ExternalEntity.Create(
            ExternalEntityId.Create(id),
            EntityType.TreatingDoctor,
            name);
    }

    [Fact]
    public async Task SexClassification_NumberForNumber()
    {
        var db = NewDb();
        db.Patients.Add(MakePatient(1, Sex.Male, Day1));
        db.Patients.Add(MakePatient(2, Sex.Male, Day15));
        db.Patients.Add(MakePatient(3, Sex.Female, Day20));

        var query = new GetPatientCountStatisticsQuery(
            new DateOnly(2026, 3, 1),
            new DateOnly(2026, 3, 31),
            BySex: true,
            ByReferralEntity: false,
            ByAccountType: false,
            GroupByMonth: false,
            GroupByDayOfMonth: false,
            IncludeMoneyRow: false);

        var result = await Handler(db).Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value!.TotalCount);
        var male = result.Value.SexCounts.Single(c => c.Key == ((int)Sex.Male).ToString());
        var female = result.Value.SexCounts.Single(c => c.Key == ((int)Sex.Female).ToString());
        Assert.Equal(2, male.Count);
        Assert.Equal(1, female.Count);
        Assert.Empty(result.Value.ReferralEntityCounts);
        Assert.Empty(result.Value.AccountTypeCounts);
        Assert.Empty(result.Value.MonthlyCounts);
    }

    [Fact]
    public async Task AccountTypeClassification_NumberForNumber()
    {
        var db = NewDb();
        db.Patients.Add(MakePatient(1, Sex.Male, Day1, AccountType.Individual));
        db.Patients.Add(MakePatient(2, Sex.Female, Day1, AccountType.Vip));
        db.Patients.Add(MakePatient(3, Sex.Male, Day1, AccountType.Vip));
        db.Patients.Add(MakePatient(4, Sex.Male, Day1, AccountType.LabToLab));

        var query = new GetPatientCountStatisticsQuery(
            new DateOnly(2026, 3, 1),
            new DateOnly(2026, 3, 31),
            BySex: false,
            ByReferralEntity: false,
            ByAccountType: true,
            GroupByMonth: false,
            GroupByDayOfMonth: false,
            IncludeMoneyRow: false);

        var result = await Handler(db).Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(4, result.Value!.TotalCount);
        Assert.Equal(1, result.Value.AccountTypeCounts.Single(c => c.Key == ((int)AccountType.Individual).ToString()).Count);
        Assert.Equal(2, result.Value.AccountTypeCounts.Single(c => c.Key == ((int)AccountType.Vip).ToString()).Count);
        Assert.Equal(1, result.Value.AccountTypeCounts.Single(c => c.Key == ((int)AccountType.LabToLab).ToString()).Count);
        Assert.Empty(result.Value.SexCounts);
    }

    [Fact]
    public async Task ReferralEntityClassification_ResolvesNames_AndNullBucket()
    {
        var db = NewDb();
        db.ExternalEntities.Add(MakeReferral(10, "مستشفى النور"));
        db.Patients.Add(MakePatient(1, Sex.Male, Day1, referralEntityId: 10));
        db.Patients.Add(MakePatient(2, Sex.Female, Day1, referralEntityId: 10));
        db.Patients.Add(MakePatient(3, Sex.Male, Day1, referralEntityId: null));

        var query = new GetPatientCountStatisticsQuery(
            new DateOnly(2026, 3, 1),
            new DateOnly(2026, 3, 31),
            BySex: false,
            ByReferralEntity: true,
            ByAccountType: false,
            GroupByMonth: false,
            GroupByDayOfMonth: false,
            IncludeMoneyRow: false);

        var result = await Handler(db).Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value!.TotalCount);

        var named = result.Value.ReferralEntityCounts.Single(c => c.Key == "10");
        Assert.Equal("مستشفى النور", named.DisplayName);
        Assert.Equal(2, named.Count);

        var none = result.Value.ReferralEntityCounts.Single(c => c.Key == string.Empty);
        Assert.Equal("بدون جهة إحالة", none.DisplayName);
        Assert.Equal(1, none.Count);
    }

    [Fact]
    public async Task ReferralEntity_UnknownId_FallsBackToRawIdString()
    {
        var db = NewDb();
        db.Patients.Add(MakePatient(1, Sex.Male, Day1, referralEntityId: 99));

        var query = new GetPatientCountStatisticsQuery(
            new DateOnly(2026, 3, 1),
            new DateOnly(2026, 3, 31),
            BySex: false,
            ByReferralEntity: true,
            ByAccountType: false,
            GroupByMonth: false,
            GroupByDayOfMonth: false,
            IncludeMoneyRow: false);

        var result = await Handler(db).Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var bucket = result.Value!.ReferralEntityCounts.Single(c => c.Key == "99");
        Assert.Equal("99", bucket.DisplayName);
        Assert.Equal(1, bucket.Count);
    }

    [Fact]
    public async Task MonthlyBreakdown_BucketsByUtcYearMonth()
    {
        var db = NewDb();
        db.Patients.Add(MakePatient(1, Sex.Male, Day15));
        db.Patients.Add(MakePatient(2, Sex.Female, Day20));
        db.Patients.Add(MakePatient(3, Sex.Male, April10));

        var query = new GetPatientCountStatisticsQuery(
            new DateOnly(2026, 3, 1),
            new DateOnly(2026, 4, 30),
            BySex: false,
            ByReferralEntity: false,
            ByAccountType: false,
            GroupByMonth: true,
            GroupByDayOfMonth: false,
            IncludeMoneyRow: false);

        var result = await Handler(db).Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value!.TotalCount);
        Assert.Equal(2, result.Value.MonthlyCounts.Single(m => m.Year == 2026 && m.Month == 3).Count);
        Assert.Equal(1, result.Value.MonthlyCounts.Single(m => m.Year == 2026 && m.Month == 4).Count);
        Assert.Empty(result.Value.MonthlySexCounts);
    }

    [Fact]
    public async Task MonthlyAndSex_CrossBuckets_AreProduced()
    {
        var db = NewDb();
        db.Patients.Add(MakePatient(1, Sex.Male, Day1));
        db.Patients.Add(MakePatient(2, Sex.Female, Day15));
        db.Patients.Add(MakePatient(3, Sex.Male, April10));

        var query = new GetPatientCountStatisticsQuery(
            new DateOnly(2026, 3, 1),
            new DateOnly(2026, 4, 30),
            BySex: true,
            ByReferralEntity: false,
            ByAccountType: false,
            GroupByMonth: true,
            GroupByDayOfMonth: false,
            IncludeMoneyRow: false);

        var result = await Handler(db).Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var marchMale = result.Value!.MonthlySexCounts
            .Single(c => c.Year == 2026 && c.Month == 3 && c.Key == ((int)Sex.Male).ToString());
        var marchFemale = result.Value.MonthlySexCounts
            .Single(c => c.Year == 2026 && c.Month == 3 && c.Key == ((int)Sex.Female).ToString());
        var aprilMale = result.Value.MonthlySexCounts
            .Single(c => c.Year == 2026 && c.Month == 4 && c.Key == ((int)Sex.Male).ToString());

        Assert.Equal(1, marchMale.Count);
        Assert.Equal(1, marchFemale.Count);
        Assert.Equal(1, aprilMale.Count);
    }

    [Fact]
    public async Task SoftDeletedPatients_AreExcluded()
    {
        var db = NewDb();
        var live = MakePatient(1, Sex.Male, Day1);
        var deleted = MakePatient(2, Sex.Female, Day1);
        deleted.SoftDelete();
        db.Patients.Add(live);
        db.Patients.Add(deleted);

        var query = new GetPatientCountStatisticsQuery(
            new DateOnly(2026, 3, 1),
            new DateOnly(2026, 3, 31),
            BySex: true,
            ByReferralEntity: false,
            ByAccountType: false,
            GroupByMonth: false,
            GroupByDayOfMonth: false,
            IncludeMoneyRow: false);

        var result = await Handler(db).Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value!.TotalCount);
        Assert.Equal(1, result.Value.SexCounts.Single(c => c.Key == ((int)Sex.Male).ToString()).Count);
        Assert.DoesNotContain(result.Value.SexCounts, c => c.Key == ((int)Sex.Female).ToString());
    }

    [Fact]
    public async Task EmptyPeriod_ReturnsZeros_NotError()
    {
        var db = NewDb();
        db.Patients.Add(MakePatient(1, Sex.Male, Day1));

        var query = new GetPatientCountStatisticsQuery(
            new DateOnly(2026, 5, 1),
            new DateOnly(2026, 5, 31),
            BySex: true,
            ByReferralEntity: true,
            ByAccountType: true,
            GroupByMonth: true,
            GroupByDayOfMonth: false,
            IncludeMoneyRow: false);

        var result = await Handler(db).Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value!.TotalCount);
        Assert.Empty(result.Value.SexCounts);
        Assert.Empty(result.Value.ReferralEntityCounts);
        Assert.Empty(result.Value.AccountTypeCounts);
        Assert.Empty(result.Value.MonthlyCounts);
    }

    [Fact]
    public async Task DefaultPeriod_IsTodayUtc_WhenOmitted()
    {
        var db = NewDb();
        var today = new DateTime(2026, 6, 15, 14, 30, 0, DateTimeKind.Utc);
        db.Patients.Add(MakePatient(1, Sex.Male, new DateTime(2026, 6, 15, 1, 0, 0, DateTimeKind.Utc)));
        db.Patients.Add(MakePatient(2, Sex.Female, new DateTime(2026, 6, 14, 23, 0, 0, DateTimeKind.Utc)));

        var query = new GetPatientCountStatisticsQuery(
            null,
            null,
            BySex: false,
            ByReferralEntity: false,
            ByAccountType: false,
            GroupByMonth: false,
            GroupByDayOfMonth: false,
            IncludeMoneyRow: false);

        var result = await Handler(db, today).Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(new DateOnly(2026, 6, 15), result.Value!.From);
        Assert.Equal(new DateOnly(2026, 6, 15), result.Value.To);
        Assert.Equal(1, result.Value.TotalCount);
    }

    [Fact]
    public void Validator_RejectsInvertedPeriod_WithFrozenMessage()
    {
        var validator = new GetPatientCountStatisticsQueryValidator();
        var query = new GetPatientCountStatisticsQuery(
            new DateOnly(2026, 3, 10),
            new DateOnly(2026, 3, 1),
            BySex: true,
            ByReferralEntity: false,
            ByAccountType: false,
            GroupByMonth: false,
            GroupByDayOfMonth: false,
            IncludeMoneyRow: false);

        var outcome = validator.Validate(query);

        Assert.False(outcome.IsValid);
        Assert.Contains(outcome.Errors, e => e.ErrorMessage == "بداية الفترة يجب ألا تتجاوز نهايتها.");
    }

    [Fact]
    public void Validator_AllowsEqualAndOpenPeriods()
    {
        var validator = new GetPatientCountStatisticsQueryValidator();

        var equal = validator.Validate(new GetPatientCountStatisticsQuery(
            new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 1), true, false, false, false, false, false));
        var open = validator.Validate(new GetPatientCountStatisticsQuery(
            null, new DateOnly(2026, 3, 1), true, false, false, false, false, false));
        var bothNull = validator.Validate(new GetPatientCountStatisticsQuery(
            null, null, true, false, false, false, false, false));

        Assert.True(equal.IsValid);
        Assert.True(open.IsValid);
        Assert.True(bothNull.IsValid);
    }

    [Fact]
    public async Task PeriodBounds_AreInclusiveCalendarDays_HalfOpenUtc()
    {
        var db = NewDb();
        // From=Mar1, To=Mar2 → half-open [Mar1 00:00, Mar3 00:00); inclusive calendar days.
        var lastInstantOfFromDay = new DateTime(2026, 3, 1, 23, 59, 59, DateTimeKind.Utc);
        var lastInstantOfToDay = new DateTime(2026, 3, 2, 23, 59, 59, DateTimeKind.Utc);
        var firstInstantAfterToDay = new DateTime(2026, 3, 3, 0, 0, 0, DateTimeKind.Utc);

        db.Patients.Add(MakePatient(1, Sex.Male, lastInstantOfFromDay));
        db.Patients.Add(MakePatient(2, Sex.Female, lastInstantOfToDay));
        db.Patients.Add(MakePatient(3, Sex.Male, firstInstantAfterToDay));

        var query = new GetPatientCountStatisticsQuery(
            new DateOnly(2026, 3, 1),
            new DateOnly(2026, 3, 2),
            BySex: false,
            ByReferralEntity: false,
            ByAccountType: false,
            GroupByMonth: false,
            GroupByDayOfMonth: false,
            IncludeMoneyRow: false);

        var result = await Handler(db).Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.TotalCount);
    }

    // =====================================================================
    // R-F01 (a) — day-of-month grouping, BR-F01-1…5
    // =====================================================================

    private static GetPatientCountStatisticsQuery DayQuery(
        bool byDay,
        bool includeMoney = false,
        DateOnly? from = null,
        DateOnly? to = null) => new(
        from ?? new DateOnly(2026, 3, 1),
        to ?? new DateOnly(2026, 3, 31),
        BySex: false,
        ByReferralEntity: false,
        ByAccountType: false,
        GroupByMonth: false,
        GroupByDayOfMonth: byDay,
        IncludeMoneyRow: includeMoney);

    [Fact]
    public async Task DayOfMonth_GroupsByDayOrdinal()
    {
        var db = NewDb();
        db.Patients.Add(MakePatient(1, Sex.Male, Day1));
        db.Patients.Add(MakePatient(2, Sex.Female, Day1));
        db.Patients.Add(MakePatient(3, Sex.Male, Day15));

        var result = await Handler(db).Handle(DayQuery(byDay: true), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.DayOfMonthCounts.Count);
        Assert.Equal(1, result.Value.DayOfMonthCounts[0].Day);
        Assert.Equal(2, result.Value.DayOfMonthCounts[0].Count);
        Assert.Equal(15, result.Value.DayOfMonthCounts[1].Day);
        Assert.Equal(1, result.Value.DayOfMonthCounts[1].Count);

        // Ordered by Day ascending, and zero-count days are not emitted.
        Assert.Equal(result.Value.DayOfMonthCounts.Select(d => d.Day).OrderBy(d => d),
            result.Value.DayOfMonthCounts.Select(d => d.Day));
        Assert.DoesNotContain(result.Value.DayOfMonthCounts, d => d.Count == 0);
    }

    [Fact]
    public async Task DayOfMonth_FlagFalse_ReturnsEmpty()
    {
        var db = NewDb();
        db.Patients.Add(MakePatient(1, Sex.Male, Day1));

        var result = await Handler(db).Handle(DayQuery(byDay: false), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!.DayOfMonthCounts);
    }

    [Fact]
    public async Task DayOfMonth_RespectsPeriodFilter()
    {
        var db = NewDb();
        // Day 5 before From; day 5 inside the period; day 20 inside.
        db.Patients.Add(MakePatient(1, Sex.Male, new DateTime(2026, 2, 5, 10, 0, 0, DateTimeKind.Utc)));
        db.Patients.Add(MakePatient(2, Sex.Male, new DateTime(2026, 3, 5, 10, 0, 0, DateTimeKind.Utc)));
        db.Patients.Add(MakePatient(3, Sex.Male, Day20));

        var result = await Handler(db).Handle(DayQuery(byDay: true), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.TotalCount);
        Assert.Equal([5, 20], result.Value.DayOfMonthCounts.Select(d => d.Day));
    }

    [Fact]
    public async Task DayOfMonth_ExcludesDeletedPatients()
    {
        var db = NewDb();
        db.Patients.Add(MakePatient(1, Sex.Male, Day1));
        var deleted = MakePatient(2, Sex.Female, Day1);
        deleted.SoftDelete();
        db.Patients.Add(deleted);

        var result = await Handler(db).Handle(DayQuery(byDay: true), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value!.DayOfMonthCounts.Single(d => d.Day == 1).Count);
    }

    // =====================================================================
    // R-F01 (b) — the money row, BR-F01-6…12 (OD-2 = Option A)
    // =====================================================================

    private static PaymentOperation Payment(
        int id,
        int patientId,
        decimal amount,
        DateTime atUtc,
        decimal? discount = null,
        bool extraCharge = false)
    {
        return PaymentOperation.Create(
            PaymentOperationId.Create(id),
            PatientId.Create(patientId),
            amount,
            1,
            atUtc,
            discount,
            extraCharge);
    }

    private static void Void(PaymentOperation op) => op.Void();

    [Fact]
    public async Task Money_UsesPatientAccountCalculatorFormula()
    {
        var db = NewDb();
        db.Patients.Add(MakePatient(1, Sex.Male, Day1));
        db.PaymentOperations.Add(Payment(1, 1, 100m, Day1, discount: 10m));   // 110
        db.PaymentOperations.Add(Payment(2, 1, 50m, Day15));                     // 50
        db.PaymentOperations.Add(Payment(3, 1, 999m, Day20, extraCharge: true)); // excluded

        var result = await Handler(db).Handle(DayQuery(byDay: false, includeMoney: true), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value!.Money);
        Assert.Equal(160m, result.Value.Money!.AmountsPaid);
        Assert.Equal(2, result.Value.Money.PaymentCount);
    }

    [Fact]
    public async Task Money_ExcludesVoidedAndExtraCharge()
    {
        var db = NewDb();
        db.Patients.Add(MakePatient(1, Sex.Male, Day1));
        var voided = Payment(1, 1, 100m, Day1);
        Void(voided);
        db.PaymentOperations.Add(voided);
        db.PaymentOperations.Add(Payment(2, 1, 70m, Day15, extraCharge: true));
        db.PaymentOperations.Add(Payment(3, 1, 25m, Day20));

        var result = await Handler(db).Handle(DayQuery(byDay: false, includeMoney: true), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(25m, result.Value!.Money!.AmountsPaid);
        Assert.Equal(1, result.Value.Money.PaymentCount);
    }

    [Fact]
    public async Task Money_AnchoredOnOperationDate_NotRegistrationDate()
    {
        // OD-2 = Option A: a payment collected in March for a December visit counts in March.
        var db = NewDb();
        db.Patients.Add(MakePatient(1, Sex.Male, new DateTime(2025, 12, 10, 9, 0, 0, DateTimeKind.Utc)));
        db.PaymentOperations.Add(Payment(1, 1, 300m, Day15));

        var result = await Handler(db).Handle(DayQuery(byDay: false, includeMoney: true), CancellationToken.None);

        Assert.True(result.IsSuccess);
        // The patient is outside the period (TotalCount == 0), but the money is not.
        Assert.Equal(0, result.Value!.TotalCount);
        Assert.Equal(300m, result.Value.Money!.AmountsPaid);
        Assert.Equal(1, result.Value.Money.PaymentCount);
    }

    [Fact]
    public async Task Money_IncludesPaymentsOfSoftDeletedPatients()
    {
        // BR-F01-9 — pinned on purpose so a later "cleanup" cannot silently change the figure.
        var db = NewDb();
        var deleted = MakePatient(1, Sex.Male, Day1);
        deleted.SoftDelete();
        db.Patients.Add(deleted);
        db.PaymentOperations.Add(Payment(1, 1, 400m, Day15));

        var result = await Handler(db).Handle(DayQuery(byDay: false, includeMoney: true), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value!.TotalCount); // excluded from the patient list
        Assert.Equal(400m, result.Value.Money!.AmountsPaid); // but the cash still counts
        Assert.Equal(1, result.Value.Money.PaymentCount);
    }

    [Fact]
    public async Task Money_CountsPaymentOperations()
    {
        var db = NewDb();
        db.Patients.Add(MakePatient(1, Sex.Male, Day1));
        db.PaymentOperations.Add(Payment(1, 1, 10m, Day1));
        db.PaymentOperations.Add(Payment(2, 1, 10m, Day15));
        db.PaymentOperations.Add(Payment(3, 1, 10m, Day20));

        var result = await Handler(db).Handle(DayQuery(byDay: false, includeMoney: true), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value!.Money!.PaymentCount);
        Assert.Equal(30m, result.Value.Money.AmountsPaid);
    }

    [Fact]
    public async Task Money_FlagFalse_ReturnsNull()
    {
        var db = NewDb();
        db.Patients.Add(MakePatient(1, Sex.Male, Day1));
        db.PaymentOperations.Add(Payment(1, 1, 500m, Day1));

        var result = await Handler(db).Handle(DayQuery(byDay: false, includeMoney: false), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value!.Money);
    }

    [Fact]
    public async Task Money_ZeroPayments_ZeroNotNull()
    {
        var db = NewDb();
        db.Patients.Add(MakePatient(1, Sex.Male, Day1));

        var result = await Handler(db).Handle(DayQuery(byDay: false, includeMoney: true), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value!.TotalCount);
        Assert.NotNull(result.Value.Money);
        Assert.Equal(0m, result.Value.Money!.AmountsPaid);
        Assert.Equal(0, result.Value.Money.PaymentCount);
    }

    [Fact]
    public async Task Money_ExcludesOperationsOutsideThePeriod()
    {
        var db = NewDb();
        db.Patients.Add(MakePatient(1, Sex.Male, Day1));
        db.PaymentOperations.Add(Payment(1, 1, 100m, new DateTime(2026, 2, 28, 23, 0, 0, DateTimeKind.Utc)));
        db.PaymentOperations.Add(Payment(2, 1, 50m, Day15));
        db.PaymentOperations.Add(Payment(3, 1, 70m, new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc)));

        var result = await Handler(db).Handle(DayQuery(byDay: false, includeMoney: true), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(50m, result.Value!.Money!.AmountsPaid);
        Assert.Equal(1, result.Value.Money.PaymentCount);
    }

    [Fact]
    public async Task ExistingBehaviour_UnchangedWhenBothFlagsFalse()
    {
        // The regression guard for the six-to-eight-parameter record: with both new flags
        // off, all five pre-existing classifications and TotalCount are exactly as before.
        var db = NewDb();
        db.ExternalEntities.Add(MakeReferral(10, "مستشفى النور"));
        db.Patients.Add(MakePatient(1, Sex.Male, Day1, AccountType.Individual, referralEntityId: 10));
        db.Patients.Add(MakePatient(2, Sex.Female, Day15, AccountType.Vip));
        db.Patients.Add(MakePatient(3, Sex.Male, Day20, AccountType.LabToLab));
        // April 10 is deliberately OUTSIDE the March period, so it must not be counted.
        db.Patients.Add(MakePatient(4, Sex.Male, April10));

        // Regression guard for the six-to-eight-parameter record. This one needs EVERY
        // classification switched on, so it builds its query explicitly rather than using
        // DayQuery (which is deliberately narrow).
        var all = new GetPatientCountStatisticsQuery(
            new DateOnly(2026, 3, 1),
            new DateOnly(2026, 3, 31),
            BySex: true,
            ByReferralEntity: true,
            ByAccountType: true,
            GroupByMonth: true,
            GroupByDayOfMonth: false,
            IncludeMoneyRow: false);

        var result = await Handler(db).Handle(all, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var dto = result.Value!;
        Assert.Equal(3, dto.TotalCount);

        // 1. sex (patients 1 and 3 are Male; patient 2 is Female)
        Assert.Equal(2, dto.SexCounts.Single(c => c.Key == ((int)Sex.Male).ToString()).Count);
        Assert.Equal(1, dto.SexCounts.Single(c => c.Key == ((int)Sex.Female).ToString()).Count);
        // 2. referral, with the no-referral bucket (only patient 1 has a referral entity)
        Assert.Equal(1, dto.ReferralEntityCounts.Single(c => c.Key == "10").Count);
        Assert.Equal("مستشفى النور", dto.ReferralEntityCounts.Single(c => c.Key == "10").DisplayName);
        Assert.Equal(2, dto.ReferralEntityCounts.Single(c => c.Key == string.Empty).Count);
        // 3. account type
        Assert.Equal(1, dto.AccountTypeCounts.Single(c => c.Key == ((int)AccountType.Individual).ToString()).Count);
        Assert.Equal(1, dto.AccountTypeCounts.Single(c => c.Key == ((int)AccountType.Vip).ToString()).Count);
        Assert.Equal(1, dto.AccountTypeCounts.Single(c => c.Key == ((int)AccountType.LabToLab).ToString()).Count);
        // 4. month — all three in-period patients registered in March 2026
        Assert.Equal(3, dto.MonthlyCounts.Single(m => m.Year == 2026 && m.Month == 3).Count);
        // 5. month x sex
        Assert.Equal(2, dto.MonthlySexCounts
            .Single(c => c.Year == 2026 && c.Month == 3 && c.Key == ((int)Sex.Male).ToString()).Count);
        Assert.Equal(1, dto.MonthlySexCounts
            .Single(c => c.Year == 2026 && c.Month == 3 && c.Key == ((int)Sex.Female).ToString()).Count);

        // and the two new members take their documented empty values
        Assert.Empty(dto.DayOfMonthCounts);
        Assert.Null(dto.Money);
    }
}
