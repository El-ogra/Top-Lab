using TopLab.Application.Features.ProfileResults.Commands.AmendProfileResult;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Results;
using Xunit;

namespace TopLab.Application.Tests.Features.ProfileResults;

public class AmendProfileResultCommandHandlerTests
{
    private static (FakeApplicationDbContext Db, FakeCurrentUserService User, FakeDateTimeProvider Clock, ProfileResultItem Item) BuildPrintedScenario()
    {
        var db = new FakeApplicationDbContext();
        var user = new FakeCurrentUserService { UserId = 7 };
        var clock = new FakeDateTimeProvider { UtcNow = new DateTime(2026, 9, 9, 12, 0, 0, DateTimeKind.Utc) };
        db.Patients.Add(ProfileResultsSeed.MakePatient(1));
        db.Tests.Add(ProfileResultsSeed.MakeSpecializedTest(10));
        ProfileResultsSeed.SeedAnalyteWithRange(db, 1);
        ProfileResultsSeed.AddProfile(db, 10, 1);
        var pt = ProfileResultsSeed.AddProfileTest(db, 101, 1, 10);
        pt.EnterResult(null, null, 1, DateTime.UtcNow);
        pt.MarkReviewed(1, DateTime.UtcNow);
        pt.MarkPrinted(1, DateTime.UtcNow);
        var item = ProfileResultsSeed.AddPrintedItem(db, 501, pt, 1, resultValue: "4");
        return (db, user, clock, item);
    }

    [Fact]
    public async Task Amend_MissingItem_NotFound()
    {
        var (db, user, clock, item) = BuildPrintedScenario();

        var result = await new AmendProfileResultCommandHandler(db, user, clock).Handle(
            new AmendProfileResultCommand(999, "5", "mg", null, null),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("نتيجة البروفايل غير موجودة.", result.Error!.Message);
    }

    [Fact]
    public async Task Amend_UnprintedItem_Conflict()
    {
        var (db, user, clock, item) = BuildPrintedScenario();
        db.ProfileResultItems.Remove(item);
        db.ProfileResultItems.Add(ProfileResultItem.Create(
            ProfileResultItemId.Create(501),
            item.PatientTestId,
            item.AnalyteId,
            "4",
            "mg",
            ProfileResultFlag.Low,
            isVerified: true,
            isPrinted: false));

        var result = await new AmendProfileResultCommandHandler(db, user, clock).Handle(
            new AmendProfileResultCommand(501, "5", "mg", null, null),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("لا يمكن تعديل نتيجة البروفايل قبل الطباعة.", result.Error!.Message);
    }

    [Fact]
    public async Task Amend_EmptyValue_Validation()
    {
        var (db, user, clock, item) = BuildPrintedScenario();

        var result = await new AmendProfileResultCommandHandler(db, user, clock).Handle(
            new AmendProfileResultCommand(501, "   ", "mg", null, null),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("قيمة النتيجة مطلوبة.", result.Error!.Message);
        Assert.Empty(db.ProfileResultAmendments);
    }

    [Fact]
    public async Task Amend_UpdatesActiveItem_AddsAuditRow_SingleSave()
    {
        var (db, user, clock, item) = BuildPrintedScenario();
        var before = db.ProfileResultItems.Single();

        var result = await new AmendProfileResultCommandHandler(db, user, clock).Handle(
            new AmendProfileResultCommand(501, "9.5", "mmol", (int)ProfileResultFlag.High, "تعديل بعد الطباعة"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("9.5", before.ResultValue);
        Assert.Equal("mmol", before.Unit);
        Assert.Equal(ProfileResultFlag.High, before.Flag);
        Assert.Equal(AnalyteId.Create(1), before.AnalyteId);
        Assert.Equal(1, db.SaveChangesCallCount);

        var audit = Assert.Single(db.ProfileResultAmendments);
        Assert.Equal(before.Id, audit.ProfileResultItemId);
        Assert.Equal(7, audit.AmendedByUserId);
        Assert.Equal(clock.UtcNow, audit.AmendedAtUtc);
        Assert.Equal("4", audit.OldResultValue);
        Assert.Equal("mg", audit.OldUnit);
        Assert.Equal(ProfileResultFlag.Low, audit.OldFlag);
        Assert.Equal("9.5", audit.NewResultValue);
        Assert.Equal("mmol", audit.NewUnit);
        Assert.Equal(ProfileResultFlag.High, audit.NewFlag);
        Assert.Equal("تعديل بعد الطباعة", audit.Reason);
    }

    [Fact]
    public async Task Amend_NoUnprint_NoNewVersion_ThinRowPreserved()
    {
        var (db, user, clock, item) = BuildPrintedScenario();
        var before = db.ProfileResultItems.Single();
        var orderBefore = db.PatientTests.Single();

        var result = await new AmendProfileResultCommandHandler(db, user, clock).Handle(
            new AmendProfileResultCommand(501, "6", null, null, null),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(db.ProfileResultItems);
        Assert.True(before.IsPrinted);
        Assert.Equal(0, before.PrintCount);
        Assert.True(orderBefore.IsReviewed);
        Assert.True(orderBefore.IsPrinted);
    }

    [Fact]
    public async Task Amend_SecondAmendment_AppendsSecondAuditRow()
    {
        var (db, user, clock, item) = BuildPrintedScenario();
        var handler = new AmendProfileResultCommandHandler(db, user, clock);

        var first = await handler.Handle(new AmendProfileResultCommand(501, "6", "mg", null, "أ"),
            CancellationToken.None);
        Assert.True(first.IsSuccess);
        var second = await handler.Handle(new AmendProfileResultCommand(501, "7", "mg", null, "ب"),
            CancellationToken.None);

        Assert.True(second.IsSuccess);
        Assert.Equal(2, db.ProfileResultAmendments.Count);
        Assert.Equal("7", db.ProfileResultItems.Single().ResultValue);
        Assert.Equal(2, db.SaveChangesCallCount);
    }
}
