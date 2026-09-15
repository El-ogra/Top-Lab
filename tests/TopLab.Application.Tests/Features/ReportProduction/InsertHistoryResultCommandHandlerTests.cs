using TopLab.Application.Features.ReportProduction.Commands.InsertHistoryResult;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Settings;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Application.Tests.Features.ReportProduction;

public class InsertHistoryResultCommandHandlerTests
{
    private static FakeApplicationDbContext Seed(bool autoDisplay)
    {
        var db = new FakeApplicationDbContext();
        db.ReportSettings.Add(ReportSettings.CreateDefault());
        db.ReportSettings.Single().SetHistoryOptions(HistorySortMode.ByLabCode, autoDisplay);
        db.Patients.Add(Patient.Create(PatientId.Create(1), "Ali A", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow, labId: LabId.Create("L-1")));
        db.Patients.Add(Patient.Create(PatientId.Create(2), "Ali B", Sex.Male, 32, AgeUnit.Year, DateTime.UtcNow, labId: LabId.Create("L-1")));
        db.Patients.Add(Patient.Create(PatientId.Create(3), "Other", Sex.Male, 40, AgeUnit.Year, DateTime.UtcNow, labId: LabId.Create("L-9")));
        db.Tests.Add(Test.Create(TestId.Create(10), "Glucose", "Glucose report", "Glucose", "GLU", 1, 100m, ResultKind.Simple));
        return db;
    }

    private static PatientTest Row(int ptId, int patientId, int testId, string value)
    {
        var pt = PatientTest.Create(PatientTestId.Create(ptId), PatientId.Create(patientId), TestId.Create(testId), 100m);
        pt.EnterResult(value, ResultFlag.Normal, 1, DateTime.UtcNow);
        pt.MarkReviewed(2, DateTime.UtcNow);
        return pt;
    }

    [Fact]
    public async Task ManualInsert_WorksWhenSwitchOff_SourceRowUntouched()
    {
        var db = Seed(autoDisplay: false);
        var current = Row(101, patientId: 1, testId: 10, value: "90");
        var source = Row(102, patientId: 2, testId: 10, value: "85");
        db.PatientTests.AddRange(new[] { current, source });

        var sourceSnapshot = new { source.ResultValue, source.ResultFlag, source.IsReviewed, source.IsPrinted };

        var result = await new InsertHistoryResultCommandHandler(db)
            .Handle(new InsertHistoryResultCommand(101, 102), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var line = Assert.Single(result.Value!.Lines);
        Assert.Equal(102, line.PatientTestId);
        Assert.Equal("85", line.ResultValue);

        var reread = db.PatientTests.Single(pt => pt.Id.Value == 102);
        Assert.Equal(sourceSnapshot.ResultValue, reread.ResultValue);
        Assert.Equal(sourceSnapshot.ResultFlag, reread.ResultFlag);
        Assert.Equal(sourceSnapshot.IsReviewed, reread.IsReviewed);
        Assert.Equal(sourceSnapshot.IsPrinted, reread.IsPrinted);
    }

    [Fact]
    public async Task ManualInsert_SourceFromDifferentIdentity_Conflict()
    {
        var db = Seed(autoDisplay: false);
        db.PatientTests.Add(Row(101, patientId: 1, testId: 10, value: "90"));
        db.PatientTests.Add(Row(103, patientId: 3, testId: 10, value: "70"));

        var result = await new InsertHistoryResultCommandHandler(db)
            .Handle(new InsertHistoryResultCommand(101, 103), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("النتيجة المحددة لا تنتمي لهذا المريض.", result.Error!.Message);
    }

    [Fact]
    public async Task ManualInsert_SourceNotFound_NotFound()
    {
        var db = Seed(autoDisplay: false);
        db.PatientTests.Add(Row(101, patientId: 1, testId: 10, value: "90"));

        var result = await new InsertHistoryResultCommandHandler(db)
            .Handle(new InsertHistoryResultCommand(101, 999), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("التحليل غير موجود", result.Error!.Message);
    }

    [Fact]
    public async Task ManualInsert_CurrentRowNotFound_NotFound()
    {
        var db = Seed(autoDisplay: false);
        db.PatientTests.Add(Row(102, patientId: 2, testId: 10, value: "85"));

        var result = await new InsertHistoryResultCommandHandler(db)
            .Handle(new InsertHistoryResultCommand(999, 102), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("التحليل غير موجود", result.Error!.Message);
    }

    [Fact]
    public async Task ManualInsert_MissingSettings_Unexpected()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(Patient.Create(PatientId.Create(1), "Ali", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow));
        var current = PatientTest.Create(PatientTestId.Create(101), PatientId.Create(1), TestId.Create(10), 100m);
        current.EnterResult("90", ResultFlag.Normal, 1, DateTime.UtcNow);
        current.MarkReviewed(2, DateTime.UtcNow);
        db.PatientTests.Add(current);

        var result = await new InsertHistoryResultCommandHandler(db)
            .Handle(new InsertHistoryResultCommand(101, 102), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("سجل إعدادات التقرير مفقود.", result.Error!.Message);
    }
}