using TopLab.Application.Features.UsersAndPermissions.Commands.DeactivateUser;
using TopLab.Application.Features.UsersAndPermissions.Commands.ReactivateUser;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Users;

namespace TopLab.Application.Tests.Features.UsersAndPermissions;

public class DeactivateUserCommandHandlerTests
{
    [Fact]
    public async Task DeactivatingLastAbsolute_ReturnsConflict()
    {
        var db = new FakeApplicationDbContext();
        var hasher = new FakePasswordHasher();
        var admin = User.Create(UserId.Create(1), "admin", hasher.Hash("p"), hasher.Hash("s"), true);
        db.Users.Add(admin);
        var handler = new DeactivateUserCommandHandler(db);

        var result = await handler.Handle(new DeactivateUserCommand(1), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("لا يمكن تعطيل آخر مدير نظام؛ يجب إنشاء بديل أولاً", result.Error!.Message);
        Assert.True(admin.IsActive);
    }

    [Fact]
    public async Task DeactivatingAbsolute_WhenAnotherExists_Succeeds()
    {
        var db = new FakeApplicationDbContext();
        var hasher = new FakePasswordHasher();
        var a1 = User.Create(UserId.Create(1), "admin1", hasher.Hash("p"), hasher.Hash("s"), true);
        var a2 = User.Create(UserId.Create(2), "admin2", hasher.Hash("p"), hasher.Hash("s"), true);
        db.Users.Add(a1);
        db.Users.Add(a2);
        var handler = new DeactivateUserCommandHandler(db);

        var result = await handler.Handle(new DeactivateUserCommand(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(a1.IsActive);
    }
}

public class ReactivateUserCommandHandlerTests
{
    [Fact]
    public async Task Reactivation_RestoresSignInCapability()
    {
        var db = new FakeApplicationDbContext();
        var hasher = new FakePasswordHasher();
        var currentUser = new FakeCurrentUserService { IsAuthenticated = false };
        var dateTime = new FakeDateTimeProvider();
        var user = User.Create(UserId.Create(1), "ahmed", hasher.Hash("pass123"), hasher.Hash("sec123"));
        user.Deactivate();
        db.Users.Add(user);

        var reactivateHandler = new ReactivateUserCommandHandler(db);
        var reactivateResult = await reactivateHandler.Handle(new ReactivateUserCommand(1), CancellationToken.None);
        Assert.True(reactivateResult.IsSuccess);
        Assert.True(user.IsActive);

        var signInHandler = new TopLab.Application.Features.UsersAndPermissions.Commands.SignIn.SignInCommandHandler(db, hasher, currentUser, dateTime);
        var signInResult = await signInHandler.Handle(new TopLab.Application.Features.UsersAndPermissions.Commands.SignIn.SignInCommand("ahmed", "pass123"), CancellationToken.None);
        Assert.True(signInResult.IsSuccess);
    }
}
