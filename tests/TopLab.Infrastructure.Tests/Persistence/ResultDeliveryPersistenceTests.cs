using TopLab.Application.Features.ResultDelivery.Commands.DeliverWithSettlement;
using TopLab.Application.Features.ResultDelivery.Queries.GetDeliveryGrid;
using TopLab.Application.Features.ResultDelivery.Queries.GetUndeliveredResults;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;
using TopLab.Infrastructure.Persistence;
using TopLab.Infrastructure.Tests.Common;
using TopLab.Infrastructure.Tests.Common.Fakes;
using Xunit;

namespace TopLab.Infrastructure.Tests.Persistence;

/// <summary>
/// M-09 infrastructure proof on the real <see cref="ApplicationDbContext"/> over the
/// InMemory provider: two-day registration spread with a fully-delivered patient and a
/// soft-deleted patient in the seed. The undelivered list respects the inclusive period
/// filter and both exclusions; the grid returns the frozen order-time price (not the
/// catalog price); the composite command persists the per-line delivery audit columns.
/// </summary>
public class ResultDeliveryPersistenceTests
{
    private static readonly DateOnly Sep1 = new(2026, 9, 1);
    private static readonly DateOnly Sep2 = new(2026, 9, 2);
    private static readonly DateTime T0 = new(2026, 9, 1, 12, 0, 0, DateTimeKind.Utc);

    private static async Task<ApplicationDbContext> SeedAsync()
    {
        var options = InMemoryContextFactory.Create();
        var ctx = new ApplicationDbContext(options);
        ctx.Database.EnsureCreated();

        // Catalog price (500) deliberately differs from every frozen order-time price (100).
        ctx.Tests.Add(Test.Create(TestId.Create(10), "CBC", "CBC", "CBC", "CBC", 60, 500m));

        var patientA = Patient.Create(PatientId.Create(1), "Seham", Sex.Female, 30, AgeUnit.Year, T0);
        patientA.AssignLabId(LabId.Create("LAB-1"));
        var patientB = Patient.Create(
            PatientId.Create(2), "Karim", Sex.Male, 40, AgeUnit.Year, T0.AddDays(1));
        var patientC = Patient.Create(PatientId.Create(3), "Mona", Sex.Female, 25, AgeUnit.Year, T0);
        var patientD = Patient.Create(PatientId.Create(4), "Omar", Sex.Male, 50, AgeUnit.Year, T0);
        patientD.SoftDelete();
        ctx.Patients.AddRange(patientA, patientB, patientC, patientD);

        var undelivered = PatientTest.Create(PatientTestId.Create(11), PatientId.Create(1), TestId.Create(10), 100m);
        undelivered.EnterResult("5", ResultFlag.Normal, 1, T0);
        undelivered.MarkReviewed(1, T0);
        undelivered.MarkPrinted(1, T0);
        ctx.PatientTests.Add(undelivered);

        var enteredOnly = PatientTest.Create(PatientTestId.Create(12), PatientId.Create(1), TestId.Create(10), 100m);
        enteredOnly.EnterResult("6", ResultFlag.Normal, 1, T0);
        ctx.PatientTests.Add(enteredOnly);

        var dayTwo = PatientTest.Create(
            PatientTestId.Create(13), PatientId.Create(2), TestId.Create(10), 100m);
        dayTwo.EnterResult("7", ResultFlag.Normal, 1, T0);
        dayTwo.MarkReviewed(1, T0);
        dayTwo.MarkPrinted(1, T0);
        ctx.PatientTests.Add(dayTwo);

        var delivered = PatientTest.Create(PatientTestId.Create(14), PatientId.Create(3), TestId.Create(10), 100m);
        delivered.EnterResult("8", ResultFlag.Normal, 1, T0);
        delivered.MarkReviewed(1, T0);
        delivered.MarkPrinted(1, T0);
        delivered.MarkDelivered(1, T0);
        ctx.PatientTests.Add(delivered);

        var deletedRow = PatientTest.Create(PatientTestId.Create(15), PatientId.Create(4), TestId.Create(10), 100m);
        deletedRow.EnterResult("9", ResultFlag.Normal, 1, T0);
        deletedRow.MarkReviewed(1, T0);
        deletedRow.MarkPrinted(1, T0);
        ctx.PatientTests.Add(deletedRow);

        await ctx.SaveChangesAsync();
        return ctx;
    }

    [Fact]
    public async Task UndeliveredList_OnRealContext_RespectsPeriodAndExclusions()
    {
        await using var ctx = await SeedAsync();

        var handler = new GetUndeliveredResultsQueryHandler(ctx);
        var dayOne = await handler.Handle(
            new GetUndeliveredResultsQuery(Sep1, Sep1), CancellationToken.None);

        Assert.True(dayOne.IsSuccess);
        // Patient 1 only: patient 3 is fully delivered, patient 4 is soft-deleted,
        // patient 2 registered on day two.
        var row = Assert.Single(dayOne.Value!);
        Assert.Equal(1, row.PatientId);
        Assert.Equal("Seham", row.PatientFullName);
        Assert.Equal("LAB-1", row.LabId);
        // Lines 11 (printed) and 12 (entered only) are both undelivered.
        Assert.Equal(2, row.UndeliveredCount);

        var bothDays = await handler.Handle(
            new GetUndeliveredResultsQuery(Sep1, Sep2), CancellationToken.None);

        Assert.True(bothDays.IsSuccess);
        Assert.Equal([1, 2], bothDays.Value!.Select(r => r.PatientId).Order().ToList());
    }

    [Fact]
    public async Task Grid_OnRealContext_ReturnsFrozenPrice()
    {
        await using var ctx = await SeedAsync();

        var handler = new GetDeliveryGridQueryHandler(ctx);
        var result = await handler.Handle(new GetDeliveryGridQuery(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Count);
        Assert.All(result.Value, r => Assert.Equal(100m, r.Price));
        Assert.Equal("CBC", result.Value[0].TestName);
    }

    [Fact]
    public async Task DeliverWithSettlement_OnRealContext_PersistsAuditColumns()
    {
        await using var ctx = await SeedAsync();
        var deliveredAt = new DateTime(2026, 9, 3, 9, 0, 0, DateTimeKind.Utc);

        var handler = new DeliverWithSettlementCommandHandler(
            ctx,
            new FakeCurrentUserService { UserId = 7 },
            new FakeDateTimeProvider { UtcNow = deliveredAt });
        var result = await handler.Handle(
            new DeliverWithSettlementCommand(1, new[] { 11 }), CancellationToken.None);

        Assert.True(result.IsSuccess);

        var saved = ctx.PatientTests.Single(pt => pt.Id.Value == 11);
        Assert.True(saved.IsDelivered);
        Assert.Equal(7, saved.DeliveredByUserId);
        Assert.Equal(deliveredAt, saved.DeliveredAtUtc);

        var untouched = ctx.PatientTests.Single(pt => pt.Id.Value == 12);
        Assert.False(untouched.IsDelivered);
        Assert.Null(untouched.DeliveredByUserId);
        Assert.Null(untouched.DeliveredAtUtc);
    }
}
