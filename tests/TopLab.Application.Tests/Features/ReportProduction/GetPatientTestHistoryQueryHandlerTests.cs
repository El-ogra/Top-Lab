using TopLab.Application.Features.ReportProduction.Queries.GetPatientTestHistory;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Settings;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Application.Tests.Features.ReportProduction;

public class GetPatientTestHistoryQueryHandlerTests
{
    [Fact]
    public async Task ByLabCode_RollsUpAllVisitsOfSameLabId()
    {
        var db = SeedSettings();
        db.Patients.Add(Patient.Create(PatientId.Create(1), "Ali A", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow, labId: LabId.Create("L-1")));
        db.Patients.Add(Patient.Create(PatientId.Create(2), "Ali B", Sex.Male, 32, AgeUnit.Year, DateTime.UtcNow, labId: LabId.Create("L-1")));
        db.Patients.Add(Patient.Create(PatientId.Create(3), "Other", Sex.Male, 40, AgeUnit.Year, DateTime.UtcNow, labId: LabId.Create("L-9")));
        db.Tests.Add(Test.Create(TestId.Create(10), "Glucose", "Glucose report", "Glucose", "GLU", 1, 100m, ResultKind.Simple));
        db.PatientTests.Add(Reviewed(101, patientId: 1, testId: 10, value: "90"));
        db.PatientTests.Add(Reviewed(102, patientId: 2, testId: 10, value: "85", reviewedAt: DateTime.UtcNow.AddMinutes(1)));
        db.PatientTests.Add(Reviewed(103, patientId: 3, testId: 10, value: "70"));

        var result = await new GetPatientTestHistoryQueryHandler(db)
            .Handle(new GetPatientTestHistoryQuery(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Entries.Count);
        Assert.Equal(new[] { 1, 2 }, result.Value.Entries.Select(e => e.PatientId));
        Assert.Equal(new[] { 101, 102 }, result.Value.Entries.Select(e => e.PatientTestId));
        Assert.Equal("ByLabCode", result.Value.HistorySortMode);
        Assert.True(result.Value.HistoryAutoDisplayEnabled);
    }

    [Fact]
    public async Task ByPatientName_MatchesNormalizedNameVariants()
    {
        var db = SeedSettings();
        db.ReportSettings.Single().SetHistoryOptions(HistorySortMode.ByPatientName, true);
        db.Patients.Add(Patient.Create(PatientId.Create(1), "  Ahmed   Mohamed ", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow));
        db.Patients.Add(Patient.Create(PatientId.Create(2), "AHMED MOHAMED", Sex.Male, 31, AgeUnit.Year, DateTime.UtcNow));
        db.Patients.Add(Patient.Create(PatientId.Create(3), "Ahmed Omar", Sex.Male, 40, AgeUnit.Year, DateTime.UtcNow));
        db.Tests.Add(Test.Create(TestId.Create(10), "Glucose", "Glucose report", "Glucose", "GLU", 1, 100m, ResultKind.Simple));
        db.PatientTests.Add(Reviewed(101, patientId: 1, testId: 10, value: "90"));
        db.PatientTests.Add(Reviewed(102, patientId: 2, testId: 10, value: "85", reviewedAt: DateTime.UtcNow.AddMinutes(1)));
        db.PatientTests.Add(Reviewed(103, patientId: 3, testId: 10, value: "70"));

        var result = await new GetPatientTestHistoryQueryHandler(db)
            .Handle(new GetPatientTestHistoryQuery(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(new[] { 1, 2 }, result.Value!.Entries.Select(e => e.PatientId));
        Assert.Equal(new[] { 101, 102 }, result.Value.Entries.Select(e => e.PatientTestId));
    }

    [Fact]
    public async Task MissingSettings_ReturnsUnexpected()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(Patient.Create(PatientId.Create(1), "Ali", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow));

        var result = await new GetPatientTestHistoryQueryHandler(db)
            .Handle(new GetPatientTestHistoryQuery(1), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("سجل إعدادات التقرير مفقود.", result.Error!.Message);
    }

    [Fact]
    public async Task UnresolvableIdentity_ReturnsTranslatedConflict()
    {
        var db = SeedSettings();
        db.Patients.Add(Patient.Create(PatientId.Create(1), "Ali", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow));

        var result = await new GetPatientTestHistoryQueryHandler(db)
            .Handle(new GetPatientTestHistoryQuery(1), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("تعذر تحديد هوية المريض للتاريخ المرضي.", result.Error!.Message);
    }

    [Fact]
    public async Task MissingPatient_ReturnsNotFound()
    {
        var db = SeedSettings();

        var result = await new GetPatientTestHistoryQueryHandler(db)
            .Handle(new GetPatientTestHistoryQuery(42), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("المريض غير موجود.", result.Error!.Message);
    }

    [Fact]
    public async Task NoSharedIdentity_ReturnsEmptyEntries()
    {
        var db = SeedSettings();
        db.Patients.Add(Patient.Create(PatientId.Create(1), "Ali", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow, labId: LabId.Create("L-1")));
        db.Patients.Add(Patient.Create(PatientId.Create(2), "Other", Sex.Male, 40, AgeUnit.Year, DateTime.UtcNow, labId: LabId.Create("L-9")));
        db.Tests.Add(Test.Create(TestId.Create(10), "Glucose", "Glucose report", "Glucose", "GLU", 1, 100m, ResultKind.Simple));
        db.PatientTests.Add(Reviewed(101, patientId: 2, testId: 10, value: "70"));

        var result = await new GetPatientTestHistoryQueryHandler(db)
            .Handle(new GetPatientTestHistoryQuery(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!.Entries);
    }

    private static FakeApplicationDbContext SeedSettings()
    {
        var db = new FakeApplicationDbContext();
        db.ReportSettings.Add(ReportSettings.CreateDefault());
        return db;
    }

    internal static PatientTest Reviewed(int ptId, int patientId, int testId, string value, DateTime? reviewedAt = null)
    {
        var pt = PatientTest.Create(PatientTestId.Create(ptId), PatientId.Create(patientId), TestId.Create(testId), 100m);
        pt.EnterResult(value, ResultFlag.Normal, 1, DateTime.UtcNow);
        pt.MarkReviewed(2, reviewedAt ?? DateTime.UtcNow);
        return pt;
    }
}