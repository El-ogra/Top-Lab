using TopLab.Application.Features.ProfileResults.Commands.UnverifyProfileResults;
using TopLab.Application.Features.ProfileResults.Commands.VerifyProfileResults;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Results;
using Xunit;

namespace TopLab.Application.Tests.Features.ProfileResults;

public class VerifyUnverifyProfileResultsCommandHandlerTests
{
    private static (FakeApplicationDbContext Db, FakeCurrentUserService User, FakeDateTimeProvider Clock, PatientTest Pt) BuildEnteredScenario(
        bool entered = true)
    {
        var db = new FakeApplicationDbContext();
        var user = new FakeCurrentUserService { UserId = 7 };
        var clock = new FakeDateTimeProvider { UtcNow = new DateTime(2026, 9, 9, 12, 0, 0, DateTimeKind.Utc) };
        db.Patients.Add(ProfileResultsSeed.MakePatient(1));
        db.Tests.Add(ProfileResultsSeed.MakeSpecializedTest(10));
        ProfileResultsSeed.SeedAnalyteWithRange(db, 1);
        ProfileResultsSeed.SeedAnalyteWithRange(db, 2);
        ProfileResultsSeed.AddProfile(db, 10, 1, 2);
        var pt = ProfileResultsSeed.AddProfileTest(db, 101, 1, 10);
        if (entered)
        {
            pt.EnterResult(null, null, 1, DateTime.UtcNow);
        }

        db.ProfileResultItems.Add(ProfileResultItem.Create(
            ProfileResultItemId.Create(501),
            pt.Id,
            AnalyteId.Create(1),
            "4",
            "mg",
            null));
        return (db, user, clock, pt);
    }

    [Fact]
    public async Task Verify_MarksReviewed_AndVerifiesItems()
    {
        var (db, user, clock, pt) = BuildEnteredScenario();

        var result = await new VerifyProfileResultsCommandHandler(db, user, clock).Handle(
            new VerifyProfileResultsCommand(101),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(pt.IsReviewed);
        Assert.NotNull(pt.ReviewedByUserId);
        Assert.Equal(7, pt.ReviewedByUserId);
        Assert.True(db.ProfileResultItems.Single().IsVerified);
        Assert.Equal(1, db.SaveChangesCallCount);
    }

    [Fact]
    public async Task Verify_WithoutEntry_Conflict()
    {
        var (db, user, clock, pt) = BuildEnteredScenario(entered: false);

        var result = await new VerifyProfileResultsCommandHandler(db, user, clock).Handle(
            new VerifyProfileResultsCommand(101),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("لا يمكن اعتماد نتيجة غير مدخلة.", result.Error!.Message);
        Assert.False(pt.IsReviewed);
    }

    [Fact]
    public async Task Unverify_AfterVerify_Unreviews()
    {
        var (db, user, clock, pt) = BuildEnteredScenario();
        var verify = await new VerifyProfileResultsCommandHandler(db, user, clock).Handle(
            new VerifyProfileResultsCommand(101),
            CancellationToken.None);
        Assert.True(verify.IsSuccess);

        var result = await new UnverifyProfileResultsCommandHandler(db).Handle(
            new UnverifyProfileResultsCommand(101),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(pt.IsReviewed);
        Assert.False(db.ProfileResultItems.Single().IsVerified);
    }

    [Fact]
    public async Task Unverify_PrintedItem_Conflict()
    {
        var (db, user, clock, pt) = BuildEnteredScenario();
        pt.EnterResult(null, null, 1, DateTime.UtcNow);
        pt.MarkReviewed(1, DateTime.UtcNow);
        pt.MarkPrinted(1, DateTime.UtcNow);

        var result = await new UnverifyProfileResultsCommandHandler(db).Handle(
            new UnverifyProfileResultsCommand(101),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("لا يمكن إلغاء اعتماد نتيجة مطبوعة أو مسلمة.", result.Error!.Message);
        Assert.True(pt.IsReviewed);
    }
}