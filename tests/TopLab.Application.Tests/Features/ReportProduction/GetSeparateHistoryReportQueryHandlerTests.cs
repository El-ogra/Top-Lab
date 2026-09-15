using TopLab.Application.Features.ReportProduction.Queries.GetSeparateHistoryReport;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Settings;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Application.Tests.Features.ReportProduction;

public class GetSeparateHistoryReportQueryHandlerTests
{
    private static FakeApplicationDbContext Seed(
        bool autoDisplay = true, HistorySortMode? sortMode = null)
    {
        var db = new FakeApplicationDbContext();
        var rn = ReportSettings.CreateDefault();
        rn.SetHistoryOptions(sortMode ?? HistorySortMode.ByLabCode, autoDisplay);
        db.ReportSettings.Add(rn);
        return db;
    }

    [Fact]
    public async Task ByLabCode_ResolvesSharedVisitPatients_AndBuildsEntries()
    {
        var db = Seed();
        db.Patients.Add(Patient.Create(PatientId.Create(1), "Ali A", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow, labId: LabId.Create("L-1")));
        db.Patients.Add(Patient.Create(PatientId.Create(2), "Ali B", Sex.Male, 32, AgeUnit.Year, DateTime.UtcNow, labId: LabId.Create("L-1")));
        db.Patients.Add(Patient.Create(PatientId.Create(3), "Other", Sex.Male, 40, AgeUnit.Year, DateTime.UtcNow, labId: LabId.Create("L-9")));
        db.Tests.Add(Test.Create(TestId.Create(10), "Glucose", "Glucose report", "Glucose", "GLU", 1, 100m, ResultKind.Simple));
        db.PatientTests.Add(Reviewed(101, patientId: 1, testId: 10, value: "90"));
        db.PatientTests.Add(Reviewed(102, patientId: 2, testId: 10, value: "85", reviewedAt: DateTime.UtcNow.AddMinutes(1)));
        db.PatientTests.Add(Reviewed(103, patientId: 3, testId: 10, value: "70"));

        var result = await new GetSeparateHistoryReportQueryHandler(db)
            .Handle(new GetSeparateHistoryReportQuery(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(new[] { 1, 2 }, result.Value!.Entries.Select(e => e.PatientId));
        Assert.Equal(new[] { 101, 102 }, result.Value.Entries.Select(e => e.PatientTestId));
        Assert.Equal("ByLabCode", result.Value.HistorySortMode.ToString());
        Assert.True(result.Value.HistoryAutoDisplayEnabled);
    }

    [Fact]
    public async Task MissingSettings_ReturnsUnexpected()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(Patient.Create(PatientId.Create(1), "Ali", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow));

        var result = await new GetSeparateHistoryReportQueryHandler(db)
            .Handle(new GetSeparateHistoryReportQuery(1), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("سجل إعدادات التقرير مفقود.", result.Error!.Message);
    }

    [Fact]
    public async Task UnresolvableIdentity_ReturnsTranslatedConflict()
    {
        var db = Seed();
        db.Patients.Add(Patient.Create(PatientId.Create(1), "Ali", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow));

        var result = await new GetSeparateHistoryReportQueryHandler(db)
            .Handle(new GetSeparateHistoryReportQuery(1), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("تعذر تحديد هوية المريض للتاريخ المرضي.", result.Error!.Message);
    }

    [Fact]
    public async Task MissingPatient_ReturnsNotFound()
    {
        var db = Seed();

        var result = await new GetSeparateHistoryReportQueryHandler(db)
            .Handle(new GetSeparateHistoryReportQuery(42), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("المريض غير موجود.", result.Error!.Message);
    }

    private static PatientTest Reviewed(int ptId, int patientId, int testId, string value, DateTime? reviewedAt = null)
    {
        var pt = PatientTest.Create(PatientTestId.Create(ptId), PatientId.Create(patientId), TestId.Create(testId), 100m);
        pt.EnterResult(value, ResultFlag.Normal, 1, DateTime.UtcNow);
        pt.MarkReviewed(2, reviewedAt ?? DateTime.UtcNow);
        return pt;
    }
}
