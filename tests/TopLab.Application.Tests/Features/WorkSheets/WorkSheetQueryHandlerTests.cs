using TopLab.Application.Features.WorkSheets.Queries.GetWorkSheetByTestGroup;
using TopLab.Application.Features.WorkSheets.Queries.GetWorkSheetByWorkGroupLog;
using TopLab.Application.Features.WorkSheets.Queries.GetWorkSheetSummary;
using TopLab.Application.Features.WorkSheets.Queries.GetWorkSheetTestCountByPeriod;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Settings;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Application.Tests.Features.WorkSheets;

public class WorkSheetQueryHandlerTests
{
    private static readonly DateOnly Sep1 = new(2026, 9, 1);
    private static readonly DateOnly Sep2 = new(2026, 9, 2);
    private static readonly DateOnly Sep3 = new(2026, 9, 3);

    private static DateTime Noon(DateOnly day) =>
        new(day.Year, day.Month, day.Day, 12, 0, 0, DateTimeKind.Utc);

    private static Patient MakePatient(int id, string name, DateTime regUtc, string? labId = null, bool deleted = false)
    {
        var patient = Patient.Create(
            PatientId.Create(id), name, Sex.Male, 30, AgeUnit.Year, regUtc,
            labId: labId == null ? null : LabId.Create(labId));
        if (deleted)
        {
            patient.SoftDelete();
        }

        return patient;
    }

    private static Test MakeTest(int id, string code, int? groupId = null, string? barcode = null, bool active = true)
    {
        return Test.Create(
            TestId.Create(id), $"Test {code}", $"Test {code}", $"Test {code}", code, 60, 100m,
            testGroupId: groupId.HasValue ? TestGroupId.Create(groupId.Value) : null,
            barcode: barcode,
            isActive: active);
    }

    private static PatientTest MakeLine(
        int ptId, int patientId, int testId, bool outside = false, bool drawn = false, bool resulted = false)
    {
        var pt = PatientTest.Create(
            PatientTestId.Create(ptId), PatientId.Create(patientId), TestId.Create(testId), 100m,
            isTakenOutsideLab: outside);
        if (drawn || resulted)
        {
            pt.MarkSampleDrawn(DateTime.UtcNow);
        }

        if (resulted)
        {
            pt.EnterResult("5.0", null, 1, DateTime.UtcNow);
        }

        return pt;
    }

    private static WorkGroupLog SeedLog(FakeApplicationDbContext db, int id, string name, params int[] testIds)
    {
        var log = WorkGroupLog.Create(WorkGroupLogId.Create(id), name);
        db.WorkGroupLogs.Add(log);
        foreach (var testId in testIds)
        {
            db.WorkGroupLogItems.Add(WorkGroupLogItem.Create(log.Id, TestId.Create(testId)));
        }

        return log;
    }

    [Fact]
    public async Task ByLog_DefaultsBothNullToTodayUtc()
    {
        var db = new FakeApplicationDbContext();
        db.SystemSettings.Add(SystemSettings.CreateDefault());
        db.Tests.Add(MakeTest(10, "T10"));
        db.Patients.Add(MakePatient(1, "Today", DateTime.UtcNow, "L-1"));
        db.PatientTests.Add(MakeLine(11, 1, 10));
        SeedLog(db, 7, "Morning", 10);

        var handler = new GetWorkSheetByWorkGroupLogQueryHandler(db);
        var result = await handler.Handle(new GetWorkSheetByWorkGroupLogQuery(7), CancellationToken.None);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        Assert.True(result.IsSuccess);
        Assert.Equal(today, result.Value!.From);
        Assert.Equal(today, result.Value.To);
        Assert.Equal("WorkGroupLog", result.Value.Mode);
        Assert.Single(result.Value.Sections);
        Assert.Equal(7, result.Value.Sections[0].SectionId);
        Assert.Equal("Morning", result.Value.Sections[0].SectionName);
        Assert.Equal(1, result.Value.TotalTests);
    }

