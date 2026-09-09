using TopLab.Application.Features.ProfileResults.Commands.SaveProfileResults;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Results;
using Xunit;

namespace TopLab.Application.Tests.Features.ProfileResults;

public class SaveProfileResultsCommandHandlerTests
{
    private static (FakeApplicationDbContext Db, FakeCurrentUserService User, FakeDateTimeProvider Clock) Build()
    {
        var db = new FakeApplicationDbContext();
        var user = new FakeCurrentUserService { UserId = 7 };
        var clock = new FakeDateTimeProvider { UtcNow = new DateTime(2026, 9, 9, 12, 0, 0, DateTimeKind.Utc) };
        return (db, user, clock);
    }

    private static (FakeApplicationDbContext Db, FakeCurrentUserService User, FakeDateTimeProvider Clock, PatientTest Pt) BuildProfileScenario(
        int patientTestId = 101,
        int patientId = 1,
        int testId = 10)
    {
        var (db, user, clock) = Build();
        db.Patients.Add(ProfileResultsSeed.MakePatient(patientId));
        db.Tests.Add(ProfileResultsSeed.MakeSpecializedTest(testId));
        ProfileResultsSeed.SeedAnalyteWithRange(db, 1);
        ProfileResultsSeed.SeedAnalyteWithRange(db, 2);
        ProfileResultsSeed.AddProfile(db, testId, 1, 2);
        var pt = ProfileResultsSeed.AddProfileTest(db, patientTestId, patientId, testId);
        return (db, user, clock, pt);
    }

