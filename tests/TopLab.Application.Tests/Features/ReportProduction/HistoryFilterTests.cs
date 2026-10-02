using TopLab.Application.Features.ReportProduction.Common;
using TopLab.Application.Features.ReportProduction.Queries.GetMultiPatientHistory;
using TopLab.Application.Features.ReportProduction.Queries.GetPatientTestHistory;
using TopLab.Application.Features.ReportProduction.Queries.GetSeparateHistoryReport;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Application.Tests.Features.ReportProduction;

/// <summary>W-02 S12 (WP-10): history filters, delegation, bounded reads.</summary>
public class HistoryFilterTests
{
    private static FakeApplicationDbContext Seed()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(Patient.Create(PatientId.Create(1), "Ali", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow, labId: LabId.Create("L-1")));
        db.Patients.Add(Patient.Create(PatientId.Create(2), "Sara", Sex.Female, 25, AgeUnit.Year, DateTime.UtcNow, labId: LabId.Create("L-2")));
        db.Tests.Add(Test.Create(TestId.Create(1), "Glucose", "Glucose", "GLU", "GLU", 1, 100m, ResultKind.Simple));
        db.Tests.Add(Test.Create(TestId.Create(2), "CBC", "CBC", "CBC", "CBC", 1, 100m, ResultKind.Simple));
        db.ReportSettings.Add(TopLab.Domain.Settings.ReportSettings.CreateDefault());
        return db;
    }

    private static PatientTest Entered(int ptId, int patientId, int testId, DateTime enteredAt)
    {
        var pt = PatientTest.Create(PatientTestId.Create(ptId), PatientId.Create(patientId), TestId.Create(testId), 100m);
        pt.EnterResult("5", ResultFlag.Normal, 1, enteredAt);
        return pt;
    }

    [Fact]
    public async Task GetPatientTestHistory_FiltersByDateRange()
    {
        var db = Seed();
        db.PatientTests.Add(Entered(11, 1, 1, new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc)));
        db.PatientTests.Add(Entered(12, 1, 1, new DateTime(2026, 3, 10, 0, 0, 0, DateTimeKind.Utc)));

        var result = await new GetPatientTestHistoryQueryHandler(db).Handle(
            new GetPatientTestHistoryQuery(1, new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31), null),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(new[] { 12 }, result.Value!.Entries.Select(e => e.PatientTestId));
    }

    [Fact]
    public async Task GetPatientTestHistory_FiltersByTestId()
    {
        var db = Seed();
        db.PatientTests.Add(Entered(11, 1, 1, DateTime.UtcNow));
        db.PatientTests.Add(Entered(12, 1, 2, DateTime.UtcNow));

        var result = await new GetPatientTestHistoryQueryHandler(db).Handle(
            new GetPatientTestHistoryQuery(1, null, null, 2),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(new[] { 12 }, result.Value!.Entries.Select(e => e.PatientTestId));
    }

    [Fact]
    public async Task GetPatientTestHistory_OmittedFilters_ReturnEverything()
    {
        var db = Seed();
        db.PatientTests.Add(Entered(11, 1, 1, DateTime.UtcNow));
        db.PatientTests.Add(Entered(12, 1, 2, DateTime.UtcNow));

        var result = await new GetPatientTestHistoryQueryHandler(db).Handle(
            new GetPatientTestHistoryQuery(1),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Entries.Count);
    }

    [Fact]
    public async Task GetPatientTestHistory_ExcludesOtherPatients()
    {
        var db = Seed();
        db.PatientTests.Add(Entered(11, 1, 1, DateTime.UtcNow));
        db.PatientTests.Add(Entered(12, 2, 1, DateTime.UtcNow));

        var result = await new GetPatientTestHistoryQueryHandler(db).Handle(
            new GetPatientTestHistoryQuery(1),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.All(result.Value!.Entries, e => Assert.Equal(1, e.PatientId));
    }

    [Fact]
    public async Task GetMultiPatientHistory_ReturnsUnionOfVisits()
    {
        var db = Seed();
        db.PatientTests.Add(Entered(11, 1, 1, DateTime.UtcNow));
        db.PatientTests.Add(Entered(12, 2, 1, DateTime.UtcNow));

        var result = await new GetMultiPatientHistoryQueryHandler(db).Handle(
            new GetMultiPatientHistoryQuery(new[] { 1, 2 }),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Entries.Count);
    }

    [Fact]
    public async Task GetSeparateHistoryReport_ReusesSameQueryShape()
    {
        var db = Seed();
        db.PatientTests.Add(Entered(11, 1, 1, DateTime.UtcNow));

        var separate = await new GetSeparateHistoryReportQueryHandler(db).Handle(
            new GetSeparateHistoryReportQuery(1), CancellationToken.None);
        var direct = await new GetPatientTestHistoryQueryHandler(db).Handle(
            new GetPatientTestHistoryQuery(1), CancellationToken.None);

        Assert.True(separate.IsSuccess);
        Assert.True(direct.IsSuccess);
        Assert.Equal(
            direct.Value!.Entries.Select(e => e.PatientTestId),
            separate.Value!.Entries.Select(e => e.PatientTestId));
    }

    [Fact]
    public void PatientHistoryReader_ByPatientName_DoesNotMaterializeAllPatients()
    {
        var source = ResolveVisitPatientsSource();

        Assert.Contains("StartsWith(firstToken)", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Where(p => !p.IsDeleted)\n                .ToList()", source, StringComparison.Ordinal);
    }

    private static string ResolveVisitPatientsSource()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var candidate = Path.Combine(
                dir.FullName, "src", "TopLab.Application", "Features",
                "ReportProduction", "Common", "PatientHistoryReader.cs");
            if (File.Exists(candidate))
            {
                return File.ReadAllText(candidate);
            }

            dir = dir.Parent;
        }

        throw new InvalidOperationException("Could not locate PatientHistoryReader.cs");
    }
}
