using TopLab.Application.Features.ResultsEntry.Commands.EnterResult;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Settings;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Application.Tests.Features.ResultsEntry;

public class EnterResultCommandHandlerTests
{
    private static Patient MakePatient(int id = 1, Sex sex = Sex.Male, int age = 30, AgeUnit unit = AgeUnit.Year)
    {
        return Patient.Create(PatientId.Create(id), $"P{id}", sex, age, unit, DateTime.UtcNow);
    }

    private static void AddSimpleTest(FakeApplicationDbContext db, int id)
    {
        db.Tests.Add(Test.Create(TestId.Create(id), $"T{id}", $"T{id}", $"T{id}", $"T{id}", 30, 100m, ResultKind.Simple));
    }

    private static void AddProfileTest(FakeApplicationDbContext db, int id)
    {
        db.Tests.Add(Test.Create(TestId.Create(id), $"T{id}", $"T{id}", $"T{id}", $"T{id}", 30, 100m, ResultKind.SpecializedProfile));
    }

    private static (FakeApplicationDbContext Db, FakeCurrentUserService User, FakeDateTimeProvider Clock) Build(bool autoReview = false)
    {
        var db = new FakeApplicationDbContext();
        var user = new FakeCurrentUserService { UserId = 7 };
        var clock = new FakeDateTimeProvider { UtcNow = new DateTime(2026, 9, 8, 12, 0, 0, DateTimeKind.Utc) };
        var settings = SystemSettings.CreateDefault();
        if (autoReview)
        {
            settings.SetGeneralFlags(false, false, false, false, false, false, true, false);
        }

        db.SystemSettings.Add(settings);
        return (db, user, clock);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task EmptyOrWhitespace_Rejected_BeforeMutation_D4(string? value)
    {
        var (db, user, clock) = Build();
        db.Patients.Add(MakePatient());
        AddSimpleTest(db, 10);
        var pt = PatientTest.Create(PatientTestId.Create(101), PatientId.Create(1), TestId.Create(10), 100m);
        db.PatientTests.Add(pt);

        var handler = new EnterResultCommandHandler(db, user, clock);
        var result = await handler.Handle(new EnterResultCommand(101, value), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("الرجاء إدخال قيمة النتيجة قبل الحفظ", result.Error!.Message);
        Assert.Null(pt.ResultValue);
        Assert.Null(pt.EnteredAtUtc);
        Assert.Equal(0, db.SaveChangesCallCount);
    }

    [Theory]
    [InlineData("5.5")]
    [InlineData("Positive")]
    [InlineData("+++")]
    [InlineData("  7  ")]
    public async Task NonWhitespace_NumericAndSymbolic_Accepted_D4(string value)
    {
        var (db, user, clock) = Build();
        db.Patients.Add(MakePatient());
        AddSimpleTest(db, 10);
        db.PatientTests.Add(PatientTest.Create(PatientTestId.Create(101), PatientId.Create(1), TestId.Create(10), 100m));

        var handler = new EnterResultCommandHandler(db, user, clock);
        var result = await handler.Handle(new EnterResultCommand(101, value), CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task WrongKind_Rejected()
    {
        var (db, user, clock) = Build();
        db.Patients.Add(MakePatient());
        AddProfileTest(db, 11);
        db.PatientTests.Add(PatientTest.Create(PatientTestId.Create(101), PatientId.Create(1), TestId.Create(11), 100m));

        var handler = new EnterResultCommandHandler(db, user, clock);
        var result = await handler.Handle(new EnterResultCommand(101, "5"), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("لا يمكن إدخال نتيجة إلا لتحليل بسيط.", result.Error!.Message);
    }

    [Fact]
    public async Task Reviewed_Rejected()
    {
        var (db, user, clock) = Build();
        db.Patients.Add(MakePatient());
        AddSimpleTest(db, 10);
        var pt = PatientTest.Create(PatientTestId.Create(101), PatientId.Create(1), TestId.Create(10), 100m);
        pt.EnterResult("1", ResultFlag.Normal, 1, DateTime.UtcNow);
        pt.MarkReviewed(1, DateTime.UtcNow);
        db.PatientTests.Add(pt);

        var handler = new EnterResultCommandHandler(db, user, clock);
        var result = await handler.Handle(new EnterResultCommand(101, "2"), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("النتيجة معتمدة؛ ألغِ الاعتماد أولاً.", result.Error!.Message);
    }

    [Fact]
    public async Task Flag_AutoComputed_When_NotSupplied()
    {
        var (db, user, clock) = Build();
        db.Patients.Add(MakePatient(1, Sex.Male, 30, AgeUnit.Year));
        AddSimpleTest(db, 10);
        db.ReferenceRanges.Add(ReferenceRange.Create(ReferenceRangeId.Create(1), TestId.Create(10), AgeUnit.Year, 0, 100, 4m, 10m));
        db.PatientTests.Add(PatientTest.Create(PatientTestId.Create(101), PatientId.Create(1), TestId.Create(10), 100m));

        var handler = new EnterResultCommandHandler(db, user, clock);
        var result = await handler.Handle(new EnterResultCommand(101, "12"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(ResultFlag.High, db.PatientTests[0].ResultFlag);
        Assert.Single(db.PatientTestReferenceRangeSnapshots);
    }

    [Fact]
    public async Task Flag_OperatorOverride_Wins()
    {
        var (db, user, clock) = Build();
        db.Patients.Add(MakePatient());
        AddSimpleTest(db, 10);
        db.ReferenceRanges.Add(ReferenceRange.Create(ReferenceRangeId.Create(1), TestId.Create(10), AgeUnit.Year, 0, 100, 4m, 10m));
        db.PatientTests.Add(PatientTest.Create(PatientTestId.Create(101), PatientId.Create(1), TestId.Create(10), 100m));

        var handler = new EnterResultCommandHandler(db, user, clock);
        var result = await handler.Handle(new EnterResultCommand(101, "12", (int)ResultFlag.Normal), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(ResultFlag.Normal, db.PatientTests[0].ResultFlag);
    }

    [Fact]
    public async Task Snapshot_Deleted_When_NoMatch()
    {
        var (db, user, clock) = Build();
        db.Patients.Add(MakePatient());
        AddSimpleTest(db, 10);
        // No ranges at all → no match.
        var pt = PatientTest.Create(PatientTestId.Create(101), PatientId.Create(1), TestId.Create(10), 100m);
        db.PatientTests.Add(pt);
        var stale = new ReferenceRangeSnapshot(10, null, AgeUnit.Year, 0, 100, 1m, 2m, null, null, DateTimeOffset.UtcNow);
        db.PatientTestReferenceRangeSnapshots.Add(PatientTestReferenceRangeSnapshot.FromSnapshot(PatientTestId.Create(101), stale));

        var handler = new EnterResultCommandHandler(db, user, clock);
        var result = await handler.Handle(new EnterResultCommand(101, "5"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(db.PatientTestReferenceRangeSnapshots);
    }

    [Fact]
    public async Task AutoReview_Off_Leaves_Unreviewed()
    {
        var (db, user, clock) = Build(autoReview: false);
        db.Patients.Add(MakePatient());
        AddSimpleTest(db, 10);
        db.PatientTests.Add(PatientTest.Create(PatientTestId.Create(101), PatientId.Create(1), TestId.Create(10), 100m));

        var handler = new EnterResultCommandHandler(db, user, clock);
        var result = await handler.Handle(new EnterResultCommand(101, "5"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(db.PatientTests[0].IsReviewed);
    }

    [Fact]
    public async Task AutoReview_On_Reviews_AsSystemAction()
    {
        var (db, user, clock) = Build(autoReview: true);
        db.Patients.Add(MakePatient());
        AddSimpleTest(db, 10);
        db.PatientTests.Add(PatientTest.Create(PatientTestId.Create(101), PatientId.Create(1), TestId.Create(10), 100m));

        var handler = new EnterResultCommandHandler(db, user, clock);
        var result = await handler.Handle(new EnterResultCommand(101, "5"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(db.PatientTests[0].IsReviewed);
        Assert.Equal(7, db.PatientTests[0].ReviewedByUserId);
    }

    [Fact]
    public async Task MissingTest_NotFound()
    {
        var (db, user, clock) = Build();
        var handler = new EnterResultCommandHandler(db, user, clock);
        var result = await handler.Handle(new EnterResultCommand(999, "5"), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("التحليل غير موجود", result.Error!.Message);
    }
}
