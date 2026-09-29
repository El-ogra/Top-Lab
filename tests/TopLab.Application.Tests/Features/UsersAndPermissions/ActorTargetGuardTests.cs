using TopLab.Application.Features.UsersAndPermissions.Commands.DeactivateUser;
using TopLab.Application.Features.UsersAndPermissions.Commands.DeleteUser;
using TopLab.Application.Features.UsersAndPermissions.Commands.ReactivateUser;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Users;

namespace TopLab.Application.Tests.Features.UsersAndPermissions;

public sealed class ActorTargetGuardTests
{
    private static void AddAbsolute(FakeApplicationDbContext db, int id, string name, bool active = true)
    {
        var hasher = new FakePasswordHasher();
        var admin = User.Create(UserId.Create(id), name, hasher.Hash("p"), hasher.Hash("s"), isAbsolutePermission: true);
        if (!active)
        {
            admin.Deactivate();
        }

        db.Users.Add(admin);
    }

    private static void AddRegular(FakeApplicationDbContext db, int id, string name, bool active = true)
    {
        var hasher = new FakePasswordHasher();
        var user = User.Create(UserId.Create(id), name, hasher.Hash("p"), hasher.Hash("s"), isAbsolutePermission: false);
        if (!active)
        {
            user.Deactivate();
        }

        db.Users.Add(user);
    }

    [Fact]
    public async Task Delete_NonAbsoluteCaller_AbsoluteTarget_Forbidden_EvenWithAnotherAdmin()
    {
        var db = new FakeApplicationDbContext();
        AddAbsolute(db, 1, "admin1");
        AddAbsolute(db, 2, "admin2");
        var current = new FakeCurrentUserService { IsAuthenticated = true, IsAbsolutePermission = false };
        var result = await new DeleteUserCommandHandler(db, current).Handle(new DeleteUserCommand(1), CancellationToken.None);
        Assert.False(result.IsSuccess);
        Assert.Equal(TopLab.Application.Common.Results.ErrorType.Forbidden, result.Error!.Type);
    }

    [Fact]
    public async Task Delete_NonAbsoluteCaller_NonAbsoluteTarget_Succeeds()
    {
        var db = new FakeApplicationDbContext();
        AddRegular(db, 10, "temp");
        var current = new FakeCurrentUserService { IsAuthenticated = true, IsAbsolutePermission = false };
        var result = await new DeleteUserCommandHandler(db, current).Handle(new DeleteUserCommand(10), CancellationToken.None);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Delete_AbsoluteCaller_AbsoluteTarget_Succeeds_WhenAnotherExists()
    {
        var db = new FakeApplicationDbContext();
        AddAbsolute(db, 1, "admin1");
        AddAbsolute(db, 2, "admin2");
        var current = new FakeCurrentUserService { IsAuthenticated = true, IsAbsolutePermission = true };
        var result = await new DeleteUserCommandHandler(db, current).Handle(new DeleteUserCommand(1), CancellationToken.None);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Deactivate_NonAbsoluteCaller_AbsoluteTarget_Forbidden_EvenWithAnotherAdmin()
    {
        var db = new FakeApplicationDbContext();
        AddAbsolute(db, 1, "admin1");
        AddAbsolute(db, 2, "admin2");
        var current = new FakeCurrentUserService { IsAuthenticated = true, IsAbsolutePermission = false };
        var result = await new DeactivateUserCommandHandler(db, current).Handle(new DeactivateUserCommand(1), CancellationToken.None);
        Assert.False(result.IsSuccess);
        Assert.Equal(TopLab.Application.Common.Results.ErrorType.Forbidden, result.Error!.Type);
    }

    [Fact]
    public async Task Deactivate_NonAbsoluteCaller_NonAbsoluteTarget_Succeeds()
    {
        var db = new FakeApplicationDbContext();
        AddRegular(db, 10, "temp");
        var current = new FakeCurrentUserService { IsAuthenticated = true, IsAbsolutePermission = false };
        var result = await new DeactivateUserCommandHandler(db, current).Handle(new DeactivateUserCommand(10), CancellationToken.None);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Deactivate_AbsoluteCaller_AbsoluteTarget_Succeeds_WhenAnotherExists()
    {
        var db = new FakeApplicationDbContext();
        AddAbsolute(db, 1, "admin1");
        AddAbsolute(db, 2, "admin2");
        var current = new FakeCurrentUserService { IsAuthenticated = true, IsAbsolutePermission = true };
        var result = await new DeactivateUserCommandHandler(db, current).Handle(new DeactivateUserCommand(1), CancellationToken.None);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Reactivate_NonAbsoluteCaller_DisabledAbsoluteTarget_Forbidden()
    {
        var db = new FakeApplicationDbContext();
        AddAbsolute(db, 1, "admin1", active: false);
        AddAbsolute(db, 2, "admin2");
        var current = new FakeCurrentUserService { IsAuthenticated = true, IsAbsolutePermission = false };
        var result = await new ReactivateUserCommandHandler(db, current).Handle(new ReactivateUserCommand(1), CancellationToken.None);
        Assert.False(result.IsSuccess);
        Assert.Equal(TopLab.Application.Common.Results.ErrorType.Forbidden, result.Error!.Type);
    }

    [Fact]
    public async Task Reactivate_NonAbsoluteCaller_NonAbsoluteTarget_Succeeds()
    {
        var db = new FakeApplicationDbContext();
        AddRegular(db, 10, "temp", active: false);
        var current = new FakeCurrentUserService { IsAuthenticated = true, IsAbsolutePermission = false };
        var result = await new ReactivateUserCommandHandler(db, current).Handle(new ReactivateUserCommand(10), CancellationToken.None);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Reactivate_AbsoluteCaller_AbsoluteTarget_Succeeds()
    {
        var db = new FakeApplicationDbContext();
        AddAbsolute(db, 1, "admin1", active: false);
        var current = new FakeCurrentUserService { IsAuthenticated = true, IsAbsolutePermission = true };
        var result = await new ReactivateUserCommandHandler(db, current).Handle(new ReactivateUserCommand(1), CancellationToken.None);
        Assert.True(result.IsSuccess);
    }
}
