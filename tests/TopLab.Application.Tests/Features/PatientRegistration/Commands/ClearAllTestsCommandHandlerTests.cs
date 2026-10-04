using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientRegistration.Commands.ClearAllTests;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using Xunit;

namespace TopLab.Application.Tests.Features.PatientRegistration.Commands;

public class ClearAllTestsCommandHandlerTests
{
    [Fact]
    public async Task Clear_HappyPath_RemovesAll()
    {
        var db = new FakeApplicationDbContext();
        var p = Patient.Create(PatientId.Create(1), "X", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow);
        p.CreatedAtUtc = DateTime.UtcNow;
        db.Patients.Add(p);
        db.PatientTests.Add(PatientTest.Create(PatientTestId.Create(1), PatientId.Create(1), TestId.Create(10), 100m));
        db.PatientTests.Add(PatientTest.Create(PatientTestId.Create(2), PatientId.Create(1), TestId.Create(11), 200m));

        var handler = new ClearAllTestsCommandHandler(db, new FakeDateTimeProvider { UtcNow = DateTime.UtcNow });
        var result = await handler.Handle(new ClearAllTestsCommand(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value);
        Assert.Empty(db.PatientTests);
    }

    [Fact]
    public async Task Clear_TestWithResult_Conflict()
    {
        var db = new FakeApplicationDbContext();
        var p = Patient.Create(PatientId.Create(1), "X", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow);
        db.Patients.Add(p);
        var pt = PatientTest.Create(PatientTestId.Create(1), PatientId.Create(1), TestId.Create(10), 100m);
        pt.EnterResult("5.0", ResultFlag.Normal, 1, DateTime.UtcNow);
        db.PatientTests.Add(pt);

        var handler = new ClearAllTestsCommandHandler(db, new FakeDateTimeProvider());
        var result = await handler.Handle(new ClearAllTestsCommand(1), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
    }

    // =====================================================================
    // R-A04 (a) — the first-registration guard, BR-A04-1…4 (OD-3 = Option A + kept 24h rule)
    // =====================================================================

    private const string FirstVisitMessage = "لا يمكن مسح التحاليل إلا عند إضافة المريض أول مرة.";

    private static readonly DateTime Clock = new(2026, 6, 15, 12, 0, 0, DateTimeKind.Utc);

    private static Patient Visit(
        int id,
        DateTime registered,
        string? labId = null,
        bool deleted = false,
        DateTime? createdAtUtc = null)
    {
        var patient = Patient.Create(
            PatientId.Create(id),
            $"Patient {id}",
            Sex.Male,
            30,
            AgeUnit.Year,
            registered,
            AccountType.Individual,
            false,
            labId is null ? null : LabId.Create(labId));

        patient.CreatedAtUtc = createdAtUtc ?? Clock;

        if (deleted)
        {
            patient.SoftDelete();
        }

        return patient;
    }

    private static void AddFreshTest(FakeApplicationDbContext db, int id, int patientId, int testId)
    {
        db.PatientTests.Add(PatientTest.Create(PatientTestId.Create(id), PatientId.Create(patientId), TestId.Create(testId), 100m));
    }

    private static Task<Result<int>> RunAsync(FakeApplicationDbContext db, int patientId)
    {
        var handler = new ClearAllTestsCommandHandler(db, new FakeDateTimeProvider { UtcNow = Clock });
        return handler.Handle(new ClearAllTestsCommand(patientId), CancellationToken.None);
    }

    [Fact]
    public async Task Clear_FirstVisit_Succeeds()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(Visit(1, Clock.AddHours(-2), labId: "LAB-A"));
        db.Patients.Add(Visit(2, Clock.AddHours(-1), labId: "LAB-B")); // different lab group
        AddFreshTest(db, 1, 1, 10);
        AddFreshTest(db, 2, 1, 11);
        AddFreshTest(db, 3, 2, 10);   // patient 2's own row

        var result = await RunAsync(db, 1);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value);
        Assert.Single(db.PatientTests); // only patient 2's row survives
    }

    [Fact]
    public async Task Clear_NonFirstVisit_Conflict()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(Visit(1, Clock.AddDays(-30), labId: "LAB-A"));
        db.Patients.Add(Visit(2, Clock.AddHours(-1), labId: "LAB-A"));
        AddFreshTest(db, 1, 2, 10);

