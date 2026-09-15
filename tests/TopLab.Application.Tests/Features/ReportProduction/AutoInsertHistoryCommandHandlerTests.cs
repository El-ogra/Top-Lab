using TopLab.Application.Features.ReportProduction.Commands.AutoInsertHistory;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Settings;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Application.Tests.Features.ReportProduction;

public class AutoInsertHistoryCommandHandlerTests
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
        db.Tests.Add(Test.Create(TestId.Create(11), "Urea", "Urea report", "Urea", "UREA", 1, 50m, ResultKind.Simple));
        return db;
    }

    private static PatientTest Row(int ptId, int patientId, int testId, string value,
        DateTime? enteredAt = null, DateTime? reviewedAt = null)
    {
        var pt = PatientTest.Create(PatientTestId.Create(ptId), PatientId.Create(patientId), TestId.Create(testId), 100m);
        pt.EnterResult(value, ResultFlag.Normal, 1, enteredAt ?? DateTime.UtcNow);
        pt.MarkReviewed(2, reviewedAt ?? DateTime.UtcNow);
        return pt;
    }

    [Fact]
    public async Task SwitchFalse_ReturnsSuccessWithEmptyInsertionSet()
    {
        var db = Seed(autoDisplay: false);
        db.PatientTests.Add(Row(101, patientId: 1, testId: 10, value: "90"));
        db.PatientTests.Add(Row(102, patientId: 2, testId: 10, value: "85"));

        var result = await new AutoInsertHistoryCommandHandler(db)
            .Handle(new AutoInsertHistoryCommand(101), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!.Lines);
    }

    [Fact]
    public async Task SwitchTrue_CopiesPriorSameTestResults_AndNeverMutatesSourceRows()
    {
        var db = Seed(autoDisplay: true);
        var current = Row(101, patientId: 1, testId: 10, value: "90", enteredAt: DateTime.UtcNow);
        var prior = Row(102, patientId: 2, testId: 10, value: "85",
            enteredAt: DateTime.UtcNow.AddMinutes(1), reviewedAt: DateTime.UtcNow.AddMinutes(1));
        var otherTest = Row(103, patientId: 2, testId: 11, value: "20");
        db.PatientTests.AddRange(new[] { current, prior, otherTest });

        // Snapshot for the no-mutation pin.
        var priorSnapshot = new
        {
            prior.ResultValue,
            prior.ResultFlag,
            prior.IsReviewed,
            prior.ReviewedByUserId,
            prior.ReviewedAtUtc,
            prior.IsPrinted,
            prior.PrintCount,
            prior.LastPrintedByUserId,
            prior.LastPrintedAtUtc
        };

        var result = await new AutoInsertHistoryCommandHandler(db)
            .Handle(new AutoInsertHistoryCommand(101), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Ali A", result.Value!.PatientFullName);
        var line = Assert.Single(result.Value.Lines);
        Assert.Equal(102, line.PatientTestId);
        Assert.Equal(10, line.TestId);
        Assert.Equal("Glucose", line.TestName);
        Assert.Equal("GLU", line.TestCode);
        Assert.Equal("85", line.ResultValue);

        // No mutator was ever called on any stored PatientTest row.
        var t102 = db.PatientTests.Single(pt => pt.Id.Value == 102);
        Assert.Equal(priorSnapshot.ResultValue, t102.ResultValue);
        Assert.Equal(priorSnapshot.ResultFlag, t102.ResultFlag);
        Assert.Equal(priorSnapshot.IsReviewed, t102.IsReviewed);
        Assert.Equal(priorSnapshot.ReviewedByUserId, t102.ReviewedByUserId);
        Assert.Equal(priorSnapshot.ReviewedAtUtc, t102.ReviewedAtUtc);
        Assert.Equal(priorSnapshot.IsPrinted, t102.IsPrinted);
        Assert.Equal(priorSnapshot.PrintCount, t102.PrintCount);
        Assert.Equal(priorSnapshot.LastPrintedByUserId, t102.LastPrintedByUserId);
        Assert.Equal(priorSnapshot.LastPrintedAtUtc, t102.LastPrintedAtUtc);
    }

    [Fact]
    public async Task SwitchTrue_DifferentTestResult_IsExcluded()
    {
        var db = Seed(autoDisplay: true);
        db.PatientTests.Add(Row(101, patientId: 1, testId: 10, value: "90"));
        db.PatientTests.Add(Row(103, patientId: 2, testId: 11, value: "20"));

        var result = await new AutoInsertHistoryCommandHandler(db)
            .Handle(new AutoInsertHistoryCommand(101), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!.Lines);
    }

    [Fact]
    public async Task SwitchTrue_NoSharedIdentity_ReturnsEmptyInsertionSet()
    {
        var db = Seed(autoDisplay: true);
        db.PatientTests.Add(Row(101, patientId: 1, testId: 10, value: "90"));
        db.PatientTests.Add(Row(104, patientId: 3, testId: 10, value: "70"));

        var result = await new AutoInsertHistoryCommandHandler(db)
            .Handle(new AutoInsertHistoryCommand(101), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!.Lines);
    }

    [Fact]
    public async Task MissingCurrentRow_ReturnsNotFound()
    {
        var db = Seed(autoDisplay: true);

        var result = await new AutoInsertHistoryCommandHandler(db)
            .Handle(new AutoInsertHistoryCommand(999), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("التحليل غير موجود", result.Error!.Message);
    }

    [Fact]
    public async Task MissingPatient_ReturnsNotFound()
    {
        var db = Seed(autoDisplay: true);
        db.PatientTests.Add(Row(101, patientId: 9, testId: 10, value: "90"));

        var result = await new AutoInsertHistoryCommandHandler(db)
            .Handle(new AutoInsertHistoryCommand(101), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("المريض غير موجود.", result.Error!.Message);
    }

    [Fact]
    public async Task MissingSettings_ReturnsUnexpected()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(Patient.Create(PatientId.Create(1), "Ali", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow));
        db.PatientTests.Add(Row(101, patientId: 1, testId: 10, value: "90"));

        var result = await new AutoInsertHistoryCommandHandler(db)
            .Handle(new AutoInsertHistoryCommand(101), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("سجل إعدادات التقرير مفقود.", result.Error!.Message);
    }

    [Fact]
    public async Task UnresolvableIdentity_ReturnsTranslatedConflict()
    {
        var db = new FakeApplicationDbContext();
        db.ReportSettings.Add(ReportSettings.CreateDefault());
        db.Patients.Add(Patient.Create(PatientId.Create(1), "Ali", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow));
        db.Tests.Add(Test.Create(TestId.Create(10), "Glucose", "Glucose report", "Glucose", "GLU", 1, 100m, ResultKind.Simple));
        db.PatientTests.Add(Row(101, patientId: 1, testId: 10, value: "90"));

        var result = await new AutoInsertHistoryCommandHandler(db)
            .Handle(new AutoInsertHistoryCommand(101), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("تعذر تحديد هوية المريض للتاريخ المرضي.", result.Error!.Message);
    }
}