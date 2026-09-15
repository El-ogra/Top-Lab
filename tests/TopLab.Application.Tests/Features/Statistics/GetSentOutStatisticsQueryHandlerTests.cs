using TopLab.Application.Features.Statistics.Queries.GetSentOutStatistics;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.ExternalEntities;
using TopLab.Domain.SentOutSamples;
using Xunit;

namespace TopLab.Application.Tests.Features.Statistics;

public class GetSentOutStatisticsQueryHandlerTests
{
    private static readonly DateTime Day1 = new(2026, 3, 1, 10, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Day15 = new(2026, 3, 15, 8, 0, 0, DateTimeKind.Utc);

    private static FakeApplicationDbContext NewDb() => new();

    private static GetSentOutStatisticsQueryHandler Handler(FakeApplicationDbContext db, DateTime? utcNow = null)
    {
        var clock = new FakeDateTimeProvider();
        if (utcNow.HasValue)
        {
            clock.UtcNow = utcNow.Value;
        }

        return new GetSentOutStatisticsQueryHandler(db, clock);
    }

    private static ExternalEntity MakeLab(int id, string name)
    {
        return ExternalEntity.Create(ExternalEntityId.Create(id), EntityType.PartnerLab, name);
    }

    private static SentOutSample MakeSample(int id, int labId, decimal cost, DateTime sentAtUtc)
    {
        return SentOutSample.Create(
            SentOutSampleId.Create(id),
            PatientTestId.Create(id),
            ExternalEntityId.Create(labId),
            cost,
            cost + 10m,
            sentAtUtc);
    }

    private static SentOutSamplePayment MakePayment(int id, int sampleId, decimal amount)
    {
        return SentOutSamplePayment.Create(
            SentOutSamplePaymentId.Create(id),
            SentOutSampleId.Create(sampleId),
            amount,
            Day15,
            1);
    }

    [Fact]
    public async Task PerLabTotals_MatchCalculator_NumberForNumber()
    {
        var db = NewDb();
        db.ExternalEntities.Add(MakeLab(5, "معمل الشريك"));
        db.SentOutSamples.Add(MakeSample(1, 5, 100m, Day1));
        db.SentOutSamples.Add(MakeSample(2, 5, 50m, Day1));
        db.SentOutSamples.Add(MakeSample(3, 5, 25m, Day15));
        db.SentOutSamplePayments.Add(MakePayment(1, 1, 40m));
        db.SentOutSamplePayments.Add(MakePayment(2, 2, 20m));

        var query = new GetSentOutStatisticsQuery(
            new DateOnly(2026, 3, 1),
            new DateOnly(2026, 3, 31),
            ExternalLabEntityId: null);

        var result = await Handler(db).Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value!.TotalSent);
        var lab = result.Value.Labs.Single();
        Assert.Equal(5, lab.LabId);
        Assert.Equal("معمل الشريك", lab.LabName);
        Assert.Equal(3, lab.SentCount);
        Assert.Equal(175m, lab.TotalCost);
        Assert.Equal(60m, lab.TotalPaid);
        Assert.Equal(115m, lab.Remaining);

        var expectedRemaining = SentOutAccountCalculator.Remaining(
            SentOutAccountCalculator.TotalCost(db.SentOutSamples.ToList()),
            SentOutAccountCalculator.TotalPaid(db.SentOutSamplePayments.ToList()));
        Assert.Equal(expectedRemaining, lab.Remaining);
    }

    [Fact]
    public async Task LabFilter_OnlyCountsThatLab()
    {
        var db = NewDb();
        db.ExternalEntities.Add(MakeLab(5, "A"));
        db.ExternalEntities.Add(MakeLab(6, "B"));
        db.SentOutSamples.Add(MakeSample(1, 5, 100m, Day1));
        db.SentOutSamples.Add(MakeSample(2, 6, 80m, Day1));

        var query = new GetSentOutStatisticsQuery(
            new DateOnly(2026, 3, 1),
            new DateOnly(2026, 3, 31),
            ExternalLabEntityId: 6);

        var result = await Handler(db).Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value!.TotalSent);
        Assert.Equal(6, result.Value.Labs.Single().LabId);
        Assert.Equal(80m, result.Value.Labs.Single().TotalCost);
    }

    [Fact]
    public async Task UnknownLabFilter_ReturnsNotFound()
    {
        var db = NewDb();

        var query = new GetSentOutStatisticsQuery(
            new DateOnly(2026, 3, 1),
            new DateOnly(2026, 3, 31),
            ExternalLabEntityId: 999);

        var result = await Handler(db).Handle(query, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("الجهة الخارجية غير موجودة.", result.Error!.Message);
    }

    [Fact]
    public async Task EmptyPeriod_ReturnsZeros()
    {
        var db = NewDb();
        db.ExternalEntities.Add(MakeLab(5, "A"));
        db.SentOutSamples.Add(MakeSample(1, 5, 100m, Day1));

        var query = new GetSentOutStatisticsQuery(
            new DateOnly(2026, 5, 1),
            new DateOnly(2026, 5, 31),
            ExternalLabEntityId: null);

        var result = await Handler(db).Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value!.TotalSent);
        Assert.Empty(result.Value.Labs);
    }

    [Fact]
    public async Task UnknownLabName_FallsBackToRawIdString()
    {
        var db = NewDb();
        db.SentOutSamples.Add(MakeSample(1, 42, 10m, Day1));

        var query = new GetSentOutStatisticsQuery(
            new DateOnly(2026, 3, 1),
            new DateOnly(2026, 3, 31),
            ExternalLabEntityId: null);

        var result = await Handler(db).Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("42", result.Value!.Labs.Single().LabName);
    }

    [Fact]
    public async Task NoPayments_RemainingEqualsCost()
    {
        var db = NewDb();
        db.ExternalEntities.Add(MakeLab(5, "A"));
        db.SentOutSamples.Add(MakeSample(1, 5, 100m, Day1));

        var query = new GetSentOutStatisticsQuery(
            new DateOnly(2026, 3, 1),
            new DateOnly(2026, 3, 31),
            ExternalLabEntityId: null);

        var result = await Handler(db).Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var lab = result.Value!.Labs.Single();
        Assert.Equal(0m, lab.TotalPaid);
        Assert.Equal(100m, lab.Remaining);
    }

    [Fact]
    public void Validator_RejectsInvertedPeriod_WithFrozenMessage()
    {
        var validator = new GetSentOutStatisticsQueryValidator();
        var query = new GetSentOutStatisticsQuery(
            new DateOnly(2026, 3, 10),
            new DateOnly(2026, 3, 1),
            ExternalLabEntityId: null);

        var outcome = validator.Validate(query);

        Assert.False(outcome.IsValid);
        Assert.Contains(outcome.Errors, e => e.ErrorMessage == "بداية الفترة يجب ألا تتجاوز نهايتها.");
    }
}