        var result = await RunAsync(db, 2);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
        Assert.Equal(FirstVisitMessage, result.Error.Message);
    }

    [Fact]
    public async Task Clear_OnlyVisitWithoutLabId_Succeeds()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(Visit(1, Clock.AddHours(-2), labId: null));
        AddFreshTest(db, 1, 1, 10);

        var result = await RunAsync(db, 1);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value);
        Assert.Empty(db.PatientTests);
    }

    [Fact]
    public async Task Clear_SoftDeletedPatient_StillSoftDeleteConflict()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(Visit(1, Clock.AddHours(-2), labId: "LAB-A", deleted: true));
        AddFreshTest(db, 1, 1, 10);

        var result = await RunAsync(db, 1);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
        // The PRE-EXISTING message, not the new one — the soft-delete check runs first.
        Assert.Equal("المريض محذوف.", result.Error.Message);
    }

    [Fact]
    public async Task Clear_ResultEntered_Conflict()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(Visit(1, Clock.AddHours(-2), labId: "LAB-A"));
        var pt = PatientTest.Create(PatientTestId.Create(1), PatientId.Create(1), TestId.Create(10), 100m);
        pt.EnterResult("5.0", ResultFlag.Normal, 1, Clock);
        db.PatientTests.Add(pt);

        var result = await RunAsync(db, 1);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
        // The PRE-EXISTING message still fires for a first visit.
        Assert.Equal("لا يمكن مسح تحاليل تم تسجيل نتائج لها.", result.Error.Message);
    }

    [Fact]
    public async Task Clear_OlderThanTwentyFourHours_StillBlocked()
    {
        var db = new FakeApplicationDbContext();
        // NOTE the asymmetry the plan records at H-10: the pre-existing 24-hour rule reads
        // CreatedAtUtc, NOT RegistrationDateUtc, so BOTH are set 25 h back here.
        db.Patients.Add(Visit(1, Clock.AddHours(-25), labId: "LAB-A", createdAtUtc: Clock.AddHours(-25)));
        AddFreshTest(db, 1, 1, 10);

        var result = await RunAsync(db, 1);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
        // The PRE-EXISTING 24-hour message, on a first visit.
        Assert.Equal("لا يمكن مسح التحاليل من مريض أضيف قبل أكثر من 24 ساعة.", result.Error.Message);
    }

    [Fact]
    public async Task Clear_DeletedPriorVisitDoesNotBlock()
    {
        // Only non-deleted rows count, so a soft-deleted earlier visit does not make this
        // one a returning visit.
        var db = new FakeApplicationDbContext();
        db.Patients.Add(Visit(1, Clock.AddDays(-30), labId: "LAB-A", deleted: true));
        db.Patients.Add(Visit(2, Clock.AddHours(-2), labId: "LAB-A"));
        AddFreshTest(db, 1, 2, 10);

        var result = await RunAsync(db, 2);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value);
        Assert.Empty(db.PatientTests);
    }

    [Fact]
    public async Task Clear_TieOnRegistrationDate_BreaksByPatientId()
    {
        // Two rows share RegistrationDateUtc; the LOWER PatientId is the first visit.
        var sameInstant = Clock.AddHours(-2);
        var db = new FakeApplicationDbContext();
        db.Patients.Add(Visit(1, sameInstant, labId: "LAB-A"));
        db.Patients.Add(Visit(2, sameInstant, labId: "LAB-A"));
        AddFreshTest(db, 1, 2, 10);

        var result = await RunAsync(db, 2);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
        Assert.Equal(FirstVisitMessage, result.Error.Message);
    }

    [Fact]
    public async Task Clear_GuardRunsBeforeTwentyFourHourRule()
    {
        // A non-first visit that is ALSO older than 24 h must report the NEW message,
        // proving the guard sits before the 24-hour check (BR-A04-3).
        var db = new FakeApplicationDbContext();
        db.Patients.Add(Visit(1, Clock.AddDays(-30), labId: "LAB-A"));
        db.Patients.Add(Visit(2, Clock.AddHours(-25), labId: "LAB-A"));
        AddFreshTest(db, 1, 2, 10);

        var result = await RunAsync(db, 2);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
        Assert.Equal(FirstVisitMessage, result.Error.Message);
        Assert.NotEqual("لا يمكن مسح التحاليل من مريض أضيف قبل أكثر من 24 ساعة.", result.Error.Message);
    }

    [Fact]
    public async Task Clear_Guard_PerformsNoWriteOnRefusal()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(Visit(1, Clock.AddDays(-30), labId: "LAB-A"));
        db.Patients.Add(Visit(2, Clock.AddHours(-1), labId: "LAB-A"));
        AddFreshTest(db, 1, 2, 10);

        var result = await RunAsync(db, 2);

        Assert.False(result.IsSuccess);
        Assert.Single(db.PatientTests); // nothing removed
        Assert.Equal(0, db.SaveChangesCallCount); // and nothing saved
    }

    [Fact]
    public async Task Clear_LaterRegistrationDateIsTheFirstVisit()
    {
        // Guards against an inverted comparison: the EARLIER row is the first visit, so the
        // later-registered row is refused and the earlier one is allowed.
        var db = new FakeApplicationDbContext();
        db.Patients.Add(Visit(7, Clock.AddHours(-5), labId: "LAB-A"));
        db.Patients.Add(Visit(8, Clock.AddHours(-1), labId: "LAB-A"));
        AddFreshTest(db, 1, 7, 10);

        var first = await RunAsync(db, 7);
        var second = await RunAsync(db, 8);

        Assert.True(first.IsSuccess);
        Assert.False(second.IsSuccess);
        Assert.Equal(FirstVisitMessage, second.Error!.Message);
    }

    [Fact]
    public void Handler_ExposesTheSharedArabicLiteral()
    {
        // SD-17: one literal, reused byte-for-byte by the Presentation short-circuit.
        Assert.Equal(FirstVisitMessage, ClearAllTestsCommandHandler.FirstRegistrationOnlyMessage);
    }
}