using TopLab.Application.Features.UsersAndPermissions.Commands.UpdateUser;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Users;

namespace TopLab.Application.Tests.Features.UsersAndPermissions;

public class UpdateUserCommandHandlerTests
{
    private static FakeApplicationDbContext CreateDbWithPermissions()
    {
        var db = new FakeApplicationDbContext();
        var codes = new[] { "ADD_EDIT_PATIENT", "DELETE_PATIENT", "PT_AUDIT_ACCESS" };
        for (int i = 0; i < codes.Length; i++)
        {
            db.Permissions.Add(Permission.Create(PermissionId.Create(i + 1), codes[i], "desc"));
        }
        return db;
    }

    [Fact]
    public async Task Update_AppliesFields_ThroughMutators()
    {
        var db = CreateDbWithPermissions();
        var hasher = new FakePasswordHasher();
        var handler = new UpdateUserCommandHandler(db, hasher);
        var user = User.Create(UserId.Create(1), "old", hasher.Hash("pass123"), hasher.Hash("sec123"), false, 0, false, null, null, false, null);
        db.Users.Add(user);

        var cmd = new UpdateUserCommand(
            UserId: 1,
            UserName: "newname",
            IsAbsolutePermission: true,
            DiscountLimitPercent: 50,
            BlockPrintOnRemainingBalance: true,
            WorkStartTime: new TimeOnly(8, 0),
            WorkEndTime: new TimeOnly(16, 0),
            HasBreakPeriod: true,
            BreakDurationMinutes: 30,
            Password: null,
            SecondaryPassword: null);

        var result = await handler.Handle(cmd, CancellationToken.None);
        Assert.True(result.IsSuccess);
        Assert.Equal("newname", user.UserName);
        Assert.True(user.IsAbsolutePermission);
        Assert.Equal(50, user.DiscountLimitPercent);
        Assert.True(user.BlockPrintOnRemainingBalance);
        Assert.Equal(new TimeOnly(8, 0), user.WorkStartTime);
    }

    [Fact]
    public async Task PasswordHashes_Untouched_WhenNoNewPasswordSupplied()
    {
        var db = CreateDbWithPermissions();
        var hasher = new FakePasswordHasher();
        var handler = new UpdateUserCommandHandler(db, hasher);
        var hash1 = hasher.Hash("oldpass");
        var hash2 = hasher.Hash("oldsec");
        var user = User.Create(UserId.Create(1), "ahmed", hash1, hash2);
        db.Users.Add(user);

        var cmd = new UpdateUserCommand(1, "ahmed", false, 0, false, null, null, false, null, null, null);
        await handler.Handle(cmd, CancellationToken.None);

        Assert.Equal(hash1, user.PasswordHash);
        Assert.Equal(hash2, user.InternalWindowsPasswordHash);
    }

    [Fact]
    public async Task NewPasswords_Rehashed_WhenSupplied()
    {
        var db = CreateDbWithPermissions();
        var hasher = new FakePasswordHasher();
        var handler = new UpdateUserCommandHandler(db, hasher);
        var user = User.Create(UserId.Create(1), "ahmed", hasher.Hash("old"), hasher.Hash("oldsec"));
        db.Users.Add(user);

        var cmd = new UpdateUserCommand(1, "ahmed", false, 0, false, null, null, false, null, "newpass123", "newsec123");
        await handler.Handle(cmd, CancellationToken.None);

        Assert.True(hasher.Verify("newpass123", user.PasswordHash));
        Assert.True(hasher.Verify("newsec123", user.InternalWindowsPasswordHash));
    }

    [Fact]
    public async Task DemotingLastAbsolute_ReturnsConflict()
    {
        var db = CreateDbWithPermissions();
        var hasher = new FakePasswordHasher();
        var handler = new UpdateUserCommandHandler(db, hasher);
        var user = User.Create(UserId.Create(1), "admin", hasher.Hash("p"), hasher.Hash("s"), isAbsolutePermission: true);
        db.Users.Add(user);

        var cmd = new UpdateUserCommand(1, "admin", false, 0, false, null, null, false, null, null, null);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(TopLab.Application.Common.Results.ErrorType.Conflict, result.Error!.Type);
        Assert.Equal("لا يمكن تعطيل آخر مدير نظام؛ يجب إنشاء بديل أولاً", result.Error!.Message);
    }

    [Fact]
    public async Task DemotingAbsolute_WhenAnotherExists_Succeeds()
    {
        var db = CreateDbWithPermissions();
        var hasher = new FakePasswordHasher();
        var handler = new UpdateUserCommandHandler(db, hasher);
        var admin1 = User.Create(UserId.Create(1), "admin1", hasher.Hash("p"), hasher.Hash("s"), true);
        var admin2 = User.Create(UserId.Create(2), "admin2", hasher.Hash("p"), hasher.Hash("s"), true);
        db.Users.Add(admin1);
        db.Users.Add(admin2);

        var cmd = new UpdateUserCommand(1, "admin1", false, 0, false, null, null, false, null, null, null);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(admin1.IsAbsolutePermission);
    }

    [Fact]
    public async Task DuplicateUserName_ReturnsConflict()
    {
        var db = CreateDbWithPermissions();
        var hasher = new FakePasswordHasher();
        var handler = new UpdateUserCommandHandler(db, hasher);
        var u1 = User.Create(UserId.Create(1), "ahmed", hasher.Hash("p"), hasher.Hash("s"));
        var u2 = User.Create(UserId.Create(2), "sara", hasher.Hash("p"), hasher.Hash("s"));
        db.Users.Add(u1);
        db.Users.Add(u2);

        var cmd = new UpdateUserCommand(2, "ahmed", false, 0, false, null, null, false, null, null, null);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("اسم المستخدم موجود بالفعل", result.Error!.Message);
    }
}
