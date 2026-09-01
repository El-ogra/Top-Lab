using TopLab.Application.Features.UsersAndPermissions.Queries.HasAnyAbsoluteUser;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Users;

namespace TopLab.Application.Tests.Features.UsersAndPermissions;

public class HasAnyAbsoluteUserQueryHandlerTests
{
    [Fact]
    public async Task EmptyStore_ReturnsFalse()
    {
        var db = new FakeApplicationDbContext();
        var handler = new HasAnyAbsoluteUserQueryHandler(db);
        var result = await handler.Handle(new HasAnyAbsoluteUserQuery(), CancellationToken.None);
        Assert.True(result.IsSuccess);
        Assert.False(result.Value);
    }

    [Fact]
    public async Task LimitedUsersOnly_ReturnsFalse()
    {
        var db = new FakeApplicationDbContext();
        var hasher = new FakePasswordHasher();
        db.Users.Add(User.Create(UserId.Create(1), "u1", hasher.Hash("p"), hasher.Hash("s"), false));
        db.Users.Add(User.Create(UserId.Create(2), "u2", hasher.Hash("p"), hasher.Hash("s"), false));
        var handler = new HasAnyAbsoluteUserQueryHandler(db);
        var result = await handler.Handle(new HasAnyAbsoluteUserQuery(), CancellationToken.None);
        Assert.False(result.Value);
    }

    [Fact]
    public async Task OneActiveAbsolute_ReturnsTrue()
    {
        var db = new FakeApplicationDbContext();
        var hasher = new FakePasswordHasher();
        db.Users.Add(User.Create(UserId.Create(1), "admin", hasher.Hash("p"), hasher.Hash("s"), true));
        var handler = new HasAnyAbsoluteUserQueryHandler(db);
        var result = await handler.Handle(new HasAnyAbsoluteUserQuery(), CancellationToken.None);
        Assert.True(result.Value);
    }

    [Fact]
    public async Task InactiveAbsoluteOnly_ReturnsFalse()
    {
        var db = new FakeApplicationDbContext();
        var hasher = new FakePasswordHasher();
        var user = User.Create(UserId.Create(1), "admin", hasher.Hash("p"), hasher.Hash("s"), true);
        user.Deactivate();
        db.Users.Add(user);
        var handler = new HasAnyAbsoluteUserQueryHandler(db);
        var result = await handler.Handle(new HasAnyAbsoluteUserQuery(), CancellationToken.None);
        Assert.False(result.Value);
    }
}
