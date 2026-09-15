using TopLab.Application.Features.SentOutSamples.Queries.GetSentOutSamples;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.ExternalEntities;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.SentOutSamples;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Application.Tests.Features.SentOutSamples;

public class GetSentOutSamplesQueryHandlerTests
{
    private static readonly DateTime Day1 = new(2026, 3, 10, 10, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Day2 = new(2026, 3, 11, 10, 0, 0, DateTimeKind.Utc);

    private static void SeedGraph(
        FakeApplicationDbContext db, int tag, int labId, EntityType labType, DateTime sentAt,
        decimal cost = 100m, decimal paid = 0m)
    {
        var patient = Patient.Create(PatientId.Create(tag), $"P{tag}", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow);
        var test = Test.Create(
            TestId.Create(tag), $"T{tag}", $"T{tag}", $"T{tag}", $"T{tag}",
            30, 150m, ResultKind.Simple, isSentOut: true, sentOutCostPrice: cost);
        var lab = ExternalEntity.Create(ExternalEntityId.Create(labId), labType, $"Lab{labId}");
        db.Patients.Add(patient);
        db.Tests.Add(test);
        if (!db.ExternalEntities.Any(e => e.Id.Value == labId))
        {
            db.ExternalEntities.Add(lab);
        }

        var patientTest = PatientTest.Create(
            PatientTestId.Create(tag), patient.Id, test.Id, 150m);
        db.PatientTests.Add(patientTest);

        var sample = SentOutSample.Create(
            SentOutSampleId.Create(tag), patientTest.Id, ExternalEntityId.Create(labId),
            cost, 150m, sentAt);
        db.SentOutSamples.Add(sample);

        if (paid > 0)
        {
            db.SentOutSamplePayments.Add(SentOutSamplePayment.Create(
                SentOutSamplePaymentId.Create(tag), sample.Id, paid, sentAt, performedByUserId: 1));
        }
    }

    private static GetSentOutSamplesQueryHandler NewHandler(FakeApplicationDbContext db, DateTime? now = null)
    {
        var time = new FakeDateTimeProvider();
        if (now.HasValue)
        {
            time.UtcNow = now.Value;
        }

        return new GetSentOutSamplesQueryHandler(db, time);
    }

    [Fact]
    public async Task PeriodFilter_InclusiveBothEnds()
    {
        var db = new FakeApplicationDbContext();
        SeedGraph(db, 1, 1, EntityType.PartnerLab, Day1);
        SeedGraph(db, 2, 1, EntityType.PartnerLab, Day2);
        SeedGraph(db, 3, 1, EntityType.PartnerLab, Day2.AddDays(1));

        var handler = NewHandler(db);
        var result = await handler.Handle(
            new GetSentOutSamplesQuery(
                DateOnly.FromDateTime(Day1), DateOnly.FromDateTime(Day2), null, 1, 10),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Count);
        Assert.Equal([1, 2], result.Value.Select(d => d.Id).OrderBy(i => i));
    }

    [Fact]
    public async Task BoundaryInstants_Included()
    {
        var db = new FakeApplicationDbContext();
        var day = new DateTime(2026, 3, 12, 0, 0, 0, DateTimeKind.Utc);
        SeedGraph(db, 1, 1, EntityType.PartnerLab, day);
        SeedGraph(db, 2, 1, EntityType.PartnerLab, day.AddDays(1).AddTicks(-1));

        var handler = NewHandler(db);
        var result = await handler.Handle(
            new GetSentOutSamplesQuery(DateOnly.FromDateTime(day), DateOnly.FromDateTime(day), null, 1, 10),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Count);
    }

    [Fact]
    public async Task EntityFilter_ReturnsOnlyThatEntitySamples()
    {
        var db = new FakeApplicationDbContext();
        SeedGraph(db, 1, 1, EntityType.PartnerLab, Day1);
        SeedGraph(db, 2, 2, EntityType.PartnerLab, Day1);

        var handler = NewHandler(db);
        var result = await handler.Handle(
            new GetSentOutSamplesQuery(
                DateOnly.FromDateTime(Day1), DateOnly.FromDateTime(Day2), 2, 1, 10),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        var single = Assert.Single(result.Value!);
        Assert.Equal(2, single.Id);
        Assert.Equal(2, single.ExternalLabEntityId);
    }

    [Fact]
    public async Task NamesAndTotals_ResolvedViaCalculator()
    {
        var db = new FakeApplicationDbContext();
        SeedGraph(db, 1, 1, EntityType.PartnerLab, Day1, cost: 100m, paid: 40m);

        var handler = NewHandler(db);
        var result = await handler.Handle(
            new GetSentOutSamplesQuery(
                DateOnly.FromDateTime(Day1), DateOnly.FromDateTime(Day1), null, 1, 10),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        var dto = Assert.Single(result.Value!);
        Assert.Equal("P1", dto.PatientName);
        Assert.Equal("T1", dto.TestName);
        Assert.Equal("Lab1", dto.ExternalLabName);
        Assert.Equal(100m, dto.CostPrice);
        Assert.Equal(150m, dto.PatientPrice);
        Assert.Equal(40m, dto.TotalPaid);
        Assert.Equal(60m, dto.Remaining);
        Assert.False(dto.IsFullySettled);
    }

    [Fact]
    public async Task EmptySet_ReturnsEmptySuccess()
    {
        var db = new FakeApplicationDbContext();

        var handler = NewHandler(db);
        var result = await handler.Handle(
            new GetSentOutSamplesQuery(
                DateOnly.FromDateTime(Day1), DateOnly.FromDateTime(Day1), null, 1, 10),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!);
    }

    [Fact]
    public async Task OmittedPeriod_DefaultsToToday()
    {
        var db = new FakeApplicationDbContext();
        var today = new DateTime(2026, 5, 1, 12, 0, 0, DateTimeKind.Utc);
        SeedGraph(db, 1, 1, EntityType.PartnerLab, today);
        SeedGraph(db, 2, 1, EntityType.PartnerLab, today.AddDays(-1));

        var handler = NewHandler(db, today);
        var result = await handler.Handle(
            new GetSentOutSamplesQuery(null, null, null, 1, 10),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        var single = Assert.Single(result.Value!);
        Assert.Equal(1, single.Id);
    }

    [Fact]
    public async Task Paging_SkipsAndTakes()
    {
        var db = new FakeApplicationDbContext();
        SeedGraph(db, 1, 1, EntityType.PartnerLab, Day1.AddHours(1));
        SeedGraph(db, 2, 1, EntityType.PartnerLab, Day1.AddHours(2));
        SeedGraph(db, 3, 1, EntityType.PartnerLab, Day1.AddHours(3));

        var handler = NewHandler(db);
        var page2 = await handler.Handle(
            new GetSentOutSamplesQuery(
                DateOnly.FromDateTime(Day1), DateOnly.FromDateTime(Day1), null, 2, 2),
            CancellationToken.None);

        Assert.True(page2.IsSuccess);
        var single = Assert.Single(page2.Value!);
        Assert.Equal(3, single.Id);
    }

    [Fact]
    public void Validator_InvertedPeriod_RejectedWithMessage()
    {
        var validator = new GetSentOutSamplesQueryValidator();
        var result = validator.Validate(new GetSentOutSamplesQuery(
            DateOnly.FromDateTime(Day2), DateOnly.FromDateTime(Day1), null, 1, 10));

        Assert.False(result.IsValid);
        Assert.Equal("بداية الفترة يجب ألا تتجاوز نهايتها.", Assert.Single(result.Errors).ErrorMessage);
    }

    [Fact]
    public void Validator_PagingBounds()
    {
        var validator = new GetSentOutSamplesQueryValidator();

        Assert.False(validator.Validate(new GetSentOutSamplesQuery(null, null, null, 0, 10)).IsValid);
        Assert.False(validator.Validate(new GetSentOutSamplesQuery(null, null, null, 1, 0)).IsValid);
        Assert.False(validator.Validate(new GetSentOutSamplesQuery(null, null, null, 1, 101)).IsValid);
        Assert.True(validator.Validate(new GetSentOutSamplesQuery(null, null, null, 1, 10)).IsValid);
    }
}
