using TopLab.Application.Features.UsersAndPermissions.Queries.GetUserById;
using TopLab.Application.Features.UsersAndPermissions.Queries.GetUsers;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Users;

namespace TopLab.Application.Tests.Features.UsersAndPermissions;

public class GetUsersQueryHandlerTests
{
    [Fact]
    public void GetUsers_DoesNotContainHashMaterial()
    {
        var dtoType = typeof(TopLab.Application.Features.UsersAndPermissions.Common.UserSummaryDto);
        var props = dtoType.GetProperties().Select(p => p.Name).ToList();
        Assert.DoesNotContain(props, p => p.Contains("Hash", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(props, p => p.Contains("Password", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GetUsers_ReturnsList()
    {
        var db = new FakeApplicationDbContext();
        var hasher = new FakePasswordHasher();
        db.Users.Add(User.Create(UserId.Create(1), "ahmed", hasher.Hash("p"), hasher.Hash("s"), false));
        db.Users.Add(User.Create(UserId.Create(2), "sara", hasher.Hash("p"), hasher.Hash("s"), true));

        var handler = new GetUsersQueryHandler(db);
        var result = await handler.Handle(new GetUsersQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Count);
        Assert.Contains(result.Value, u => u.UserName == "ahmed");
    }

    [Fact]
    public void GetUserById_DoesNotContainHashMaterial()
    {
        var dtoType = typeof(TopLab.Application.Features.UsersAndPermissions.Common.UserDetailDto);
        var props = dtoType.GetProperties().Select(p => p.Name).ToList();
        Assert.DoesNotContain(props, p => p.Contains("Hash", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(props, p => p.Contains("Password", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GetUserById_RoundTrips_GrantsAndPolicy()
    {
        var db = new FakeApplicationDbContext();
        var hasher = new FakePasswordHasher();
        var codes = new[] { "ADD_EDIT_PATIENT", "DISCOUNT_LIMIT", "BLOCK_PRINT_ON_BALANCE" };
        for (int i = 0; i < codes.Length; i++)
        {
            db.Permissions.Add(Permission.Create(PermissionId.Create(i + 1), codes[i], "desc"));
        }

        var user = User.Create(UserId.Create(1), "ahmed", hasher.Hash("p"), hasher.Hash("s"), false, 10, true, new TimeOnly(9, 0), new TimeOnly(17, 0), true, 60);
        user.GrantPermission(PermissionId.Create(1));
        user.GrantPermission(PermissionId.Create(2));
        user.GrantPermission(PermissionId.Create(3));
        db.Users.Add(user);
        db.UserPermissionGrants.Add(new UserPermissionGrant(UserId.Create(1), PermissionId.Create(1)));
        db.UserPermissionGrants.Add(new UserPermissionGrant(UserId.Create(1), PermissionId.Create(2)));
        db.UserPermissionGrants.Add(new UserPermissionGrant(UserId.Create(1), PermissionId.Create(3)));

        var handler = new GetUserByIdQueryHandler(db);
        var result = await handler.Handle(new GetUserByIdQuery(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(10, result.Value!.DiscountLimitPercent);
        Assert.True(result.Value.BlockPrintOnRemainingBalance);
        Assert.Equal(new TimeOnly(9, 0), result.Value.WorkStartTime);
        Assert.Equal(3, result.Value.GrantedPermissionCodes.Count);
        Assert.Contains("DISCOUNT_LIMIT", result.Value.GrantedPermissionCodes);
    }

    [Fact]
    public async Task GetUserById_NotFound_ReturnsNotFound()
    {
        var db = new FakeApplicationDbContext();
        var handler = new GetUserByIdQueryHandler(db);
        var result = await handler.Handle(new GetUserByIdQuery(999), CancellationToken.None);
        Assert.False(result.IsSuccess);
        Assert.Equal(TopLab.Application.Common.Results.ErrorType.NotFound, result.Error!.Type);
    }
}