    [Fact]
    public async Task ByLog_FromOnly_SelectsSingleDay()
    {
        var db = new FakeApplicationDbContext();
        db.SystemSettings.Add(SystemSettings.CreateDefault());
        db.Tests.Add(MakeTest(10, "T10"));
        db.Patients.Add(MakePatient(1, "Day1", Noon(Sep1)));
        db.Patients.Add(MakePatient(2, "Day2", Noon(Sep2)));
        db.Patients.Add(MakePatient(3, "Day3", Noon(Sep3)));
        db.PatientTests.Add(MakeLine(11, 1, 10));
        db.PatientTests.Add(MakeLine(12, 2, 10));
        db.PatientTests.Add(MakeLine(13, 3, 10));
        SeedLog(db, 7, "Morning", 10);

        var handler = new GetWorkSheetByWorkGroupLogQueryHandler(db);
        var result = await handler.Handle(new GetWorkSheetByWorkGroupLogQuery(7, From: Sep2), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(Sep2, result.Value!.From);
        Assert.Equal(Sep2, result.Value.To);
        Assert.Single(result.Value.Sections[0].Lines);
        Assert.Equal(12, result.Value.Sections[0].Lines[0].PatientTestId);
    }

    [Fact]
    public async Task ByLog_BoundsAreInclusiveOnBothEnds()
    {
        var db = new FakeApplicationDbContext();
        db.SystemSettings.Add(SystemSettings.CreateDefault());
        db.Tests.Add(MakeTest(10, "T10"));
        db.Patients.Add(MakePatient(1, "Before", new DateTime(2026, 8, 31, 23, 59, 0, DateTimeKind.Utc)));
        db.Patients.Add(MakePatient(2, "Start", new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc)));
        db.Patients.Add(MakePatient(3, "End", new DateTime(2026, 9, 3, 23, 59, 0, DateTimeKind.Utc)));
        db.Patients.Add(MakePatient(4, "After", new DateTime(2026, 9, 4, 0, 0, 0, DateTimeKind.Utc)));
        db.PatientTests.Add(MakeLine(11, 1, 10));
        db.PatientTests.Add(MakeLine(12, 2, 10));
        db.PatientTests.Add(MakeLine(13, 3, 10));
        db.PatientTests.Add(MakeLine(14, 4, 10));
        SeedLog(db, 7, "Morning", 10);

        var handler = new GetWorkSheetByWorkGroupLogQueryHandler(db);
        var result = await handler.Handle(
            new GetWorkSheetByWorkGroupLogQuery(7, From: Sep1, To: Sep3), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal([12, 13], result.Value!.Sections[0].Lines.Select(l => l.PatientTestId));
    }

    [Fact]
    public async Task ByLog_ExcludesOutsideLabTests()
    {
        var db = new FakeApplicationDbContext();
        db.SystemSettings.Add(SystemSettings.CreateDefault());
        db.Tests.Add(MakeTest(10, "T10"));
        db.Patients.Add(MakePatient(1, "InLab", Noon(Sep1)));
        db.PatientTests.Add(MakeLine(11, 1, 10));
        db.PatientTests.Add(MakeLine(12, 1, 10, outside: true));
        SeedLog(db, 7, "Morning", 10);

        var handler = new GetWorkSheetByWorkGroupLogQueryHandler(db);
        var result = await handler.Handle(
            new GetWorkSheetByWorkGroupLogQuery(7, From: Sep1, To: Sep3), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!.Sections[0].Lines);
        Assert.Equal(11, result.Value.Sections[0].Lines[0].PatientTestId);
    }

    [Fact]
    public async Task ByLog_ExcludesSoftDeletedPatients()
    {
        var db = new FakeApplicationDbContext();
        db.SystemSettings.Add(SystemSettings.CreateDefault());
        db.Tests.Add(MakeTest(10, "T10"));
        db.Patients.Add(MakePatient(1, "Deleted", Noon(Sep1), deleted: true));
        db.PatientTests.Add(MakeLine(11, 1, 10));
        SeedLog(db, 7, "Morning", 10);

        var handler = new GetWorkSheetByWorkGroupLogQueryHandler(db);
        var result = await handler.Handle(
            new GetWorkSheetByWorkGroupLogQuery(7, From: Sep1, To: Sep3), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!.Sections[0].Lines);
        Assert.Equal(0, result.Value.TotalTests);
    }

    [Fact]
    public async Task ByLog_LogNotFound()
    {
        var db = new FakeApplicationDbContext();
        db.SystemSettings.Add(SystemSettings.CreateDefault());

        var handler = new GetWorkSheetByWorkGroupLogQueryHandler(db);
        var result = await handler.Handle(
            new GetWorkSheetByWorkGroupLogQuery(999, From: Sep1, To: Sep3), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("سجل مجموعة العمل غير موجود.", result.Error!.Message);
    }

    [Fact]
    public async Task ByLog_MissingSettingsRow_ReturnsUnexpected()
    {
        var db = new FakeApplicationDbContext();
        db.Tests.Add(MakeTest(10, "T10"));
        SeedLog(db, 7, "Morning", 10);

        var handler = new GetWorkSheetByWorkGroupLogQueryHandler(db);
        var result = await handler.Handle(
            new GetWorkSheetByWorkGroupLogQuery(7, From: Sep1, To: Sep3), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("سجل الإعدادات العامة مفقود.", result.Error!.Message);
    }

    [Fact]
    public async Task ByLog_LinesOrderedByRegistrationThenId_WithIdentityPairAndSettingsEcho()
    {
        var db = new FakeApplicationDbContext();
        var settings = SystemSettings.CreateDefault();
        settings.SetGeneralFlags(false, false, false, true, true, true, false, false);
        db.SystemSettings.Add(settings);
        db.Tests.Add(MakeTest(10, "T10", barcode: "BC-10"));
        db.Patients.Add(MakePatient(1, "Early", Noon(Sep1), "L-1"));
        db.Patients.Add(MakePatient(2, "Late", Noon(Sep2), "L-2"));
        db.PatientTests.Add(MakeLine(12, 1, 10));
        db.PatientTests.Add(MakeLine(11, 1, 10));
        db.PatientTests.Add(MakeLine(20, 2, 10));
        SeedLog(db, 7, "Morning", 10);

        var handler = new GetWorkSheetByWorkGroupLogQueryHandler(db);
        var result = await handler.Handle(
            new GetWorkSheetByWorkGroupLogQuery(7, From: Sep1, To: Sep3), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var lines = result.Value!.Sections[0].Lines;
        Assert.Equal([11, 12, 20], lines.Select(l => l.PatientTestId));
        var first = lines[0];
        Assert.Equal(11, first.PatientTestId);
        Assert.Equal(1, first.PatientId);
        Assert.Equal("Early", first.PatientFullName);
        Assert.Equal("L-1", first.LabId);
        Assert.Equal("BC-10", first.Barcode);
        Assert.Equal("T10", first.TestCode);
        Assert.False(first.IsSampleDrawn);
        Assert.False(first.HasResult);
        Assert.True(result.Value.PrintFileExternalBarcode);
        Assert.True(result.Value.PrintDateTimeOnTubeBarcode);
        Assert.True(result.Value.PrintLabIdInsteadOfPatientId);
    }

    [Fact]
    public async Task ByTestGroup_TestIdsMode_SelectsOnlyThose()
    {
        var db = new FakeApplicationDbContext();
        db.SystemSettings.Add(SystemSettings.CreateDefault());
        db.TestGroups.Add(Domain.Tests.TestGroup.Create(TestGroupId.Create(1), "Chem"));
        db.Tests.Add(MakeTest(10, "T10", groupId: 1));
        db.Tests.Add(MakeTest(20, "T20", groupId: 1));
        db.Tests.Add(MakeTest(30, "T30"));
        db.Patients.Add(MakePatient(1, "P", Noon(Sep1)));
        db.PatientTests.Add(MakeLine(11, 1, 10));
        db.PatientTests.Add(MakeLine(12, 1, 20));
        db.PatientTests.Add(MakeLine(13, 1, 30));
        SeedLog(db, 7, "Morning", 10, 20, 30);

        var handler = new GetWorkSheetByTestGroupQueryHandler(db);
        var result = await handler.Handle(
            new GetWorkSheetByTestGroupQuery(TestIds: [10, 30], From: Sep1, To: Sep3),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("TestGroup", result.Value!.Mode);
        Assert.Equal(2, result.Value.TotalTests);
        Assert.Equal(2, result.Value.Sections.Count);
        Assert.Equal("Chem", result.Value.Sections[0].SectionName);
        Assert.Equal("بدون مجموعة", result.Value.Sections[1].SectionName);
        Assert.Equal(0, result.Value.Sections[1].SectionId);
    }

    [Fact]
    public async Task ByTestGroup_UnknownTestId_ReturnsNotFound()
    {
        var db = new FakeApplicationDbContext();
        db.SystemSettings.Add(SystemSettings.CreateDefault());
        db.Tests.Add(MakeTest(10, "T10"));

        var handler = new GetWorkSheetByTestGroupQueryHandler(db);
        var result = await handler.Handle(
            new GetWorkSheetByTestGroupQuery(TestIds: [10, 999], From: Sep1, To: Sep3),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("التحليل غير موجود", result.Error!.Message);
    }

    [Fact]
    public async Task ByTestGroup_GroupMode_SingleSection()
    {
        var db = new FakeApplicationDbContext();
        db.SystemSettings.Add(SystemSettings.CreateDefault());
        db.TestGroups.Add(Domain.Tests.TestGroup.Create(TestGroupId.Create(1), "Chem"));
        db.Tests.Add(MakeTest(10, "T10", groupId: 1));
        db.Tests.Add(MakeTest(20, "T20"));
        db.Patients.Add(MakePatient(1, "P", Noon(Sep1)));
        db.PatientTests.Add(MakeLine(11, 1, 10));
        db.PatientTests.Add(MakeLine(12, 1, 20));

        var handler = new GetWorkSheetByTestGroupQueryHandler(db);
        var result = await handler.Handle(
            new GetWorkSheetByTestGroupQuery(TestGroupId: 1, From: Sep1, To: Sep3),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!.Sections);
        Assert.Equal(1, result.Value.Sections[0].SectionId);
        Assert.Equal("Chem", result.Value.Sections[0].SectionName);
        Assert.Single(result.Value.Sections[0].Lines);
    }

    [Fact]
    public async Task ByTestGroup_GroupNotFound()
    {
        var db = new FakeApplicationDbContext();
        db.SystemSettings.Add(SystemSettings.CreateDefault());

        var handler = new GetWorkSheetByTestGroupQueryHandler(db);
        var result = await handler.Handle(
            new GetWorkSheetByTestGroupQuery(TestGroupId: 999, From: Sep1, To: Sep3),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("مجموعة التحاليل غير موجودة.", result.Error!.Message);
    }

    [Fact]
    public async Task ByTestGroup_ElseMode_ActiveGroupsPlusUngrouped_ExcludesInactiveAndOutside()
    {
        var db = new FakeApplicationDbContext();
        db.SystemSettings.Add(SystemSettings.CreateDefault());
        db.TestGroups.Add(Domain.Tests.TestGroup.Create(TestGroupId.Create(1), "Chem"));
        var dead = Domain.Tests.TestGroup.Create(TestGroupId.Create(2), "Dead");
        dead.Deactivate();
        db.TestGroups.Add(dead);
        db.Tests.Add(MakeTest(10, "T10", groupId: 1));
        db.Tests.Add(MakeTest(20, "T20", groupId: 2));
        db.Tests.Add(MakeTest(30, "T30"));
        db.Patients.Add(MakePatient(1, "P", Noon(Sep1)));
        db.PatientTests.Add(MakeLine(11, 1, 10));
        db.PatientTests.Add(MakeLine(12, 1, 20));
        db.PatientTests.Add(MakeLine(13, 1, 30));
        db.PatientTests.Add(MakeLine(14, 1, 10, outside: true));

        var handler = new GetWorkSheetByTestGroupQueryHandler(db);
        var result = await handler.Handle(
            new GetWorkSheetByTestGroupQuery(From: Sep1, To: Sep3), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Sections.Count);
        Assert.Equal("Chem", result.Value.Sections[0].SectionName);
        Assert.Equal("بدون مجموعة", result.Value.Sections[1].SectionName);
        Assert.Equal(2, result.Value.TotalTests);
        Assert.DoesNotContain(result.Value.Sections, s => s.SectionName == "Dead");
    }

    [Fact]
    public async Task ByTestGroup_MissingSettingsRow_ReturnsUnexpected()
    {
        var db = new FakeApplicationDbContext();
        db.TestGroups.Add(Domain.Tests.TestGroup.Create(TestGroupId.Create(1), "Chem"));

        var handler = new GetWorkSheetByTestGroupQueryHandler(db);
        var result = await handler.Handle(
            new GetWorkSheetByTestGroupQuery(TestGroupId: 1, From: Sep1, To: Sep3),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("سجل الإعدادات العامة مفقود.", result.Error!.Message);
    }

    [Fact]
    public async Task Summary_PartitionsCountsPerLog()
    {
        var db = new FakeApplicationDbContext();
        db.Tests.Add(MakeTest(10, "T10"));
        db.Tests.Add(MakeTest(20, "T20"));
        db.Patients.Add(MakePatient(1, "P", Noon(Sep1)));
        db.PatientTests.Add(MakeLine(11, 1, 10));
        db.PatientTests.Add(MakeLine(12, 1, 10, drawn: true));
        db.PatientTests.Add(MakeLine(13, 1, 10, drawn: true, resulted: true));
        db.PatientTests.Add(MakeLine(14, 1, 10, outside: true));
        db.PatientTests.Add(MakeLine(15, 1, 20));
        SeedLog(db, 7, "Morning", 10);
        SeedLog(db, 8, "Empty");

        var handler = new GetWorkSheetSummaryQueryHandler(db);
        var result = await handler.Handle(
            new GetWorkSheetSummaryQuery(From: Sep1, To: Sep3), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Count);
        var morning = result.Value.Single(r => r.Name == "Morning");
        Assert.Equal(7, morning.WorkGroupLogId);
        Assert.Equal(1, morning.PendingCount);
        Assert.Equal(1, morning.DrawnCount);
        Assert.Equal(1, morning.ResultedCount);
        var empty = result.Value.Single(r => r.Name == "Empty");
        Assert.Equal(0, empty.PendingCount + empty.DrawnCount + empty.ResultedCount);
    }

    [Fact]
    public async Task TestCount_GroupsOrdersAndTotals_IncludesOutsideLab()
    {
        var db = new FakeApplicationDbContext();
        db.Tests.Add(MakeTest(10, "T10"));
        db.Tests.Add(MakeTest(20, "T20"));
        db.Tests.Add(MakeTest(30, "T30"));
        db.Patients.Add(MakePatient(1, "P", Noon(Sep1)));
        db.PatientTests.Add(MakeLine(11, 1, 10));
        db.PatientTests.Add(MakeLine(12, 1, 10));
        db.PatientTests.Add(MakeLine(13, 1, 10, outside: true));
        db.PatientTests.Add(MakeLine(14, 1, 20));
        db.PatientTests.Add(MakeLine(15, 1, 20));
        db.PatientTests.Add(MakeLine(16, 1, 20));
        db.PatientTests.Add(MakeLine(17, 1, 30));

        var handler = new GetWorkSheetTestCountByPeriodQueryHandler(db);
        var result = await handler.Handle(
            new GetWorkSheetTestCountByPeriodQuery(From: Sep1, To: Sep3), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(Sep1, result.Value!.From);
        Assert.Equal(Sep3, result.Value.To);
        Assert.Equal(3, result.Value.Rows.Count);
        Assert.Equal(10, result.Value.Rows[0].TestId);
        Assert.Equal(3, result.Value.Rows[0].Count);
        Assert.Equal(20, result.Value.Rows[1].TestId);
        Assert.Equal(3, result.Value.Rows[1].Count);
        Assert.Equal(30, result.Value.Rows[2].TestId);
        Assert.Equal("T30", result.Value.Rows[2].TestCode);
        Assert.Equal(7, result.Value.TotalCount);
    }

    [Fact]
    public async Task TestCount_ExcludesSoftDeletedPatientsAndOutOfPeriodRows()
    {
        var db = new FakeApplicationDbContext();
        db.Tests.Add(MakeTest(10, "T10"));
        db.Patients.Add(MakePatient(1, "Gone", Noon(Sep1), deleted: true));
        db.Patients.Add(MakePatient(2, "Old", Noon(Sep1).AddDays(-10)));
        db.Patients.Add(MakePatient(3, "Kept", Noon(Sep2)));
        db.PatientTests.Add(MakeLine(11, 1, 10));
        db.PatientTests.Add(MakeLine(12, 2, 10));
        db.PatientTests.Add(MakeLine(13, 3, 10));

        var handler = new GetWorkSheetTestCountByPeriodQueryHandler(db);
        var result = await handler.Handle(
            new GetWorkSheetTestCountByPeriodQuery(From: Sep1, To: Sep3), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!.Rows);
        Assert.Equal(1, result.Value.Rows[0].Count);
        Assert.Equal(1, result.Value.TotalCount);
    }
}
