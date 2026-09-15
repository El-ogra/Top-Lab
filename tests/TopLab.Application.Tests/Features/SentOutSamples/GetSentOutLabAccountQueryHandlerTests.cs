using TopLab.Application.Features.SentOutSamples.Queries.GetSentOutLabAccount;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.ExternalEntities;
using TopLab.Domain.SentOutSamples;
using Xunit;

namespace TopLab.Application.Tests.Features.SentOutSamples;

public class GetSentOutLabAccountQueryHandlerTests
{
    private static readonly DateTime Day1 = new(2026, 3, 10, 10, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Day2 = new(2026, 3, 11, 10, 0, 0, DateTimeKind.Utc);

    private static void SeedLab(FakeApplicationDbContext db, int id, EntityType type)
    {
        db.ExternalEntities.Add(ExternalEntity.Create(ExternalEntityId.Create(id), type, $"Lab{id}"));
    }

    private static SentOutSample SeedSample(
        FakeApplicationDbContext db, int id, int labId, DateTime sentAt, decimal cost = 100m)
    {
        var s = SentOutSample.Create(
            SentOutSampleId.Create(id),
            PatientTestId.Create(id),
            ExternalEntityId.Create(labId),
            cost,
            patientPrice: 150m,
            sentAt);
        db.SentOutSamples.Add(s);
        return s;
    }

    private static void SeedPayment(FakeApplicationDbContext db, SentOutSample sample, decimal amount)
    {
        db.SentOutSamplePayments.Add(SentOutSamplePayment.Create(
            SentOutSamplePaymentId.Create(db.SentOutSamplePayments.Count + 1),
            sample.Id,
            amount,
            DateTime.UtcNow,
            performedByUserId: 1));
    }

    [Fact]
    public async Task CountAndTotals_MatchCalculator_NumberForNumber()
    {
        var db = new FakeApplicationDbContext();
        SeedLab(db, 1, EntityType.PartnerLab);
        var s1 = SeedSample(db, 1, 1, Day1, cost: 100m);
        var s2 = SeedSample(db, 2, 1, Day2, cost: 50m);
        SeedSample(db, 3, 1, Day2.AddDays(5), cost: 999m);
        SeedPayment(db, s1, 30m);
        SeedPayment(db, s2, 50m);

        var handler = new GetSentOutLabAccountQueryHandler(db, new FakeDateTimeProvider());
        var result = await handler.Handle(
            new GetSentOutLabAccountQuery(1, DateOnly.FromDateTime(Day1), DateOnly.FromDateTime(Day2)),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        var dto = result.Value!;
        Assert.Equal(1, dto.ExternalLabEntityId);
        Assert.Equal("Lab1", dto.ExternalLabName);
        Assert.Equal(2, dto.SentCount);

        var expectedSamples = db.SentOutSamples.Where(s => s.Id.Value is 1 or 2).ToList();
        var expectedPayments = db.SentOutSamplePayments.ToList();
        var expectedCost = SentOutAccountCalculator.TotalCost(expectedSamples);
        var expectedPaid = SentOutAccountCalculator.TotalPaid(expectedPayments);

        Assert.Equal(expectedCost, dto.TotalCost);
        Assert.Equal(150m, dto.TotalCost);
        Assert.Equal(expectedPaid, dto.TotalPaid);
        Assert.Equal(80m, dto.TotalPaid);
        Assert.Equal(SentOutAccountCalculator.Remaining(expectedCost, expectedPaid), dto.Remaining);
        Assert.Equal(70m, dto.Remaining);
    }

    [Fact]
    public async Task UnknownEntity_ReturnsNotFound()
    {
        var db = new FakeApplicationDbContext();

        var handler = new GetSentOutLabAccountQueryHandler(db, new FakeDateTimeProvider());
        var result = await handler.Handle(
            new GetSentOutLabAccountQuery(9, null, null),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("الجهة الخارجية غير موجودة.", result.Error!.Message);
    }

    [Fact]
    public async Task NonLabEntity_ReturnsConflict()
    {
        var db = new FakeApplicationDbContext();
        SeedLab(db, 1, EntityType.TreatingDoctor);

        var handler = new GetSentOutLabAccountQueryHandler(db, new FakeDateTimeProvider());
        var result = await handler.Handle(
            new GetSentOutLabAccountQuery(1, null, null),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("الجهة المختارة ليست معملًا خارجيًا.", result.Error!.Message);
    }

    [Fact]
    public void Validator_Rules()
    {
        var validator = new GetSentOutLabAccountQueryValidator();

        Assert.False(validator.Validate(new GetSentOutLabAccountQuery(0, null, null)).IsValid);

        var inverted = validator.Validate(new GetSentOutLabAccountQuery(
            1, DateOnly.FromDateTime(Day2), DateOnly.FromDateTime(Day1)));
        Assert.False(inverted.IsValid);
        Assert.Equal("بداية الفترة يجب ألا تتجاوز نهايتها.", Assert.Single(inverted.Errors).ErrorMessage);

        Assert.True(validator.Validate(new GetSentOutLabAccountQuery(1, null, null)).IsValid);
    }
}
