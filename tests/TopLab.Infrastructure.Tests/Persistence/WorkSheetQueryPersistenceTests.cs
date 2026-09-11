using TopLab.Application.Features.WorkSheets.Queries.GetWorkSheetByWorkGroupLog;
using TopLab.Application.Features.WorkSheets.Queries.GetWorkSheetTestCountByPeriod;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Settings;
using TopLab.Domain.Tests;
using TopLab.Infrastructure.Persistence;
using TopLab.Infrastructure.Tests.Common;
using Xunit;

namespace TopLab.Infrastructure.Tests.Persistence;

/// <summary>
/// M-11 infrastructure proof on the real <see cref="ApplicationDbContext"/> over the
/// InMemory provider: a saved work-group log plus two patients' tests (one outside-lab
/// row, one drawn row) across a two-day period. The by-log DTO contains only the in-lab
/// rows with resolved names and correct period filtering; the FR-M11-004 classification
/// counts the same seed correctly, including the outside-lab row.
/// </summary>
public class WorkSheetQueryPersistenceTests
{
    private static readonly DateOnly Sep1 = new(2026, 9, 1);
    private static readonly DateOnly Sep2 = new(2026, 9, 2);

    private static async Task<ApplicationDbContext> SeedAsync()
    {
        var options = InMemoryContextFactory.Create();
        var ctx = new ApplicationDbContext(options);
        ctx.Database.EnsureCreated();

        ctx.Tests.Add(Test.Create(
            TestId.Create(10), "Glucose", "Glucose", "Glucose", "GLU", 60, 100m, barcode: "BC-GLU"));
        ctx.Tests.Add(Test.Create(
            TestId.Create(20), "Urea", "Urea", "Urea", "UREA", 60, 100m));

        var log = WorkGroupLog.Create(WorkGroupLogId.Create(7), "Bench");
        ctx.WorkGroupLogs.Add(log);
        ctx.WorkGroupLogItems.Add(WorkGroupLogItem.Create(log.Id, TestId.Create(10)));
        ctx.WorkGroupLogItems.Add(WorkGroupLogItem.Create(log.Id, TestId.Create(20)));

        var patientA = Patient.Create(
            PatientId.Create(1), "Seham", Sex.Female, 30, AgeUnit.Year,
            new DateTime(2026, 9, 1, 12, 0, 0, DateTimeKind.Utc));
        patientA.AssignLabId(LabId.Create("LAB-1"));
        var patientB = Patient.Create(
            PatientId.Create(2), "Karim", Sex.Male, 40, AgeUnit.Year,
            new DateTime(2026, 9, 2, 12, 0, 0, DateTimeKind.Utc));
        patientB.AssignLabId(LabId.Create("LAB-2"));
        ctx.Patients.AddRange(patientA, patientB);

        ctx.PatientTests.Add(PatientTest.Create(
            PatientTestId.Create(11), PatientId.Create(1), TestId.Create(10), 100m));
        ctx.PatientTests.Add(PatientTest.Create(
            PatientTestId.Create(12), PatientId.Create(1), TestId.Create(20), 100m,
            isTakenOutsideLab: true));
        var drawn = PatientTest.Create(
            PatientTestId.Create(13), PatientId.Create(2), TestId.Create(10), 100m);
        drawn.MarkSampleDrawn(new DateTime(2026, 9, 2, 13, 0, 0, DateTimeKind.Utc));
        ctx.PatientTests.Add(drawn);
        ctx.PatientTests.Add(PatientTest.Create(
            PatientTestId.Create(14), PatientId.Create(2), TestId.Create(20), 100m));

        await ctx.SaveChangesAsync();
        return ctx;
    }

    [Fact]
    public async Task ByLog_OnRealContext_ContainsOnlyInLabRowsWithResolvedNames()
    {
        await using var ctx = await SeedAsync();

        var handler = new GetWorkSheetByWorkGroupLogQueryHandler(ctx);
        var result = await handler.Handle(
            new GetWorkSheetByWorkGroupLogQuery(7, From: Sep1, To: Sep2), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var sheet = result.Value!;
        Assert.Equal(Sep1, sheet.From);
        Assert.Equal(Sep2, sheet.To);
        Assert.Single(sheet.Sections);
        Assert.Equal("Bench", sheet.Sections[0].SectionName);
        Assert.Equal(3, sheet.TotalTests);

        var lines = sheet.Sections[0].Lines;
        Assert.Equal([11, 13, 14], lines.Select(l => l.PatientTestId));
        Assert.All(lines, l => Assert.False(string.IsNullOrWhiteSpace(l.TestName)));
        var glucose = lines[0];
        Assert.Equal("Seham", glucose.PatientFullName);
        Assert.Equal("LAB-1", glucose.LabId);
        Assert.Equal("GLU", glucose.TestCode);
        Assert.Equal("BC-GLU", glucose.Barcode);
        Assert.False(glucose.IsSampleDrawn);
        Assert.True(lines[1].IsSampleDrawn);
    }

    [Fact]
    public async Task TestCount_OnRealContext_CountsSameSeedIncludingOutsideLabRow()
    {
        await using var ctx = await SeedAsync();

        var handler = new GetWorkSheetTestCountByPeriodQueryHandler(ctx);
        var result = await handler.Handle(
            new GetWorkSheetTestCountByPeriodQuery(From: Sep1, To: Sep2), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var count = result.Value!;
        Assert.Equal(2, count.Rows.Count);
        Assert.Equal(10, count.Rows[0].TestId);
        Assert.Equal("Glucose", count.Rows[0].TestName);
        Assert.Equal(2, count.Rows[0].Count);
        Assert.Equal(20, count.Rows[1].TestId);
        Assert.Equal(2, count.Rows[1].Count);
        Assert.Equal(4, count.TotalCount);
    }
}
