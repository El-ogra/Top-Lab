using TopLab.Application.Common.Interfaces;
using TopLab.Application.Features.UsersAndPermissions.Commands.SignIn;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Users;

namespace TopLab.Application.Tests.Features.UsersAndPermissions;

public class SignInCommandHandlerTests
{
    private static User CreateUser(
        int id,
        string userName,
        string passwordHash,
        bool isActive = true,
        bool isAbsolute = false,
        IEnumerable<Permission>? permissions = null,
        List<UserPermissionGrant>? grantsOut = null)
    {
        var user = User.Create(UserId.Create(id), userName, passwordHash, "secondary_hash_" + id, isAbsolute);
        if (!isActive) user.Deactivate();
        return user;
    }

    [Fact]
    public async Task UnknownUser_ReturnsForbidden_WithSharedMessage()
    {
        var db = new FakeApplicationDbContext();
        var hasher = new FakePasswordHasher();
        var currentUser = new FakeCurrentUserService { IsAuthenticated = false };
        var dateTime = new FakeDateTimeProvider { UtcNow = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc) };
        var handler = new SignInCommandHandler(db, hasher, currentUser, dateTime);

        var result = await handler.Handle(new SignInCommand("unknown", "any"), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("اسم المستخدم أو كلمة المرور غير صحيحة", result.Error!.Message);
    }

    [Fact]
    public async Task CorrectUser_WrongPassword_ReturnsSameForbiddenMessage()
    {
        var db = new FakeApplicationDbContext();
        var hasher = new FakePasswordHasher();
        var currentUser = new FakeCurrentUserService { IsAuthenticated = false };
        var dateTime = new FakeDateTimeProvider();
        var user = CreateUser(1, "ahmed", hasher.Hash("correct"));
        db.Users.Add(user);

        var handler = new SignInCommandHandler(db, hasher, currentUser, dateTime);

        var unknownResult = await handler.Handle(new SignInCommand("unknown", "wrong"), CancellationToken.None);
        var wrongPassResult = await handler.Handle(new SignInCommand("ahmed", "wrong"), CancellationToken.None);

        Assert.False(wrongPassResult.IsSuccess);
        Assert.Equal(unknownResult.Error!.Message, wrongPassResult.Error!.Message);
        Assert.Equal("اسم المستخدم أو كلمة المرور غير صحيحة", wrongPassResult.Error!.Message);
    }

    [Fact]
    public async Task InactiveUser_ReturnsForbidden_InactiveMessage()
    {
        var db = new FakeApplicationDbContext();
        var hasher = new FakePasswordHasher();
        var currentUser = new FakeCurrentUserService { IsAuthenticated = false };
        var dateTime = new FakeDateTimeProvider();
        var hash = hasher.Hash("pass123");
        var user = CreateUser(1, "ahmed", hash, isActive: false);
        db.Users.Add(user);

        var handler = new SignInCommandHandler(db, hasher, currentUser, dateTime);
        var result = await handler.Handle(new SignInCommand("ahmed", "pass123"), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("المستخدم غير مفعل", result.Error!.Message);
    }

    [Fact]
    public async Task Success_PopulatesSession_AndSetsLastLogin_AndCallsSaveChangesOnce()
    {
        var db = new FakeApplicationDbContext();
        var hasher = new FakePasswordHasher();
        var currentUser = new FakeCurrentUserService { IsAuthenticated = false };
        var fakeTime = new DateTime(2026, 6, 15, 10, 30, 0, DateTimeKind.Utc);
        var dateTime = new FakeDateTimeProvider { UtcNow = fakeTime };

        var perm1 = Permission.Create(PermissionId.Create(1), "ADD_EDIT_PATIENT", "desc");
        var perm2 = Permission.Create(PermissionId.Create(2), "DELETE_PATIENT", "desc2");
        db.Permissions.Add(perm1);
        db.Permissions.Add(perm2);

        var hash = hasher.Hash("secret123");
        var user = User.Create(UserId.Create(7), "limitedUser", hash, hasher.Hash("secondary"), isAbsolutePermission: false);
        user.GrantPermission(PermissionId.Create(1));
        db.Users.Add(user);
        db.UserPermissionGrants.Add(new UserPermissionGrant(UserId.Create(7), PermissionId.Create(1)));

        var handler = new SignInCommandHandler(db, hasher, currentUser, dateTime);
        var result = await handler.Handle(new SignInCommand("limitedUser", "secret123"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(7, result.Value!.Id);
        Assert.Equal("limitedUser", result.Value.UserName);
        Assert.False(result.Value.IsAbsolutePermission);
        Assert.Contains("ADD_EDIT_PATIENT", result.Value.GrantedPermissionCodes);
        Assert.DoesNotContain("DELETE_PATIENT", result.Value.GrantedPermissionCodes);

        // Session populated
        Assert.True(currentUser.IsAuthenticated);
        Assert.Equal(7, currentUser.UserId);
        Assert.Equal("limitedUser", currentUser.UserName);
        Assert.False(currentUser.IsAbsolutePermission);
        Assert.True(currentUser.HasPermission("ADD_EDIT_PATIENT"));
        Assert.False(currentUser.HasPermission("DELETE_PATIENT"));

        // LastLoginAtUtc set
        Assert.Equal(fakeTime, user.LastLoginAtUtc);
        Assert.Equal(1, db.SaveChangesCallCount);
    }

    [Fact]
    public async Task Success_AbsoluteUser_MirrorsIsAbsoluteFlag()
    {
        var db = new FakeApplicationDbContext();
        var hasher = new FakePasswordHasher();
        var currentUser = new FakeCurrentUserService { IsAuthenticated = false };
        var dateTime = new FakeDateTimeProvider();

        var hash = hasher.Hash("adminpass");
        var user = User.Create(UserId.Create(1), "admin", hash, hasher.Hash("sec"), isAbsolutePermission: true);
        db.Users.Add(user);

        var handler = new SignInCommandHandler(db, hasher, currentUser, dateTime);
        var result = await handler.Handle(new SignInCommand("admin", "adminpass"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value!.IsAbsolutePermission);
        Assert.True(currentUser.IsAbsolutePermission);
    }

    [Fact]
    public async Task FailedSignIn_DoesNotAlterUserState_AndDoesNotSave()
    {
        var db = new FakeApplicationDbContext();
        var hasher = new FakePasswordHasher();
        var currentUser = new FakeCurrentUserService { IsAuthenticated = false };
        var dateTime = new FakeDateTimeProvider { UtcNow = DateTime.UtcNow };

        var hash = hasher.Hash("correct");
        var user = User.Create(UserId.Create(1), "ahmed", hash, hasher.Hash("sec"));
        db.Users.Add(user);

        var handler = new SignInCommandHandler(db, hasher, currentUser, dateTime);

        var beforeLogin = user.LastLoginAtUtc;
        var result = await handler.Handle(new SignInCommand("ahmed", "wrong"), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(beforeLogin, user.LastLoginAtUtc);
        Assert.Equal(0, db.SaveChangesCallCount);
        Assert.False(currentUser.IsAuthenticated);
    }
}
