using TopLab.Application.Features.UsersAndPermissions.Queries.GetUserById;
using TopLab.Application.Features.UsersAndPermissions.Queries.GetUsers;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Users;

namespace TopLab.Application.Tests.Features.UsersAndPermissions;

public sealed class GetUsersGuardTests
{
    [Fact]
    public async Task GetUsers_NonAbsoluteCaller_Forbidden()
    {
        var db = new FakeApplicationDbContext();
        var hasher = new FakePasswordHasher();
        db.Users.Add(User.Create(UserId.Create(1), "ahmed", hasher.Hash("p"), hasher.Hash("s"), false));
        var current = new FakeCurrentUserService { IsAuthenticated = true, IsAbsolutePermission = false };
        var result = await new GetUsersQueryHandler(db, current).Handle(new GetUsersQuery(), CancellationToken.None);
        Assert.False(result.IsSuccess);
        Assert.Equal(TopLab.Application.Common.Results.ErrorType.Forbidden, result.Error!.Type);
    }

    [Fact]
    public async Task GetUsers_AbsoluteCaller_Succeeds_SameData()
    {
        var db = new FakeApplicationDbContext();
        var hasher = new FakePasswordHasher();
        db.Users.Add(User.Create(UserId.Create(1), "ahmed", hasher.Hash("p"), hasher.Hash("s"), false));
        var current = new FakeCurrentUserService { IsAuthenticated = true, IsAbsolutePermission = true };
        var result = await new GetUsersQueryHandler(db, current).Handle(new GetUsersQuery(), CancellationToken.None);
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!);
    }

    [Fact]
    public async Task GetUserById_NonAbsoluteCaller_Forbidden()
    {
        var db = new FakeApplicationDbContext();
        var hasher = new FakePasswordHasher();
        db.Users.Add(User.Create(UserId.Create(1), "ahmed", hasher.Hash("p"), hasher.Hash("s"), false));
        var current = new FakeCurrentUserService { IsAuthenticated = true, IsAbsolutePermission = false };
        var result = await new GetUserByIdQueryHandler(db, current).Handle(new GetUserByIdQuery(1), CancellationToken.None);
        Assert.False(result.IsSuccess);
        Assert.Equal(TopLab.Application.Common.Results.ErrorType.Forbidden, result.Error!.Type);
    }

    [Fact]
    public async Task GetUserById_AbsoluteCaller_Succeeds_SameData()
    {
        var db = new FakeApplicationDbContext();
        var hasher = new FakePasswordHasher();
        db.Users.Add(User.Create(UserId.Create(1), "ahmed", hasher.Hash("p"), hasher.Hash("s"), false));
        var current = new FakeCurrentUserService { IsAuthenticated = true, IsAbsolutePermission = true };
        var result = await new GetUserByIdQueryHandler(db, current).Handle(new GetUserByIdQuery(1), CancellationToken.None);
        Assert.True(result.IsSuccess);
        Assert.Equal("ahmed", result.Value!.UserName);
    }
}
