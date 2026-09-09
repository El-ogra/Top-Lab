using TopLab.Application.Features.ProfileResults.Commands.MarkProfilePrinted;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Billing;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Users;
using Xunit;

namespace TopLab.Application.Tests.Features.ProfileResults;

public class MarkProfilePrintedCommandHandlerTests
{
    private static (FakeApplicationDbContext Db, PatientTest Pt, ProfileResultItem Item) BuildScenario(
        bool entered = true,
        bool reviewed = true,
        bool itemVerified = true)
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(ProfileResultsSeed.MakePatient(1));
        db.Tests.Add(ProfileResultsSeed.MakeSpecializedTest(10));
        ProfileResultsSeed.SeedAnalyteWithRange(db, 1);
        ProfileResultsSeed.AddProfile(db, 10, 1);
        var pt = ProfileResultsSeed.AddProfileTest(db, 101, 1, 10);
        if (entered)
        {
            pt.EnterResult(null, null, 1, DateTime.UtcNow);
        }

        if (reviewed)
        {
            pt.MarkReviewed(1, DateTime.UtcNow);
        }

        var item = ProfileResultItem.Create(
            ProfileResultItemId.Create(501),
            pt.Id,
            AnalyteId.Create(1),
            "4",
            "mg",
            null,
            isVerified: itemVerified);
        db.ProfileResultItems.Add(item);
        return (db, pt, item);
    }

    private static void SeedPositiveBalance(FakeApplicationDbContext db, int patientId)
    {
        var extra = PaymentOperation.Create(PaymentOperationId.Create(911), PatientId.Create(patientId), 20m, 1, DateTime.UtcNow, null, true);
        var pay = PaymentOperation.Create(PaymentOperationId.Create(912), PatientId.Create(patientId), 80m, 1, DateTime.UtcNow, 10m);
        var voided = PaymentOperation.Create(PaymentOperationId.Create(913), PatientId.Create(patientId), 999m, 1, DateTime.UtcNow);
        voided.Void();
        db.PaymentOperations.Add(extra);
        db.PaymentOperations.Add(pay);
        db.PaymentOperations.Add(voided);
    }

    [Fact]
    public async Task Print_NotEntered_Conflict()
    {
        var (db, pt, item) = BuildScenario(entered: false, reviewed: false);

        var result = await new MarkProfilePrintedCommandHandler(db, new FakeCurrentUserService { UserId = 7 }, new FakeDateTimeProvider())
            .Handle(new MarkProfilePrintedCommand(101), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("لا يمكن طباعة نتيجة غير معتمدة.", result.Error!.Message);
        Assert.False(pt.IsPrinted);
    }

    [Fact]
    public async Task Print_NotReviewed_Conflict()
    {
        var (db, pt, item) = BuildScenario(entered: true, reviewed: false);

        var result = await new MarkProfilePrintedCommandHandler(db, new FakeCurrentUserService { UserId = 7 }, new FakeDateTimeProvider())
            .Handle(new MarkProfilePrintedCommand(101), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("لا يمكن طباعة نتيجة غير معتمدة.", result.Error!.Message);
        Assert.False(pt.IsPrinted);
    }

    [Fact]
    public async Task Print_Blocked_ByRemainingBalance()
    {
        var (db, pt, item) = BuildScenario();
        SeedPositiveBalance(db, 1);
        db.Users.Add(User.Create(UserId.Create(5), "cashier", "h", "h2", false, 0, true));
        var user = new FakeCurrentUserService { UserId = 5, IsAbsolutePermission = false };

        var result = await new MarkProfilePrintedCommandHandler(db, user, new FakeDateTimeProvider())
            .Handle(new MarkProfilePrintedCommand(101), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("يوجد رصيد متبقٍ على حساب المريض؛ لا يمكن الطباعة.", result.Error!.Message);
        Assert.False(pt.IsPrinted);
        Assert.False(item.IsPrinted);
    }

    [Fact]
    public async Task Print_Allowed_BalanceIgnoredForAbsolute()
    {
        var (db, pt, item) = BuildScenario();
        SeedPositiveBalance(db, 1);
        db.Users.Add(User.Create(UserId.Create(5), "cashier", "h", "h2", false, 0, true));
        var user = new FakeCurrentUserService { UserId = 5, IsAbsolutePermission = true };

        var result = await new MarkProfilePrintedCommandHandler(db, user, new FakeDateTimeProvider())
            .Handle(new MarkProfilePrintedCommand(101), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(pt.IsPrinted);
        Assert.True(item.IsPrinted);
    }

    [Fact]
    public async Task Print_Success_MarksItemsAndOrder()
    {
        var (db, pt, item) = BuildScenario();
        var user = new FakeCurrentUserService { UserId = 7 };
        var clock = new FakeDateTimeProvider { UtcNow = new DateTime(2026, 9, 9, 14, 0, 0, DateTimeKind.Utc) };

        var result = await new MarkProfilePrintedCommandHandler(db, user, clock)
            .Handle(new MarkProfilePrintedCommand(101), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(pt.IsPrinted);
        Assert.Equal(1, pt.PrintCount);
        Assert.True(item.IsPrinted);
        Assert.Equal(1, item.PrintCount);
        Assert.Equal(7, item.LastPrintedByUserId);
        Assert.Equal(clock.UtcNow, item.LastPrintedAtUtc);
        Assert.Equal(1, db.SaveChangesCallCount);
    }

    [Fact]
    public async Task Print_UnverifiedItem_Conflict()
    {
        var (db, pt, item) = BuildScenario(itemVerified: false);

        var result = await new MarkProfilePrintedCommandHandler(db, new FakeCurrentUserService { UserId = 7 }, new FakeDateTimeProvider())
            .Handle(new MarkProfilePrintedCommand(101), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("المادة غير معتمدة؛ لا يمكن طباعتها.", result.Error!.Message);
        Assert.False(pt.IsPrinted);
        Assert.False(item.IsPrinted);
    }
}