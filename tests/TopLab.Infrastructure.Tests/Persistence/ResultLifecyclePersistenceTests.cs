using Microsoft.EntityFrameworkCore;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;
using TopLab.Infrastructure.Persistence;
using TopLab.Infrastructure.Tests.Common;
using Xunit;

namespace TopLab.Infrastructure.Tests.Persistence;

public class ResultLifecyclePersistenceTests
{
    [Fact]
    public async Task EnterReviewPrintDeliver_RoundTrips_WithSnapshot()
    {
        var options = InMemoryContextFactory.Create();
        await using var ctx = new ApplicationDbContext(options);
        ctx.Database.EnsureCreated();

        ctx.Patients.Add(Patient.Create(PatientId.Create(1), "Lifecycle", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow));
        ctx.Tests.Add(Test.Create(TestId.Create(10), "CBC", "CBC", "CBC", "T10", 30, 100m));
        await ctx.SaveChangesAsync();

        var pt = PatientTest.Create(PatientTestId.Create(0), PatientId.Create(1), TestId.Create(10), 100m);
        ctx.PatientTests.Add(pt);
        await ctx.SaveChangesAsync();

        var tracked = await ctx.PatientTests.SingleAsync();
        tracked.EnterResult("5.5", ResultFlag.Normal, 7, DateTime.UtcNow, "n");
        var snapshot = new ReferenceRangeSnapshot(10, Sex.Male, AgeUnit.Year, 0, 100, 4m, 10m, "lo", "hi", DateTimeOffset.UtcNow);
        ctx.PatientTestReferenceRangeSnapshots.Add(PatientTestReferenceRangeSnapshot.FromSnapshot(tracked.Id, snapshot));
        await ctx.SaveChangesAsync();

        tracked.MarkReviewed(7, DateTime.UtcNow);
        await ctx.SaveChangesAsync();

        tracked.MarkPrinted(7, DateTime.UtcNow);
        tracked.MarkPrinted(7, DateTime.UtcNow);
        await ctx.SaveChangesAsync();

        tracked.MarkDelivered(7, DateTime.UtcNow);
        await ctx.SaveChangesAsync();

        await using var reread = new ApplicationDbContext(options);
        var row = await reread.PatientTests.AsNoTracking().SingleAsync();
        Assert.Equal("5.5", row.ResultValue);
        Assert.Equal(ResultFlag.Normal, row.ResultFlag);
        Assert.Equal("n", row.Notes);
        Assert.Equal(7, row.EnteredByUserId);
        Assert.NotNull(row.EnteredAtUtc);
        Assert.True(row.IsReviewed);
        Assert.True(row.IsPrinted);
        Assert.Equal(2, row.PrintCount);
        Assert.NotNull(row.LastPrintedAtUtc);
        Assert.True(row.IsDelivered);
        Assert.NotNull(row.DeliveredAtUtc);

        var snapRow = await reread.PatientTestReferenceRangeSnapshots.AsNoTracking().SingleAsync();
        Assert.Equal(row.Id.Value, snapRow.PatientTestId.Value);
        Assert.Equal(10, snapRow.TestId);
        Assert.Equal(4m, snapRow.MinValue);
        Assert.Equal(10m, snapRow.MaxValue);
    }

    [Fact]
    public async Task DeletingPatientTest_Cascades_ToSnapshot()
    {
        var options = InMemoryContextFactory.Create();
        await using var ctx = new ApplicationDbContext(options);
        ctx.Database.EnsureCreated();

        ctx.Patients.Add(Patient.Create(PatientId.Create(1), "Cascade", Sex.Female, 25, AgeUnit.Year, DateTime.UtcNow));
        ctx.Tests.Add(Test.Create(TestId.Create(11), "Glucose", "Glucose", "Glucose", "T11", 30, 50m));
        await ctx.SaveChangesAsync();

        var pt = PatientTest.Create(PatientTestId.Create(0), PatientId.Create(1), TestId.Create(11), 50m);
        ctx.PatientTests.Add(pt);
        await ctx.SaveChangesAsync();

        var tracked = await ctx.PatientTests.SingleAsync();
        var snapshot = new ReferenceRangeSnapshot(11, null, AgeUnit.Year, 0, 100, 1m, 2m, null, null, DateTimeOffset.UtcNow);
        ctx.PatientTestReferenceRangeSnapshots.Add(PatientTestReferenceRangeSnapshot.FromSnapshot(tracked.Id, snapshot));
        await ctx.SaveChangesAsync();
        Assert.Single(ctx.PatientTestReferenceRangeSnapshots);

        ctx.PatientTests.Remove(tracked);
        await ctx.SaveChangesAsync();

        Assert.Empty(ctx.PatientTestReferenceRangeSnapshots);
    }
}