    [Fact]
    public async Task Save_EmptyItems_Validation()
    {
        var (db, user, clock, pt) = BuildProfileScenario();
        var handler = new SaveProfileResultsCommandHandler(db, user, clock);

        var result = await handler.Handle(new SaveProfileResultsCommand(pt.Id.Value, null, Array.Empty<ProfileItemInput>()), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("يجب إدخال نتيجة مادة واحدة على الأقل.", result.Error!.Message);
        Assert.Equal(0, db.SaveChangesCallCount);
    }

    [Fact]
    public async Task Save_NonConfiguredAnalyte_Rejected()
    {
        var (db, user, clock, pt) = BuildProfileScenario();
        var handler = new SaveProfileResultsCommandHandler(db, user, clock);

        var result = await handler.Handle(
            new SaveProfileResultsCommand(pt.Id.Value, null, new[] { new ProfileItemInput(99, "5", "mg", null) }),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("المادة التحليلية غير ضمن مكونات البروفايل.", result.Error!.Message);
        Assert.Equal(0, db.SaveChangesCallCount);
    }

    [Fact]
    public async Task Save_ReviewedOrder_Rejected()
    {
        var (db, user, clock, pt) = BuildProfileScenario();
        pt.EnterResult(null, null, 1, DateTime.UtcNow);
        pt.MarkReviewed(1, DateTime.UtcNow);
        var handler = new SaveProfileResultsCommandHandler(db, user, clock);

        var result = await handler.Handle(
            new SaveProfileResultsCommand(pt.Id.Value, null, new[] { new ProfileItemInput(1, "5", "mg", null) }),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("النتيجة معتمدة؛ ألغِ الاعتماد أولاً.", result.Error!.Message);
        Assert.Equal(0, db.SaveChangesCallCount);
    }

    [Fact]
    public async Task Save_MissingTest_NotFound()
    {
        var (db, user, clock) = Build();
        var handler = new SaveProfileResultsCommandHandler(db, user, clock);

        var result = await handler.Handle(
            new SaveProfileResultsCommand(999, null, new[] { new ProfileItemInput(1, "5", "mg", null) }),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("التحليل غير موجود", result.Error!.Message);
    }

    [Fact]
    public async Task Save_ReplacesDrafts_CapturesSnapshot_SetsEntryState()
    {
        var (db, user, clock, pt) = BuildProfileScenario();

        var first = await new SaveProfileResultsCommandHandler(db, user, clock).Handle(
            new SaveProfileResultsCommand(pt.Id.Value, "بدون ملاحظات", new[] { new ProfileItemInput(1, "3.2", "mg", null) }),
            CancellationToken.None);
        Assert.True(first.IsSuccess);

        var put = db.ProfileResultItems.Single();
        var snap = db.ProfileResultItemReferenceRangeSnapshots.Single();
        Assert.Equal(1m, snap.MinValue);
        Assert.Equal(5m, snap.MaxValue);
        Assert.Null(snap.Sex);
        Assert.Equal("بدون ملاحظات", pt.Notes);
        Assert.NotNull(pt.EnteredAtUtc);

        var second = await new SaveProfileResultsCommandHandler(db, user, clock).Handle(
            new SaveProfileResultsCommand(pt.Id.Value, "محدث", new[] { new ProfileItemInput(2, "7.5", "mg", (int)ProfileResultFlag.High) }),
            CancellationToken.None);
        Assert.True(second.IsSuccess);

        var replaced = Assert.Single(db.ProfileResultItems);
        Assert.Equal(AnalyteId.Create(2), replaced.AnalyteId);
        Assert.Equal("7.5", replaced.ResultValue);
        Assert.Equal(ProfileResultFlag.High, replaced.Flag);
        var snap2 = Assert.Single(db.ProfileResultItemReferenceRangeSnapshots);
        Assert.Equal(1m, snap2.MinValue);
        Assert.Equal(5m, snap2.MaxValue);
        Assert.Equal("محدث", pt.Notes);
    }

    [Fact]
    public async Task Save_NoMatchingBand_NoSnapshot()
    {
        var (db, user, clock, pt) = BuildProfileScenario();
        var rangeForAnalyte1 = db.AnalyteReferenceRanges.Single(r => r.AnalyteId.Equals(AnalyteId.Create(1)));
        var bandsOfAnalyte1 = db.AnalyteReferenceRangeBands
            .Where(b => b.AnalyteReferenceRangeId.Equals(rangeForAnalyte1.Id))
            .ToList();
        foreach (var band in bandsOfAnalyte1)
        {
            db.AnalyteReferenceRangeBands.Remove(band);
        }

        var result = await new SaveProfileResultsCommandHandler(db, user, clock).Handle(
            new SaveProfileResultsCommand(pt.Id.Value, null, new[] { new ProfileItemInput(1, "4", "mg", null) }),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(db.ProfileResultItems);
        Assert.Empty(db.ProfileResultItemReferenceRangeSnapshots);
    }

    [Fact]
    public async Task Save_KeepsPrintedRows_PreservesTheirIdentity()
    {
        var (db, user, clock, pt) = BuildProfileScenario();
        var printed = ProfileResultsSeed.AddPrintedItem(db, 601, pt, 2);
        var draft = ProfileResultItem.Create(
            ProfileResultItemId.Create(602),
            pt.Id,
            AnalyteId.Create(1),
            "old",
            "mg",
            null);
        db.ProfileResultItems.Add(draft);

        var result = await new SaveProfileResultsCommandHandler(db, user, clock).Handle(
            new SaveProfileResultsCommand(pt.Id.Value, null, new[] { new ProfileItemInput(1, "new", "mg", null) }),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, db.ProfileResultItems.Count);
        Assert.Contains(db.ProfileResultItems, i => i.Id.Equals(printed.Id) && i.ResultValue == "4" && i.IsPrinted);
        Assert.DoesNotContain(db.ProfileResultItems, i => i.ResultValue == "old");
        Assert.Contains(db.ProfileResultItems, i => i.ResultValue == "new");
    }

    [Fact]
    public async Task Save_SingleSaveAcrossItemsAndSnapshot()
    {
        var (db, user, clock, pt) = BuildProfileScenario();

        var result = await new SaveProfileResultsCommandHandler(db, user, clock).Handle(
            new SaveProfileResultsCommand(pt.Id.Value, null, new[]
            {
                new ProfileItemInput(1, "3", "mg", null),
                new ProfileItemInput(2, "8", "mg", (int)ProfileResultFlag.High),
            }),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, db.ProfileResultItems.Count);
        Assert.Equal(2, db.ProfileResultItemReferenceRangeSnapshots.Count);
        Assert.Equal(1, db.SaveChangesCallCount);
    }
}