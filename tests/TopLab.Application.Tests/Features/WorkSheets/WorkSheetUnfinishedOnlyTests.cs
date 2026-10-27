using TopLab.Application.Features.WorkSheets.Queries.GetWorkSheetByTestGroup;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Settings;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Application.Tests.Features.WorkSheets;

/// <summary>
/// GAP-17 (2.1.3.5 / 2.3.1.5.1 / 2.3.1.5.3): the unfinished-only restriction in
/// group modes. Owner-resolved predicate (2026-10-26):
/// a <c>PatientTest</c> is unfinished when <c>EnteredAtUtc == null</c>.
/// <c>ResultValue == null</c> is explicitly NOT the predicate.
/// </summary>
public class WorkSheetUnfinishedOnlyTests
{
    private static readonly DateOnly Day = new(2026, 9, 1);

    private static DateTime Noon(DateOnly day) =>
        new(day.Year, day.Month, day.Day, 12, 0, 0, DateTimeKind.Utc);

    private static Patient MakePatient(int id)
        => Patient.Create(
            PatientId.Create(id), $"Patient{id}", Sex.Male, 30, AgeUnit.Year, Noon(Day));

    private static Test MakeTest(int id, string code, int groupId)
        => Test.Create(
            TestId.Create(id), $"Test {code}", $"Test {code}", $"Test {code}", code, 60, 100m,
            testGroupId: TestGroupId.Create(groupId));

    /// <summary>Unfinished line: no result has been entered (EnteredAtUtc is null).</summary>
    private static PatientTest UnfinishedLine(int ptId, int patientId, int testId)
        => PatientTest.Create(
            PatientTestId.Create(ptId), PatientId.Create(patientId), TestId.Create(testId), 100m);

    /// <summary>Finished line: a result has been entered, so EnteredAtUtc is set.</summary>
    private static PatientTest FinishedLine(int ptId, int patientId, int testId)
    {
        var pt = PatientTest.Create(
            PatientTestId.Create(ptId), PatientId.Create(patientId), TestId.Create(testId), 100m);
        pt.EnterResult("5.0", null, 1, DateTime.UtcNow);
        return pt;
    }

    private static FakeApplicationDbContext Db()
    {
        var db = new FakeApplicationDbContext();
        db.SystemSettings.Add(SystemSettings.CreateDefault());
        db.TestGroups.Add(Domain.Tests.TestGroup.Create(TestGroupId.Create(1), "Chem"));
        db.Tests.Add(MakeTest(10, "T10", 1));
        db.Patients.Add(MakePatient(1));
        db.PatientTests.Add(UnfinishedLine(11, 1, 10));
        db.PatientTests.Add(FinishedLine(12, 1, 10));
        return db;
    }

    [Fact]
    public async Task UnfinishedOnly_True_ReturnsOnlyLinesWithNoEnteredResult()
    {
        var handler = new GetWorkSheetByTestGroupQueryHandler(Db());

        var result = await handler.Handle(
            new GetWorkSheetByTestGroupQuery(TestGroupId: 1, From: Day, To: Day, UnfinishedOnly: true),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        var lines = result.Value!.Sections.SelectMany(s => s.Lines).ToList();
        var line = Assert.Single(lines);
        Assert.Equal(11, line.PatientTestId);
        Assert.False(line.HasResult);
    }

    [Fact]
    public async Task UnfinishedOnly_False_ReturnsAllLines()
    {
        var handler = new GetWorkSheetByTestGroupQueryHandler(Db());

        var result = await handler.Handle(
            new GetWorkSheetByTestGroupQuery(TestGroupId: 1, From: Day, To: Day, UnfinishedOnly: false),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Sections.SelectMany(s => s.Lines).Count());
    }

    [Fact]
    public async Task UnfinishedOnly_Null_ReturnsAllLines()
    {
        var handler = new GetWorkSheetByTestGroupQueryHandler(Db());

        var result = await handler.Handle(
            new GetWorkSheetByTestGroupQuery(TestGroupId: 1, From: Day, To: Day),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Sections.SelectMany(s => s.Lines).Count());
    }

    [Fact]
    public async Task UnfinishedOnly_UsesEnteredAtUtc_NotResultValue()
    {
        // A line that has EnteredAtUtc set but a null ResultValue (a qualitative or
        // cleared-value result that was still formally entered) must be treated as
        // FINISHED — proof the predicate is EnteredAtUtc, not ResultValue.
        var db = new FakeApplicationDbContext();
        db.SystemSettings.Add(SystemSettings.CreateDefault());
        db.TestGroups.Add(Domain.Tests.TestGroup.Create(TestGroupId.Create(1), "Chem"));
        db.Tests.Add(MakeTest(10, "T10", 1));
        db.Patients.Add(MakePatient(1));

        var enteredWithNullValue = PatientTest.Create(
            PatientTestId.Create(11), PatientId.Create(1), TestId.Create(10), 100m);
        enteredWithNullValue.EnterResult(null, null, 1, DateTime.UtcNow);
        db.PatientTests.Add(enteredWithNullValue);

        var handler = new GetWorkSheetByTestGroupQueryHandler(db);
        var result = await handler.Handle(
            new GetWorkSheetByTestGroupQuery(TestGroupId: 1, From: Day, To: Day, UnfinishedOnly: true),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!.Sections.SelectMany(s => s.Lines));
    }
}
