using TopLab.Application.Features.Statistics.Queries.GetPatientCountStatistics;
using TopLab.Application.Tests.Common.Fakes;
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
            GroupByMonth: false);

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
            GroupByMonth: false);

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
            GroupByMonth: false);

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
            GroupByMonth: false);

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
            GroupByMonth: true);

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
            GroupByMonth: true);

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
            GroupByMonth: false);

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
            GroupByMonth: true);

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
            GroupByMonth: false);

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
            GroupByMonth: false);

        var outcome = validator.Validate(query);

        Assert.False(outcome.IsValid);
        Assert.Contains(outcome.Errors, e => e.ErrorMessage == "بداية الفترة يجب ألا تتجاوز نهايتها.");
    }

    [Fact]
    public void Validator_AllowsEqualAndOpenPeriods()
    {
        var validator = new GetPatientCountStatisticsQueryValidator();

        var equal = validator.Validate(new GetPatientCountStatisticsQuery(
            new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 1), true, false, false, false));
        var open = validator.Validate(new GetPatientCountStatisticsQuery(
            null, new DateOnly(2026, 3, 1), true, false, false, false));
        var bothNull = validator.Validate(new GetPatientCountStatisticsQuery(
            null, null, true, false, false, false));

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
            GroupByMonth: false);

        var result = await Handler(db).Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.TotalCount);
    }
}
