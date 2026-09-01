using TopLab.Application.Features.UsersAndPermissions.Queries.GetCurrentSession;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Users;

namespace TopLab.Application.Tests.Features.UsersAndPermissions;

public class GetCurrentSessionQueryHandlerTests
{
    [Fact]
    public async Task SessionFields_MappedOneToOne()
    {
        var db = new FakeApplicationDbContext();
        var hasher = new FakePasswordHasher();
        var perm = Permission.Create(PermissionId.Create(10), "ADD_EDIT_PATIENT", "desc");
        db.Permissions.Add(perm);

        var user = User.Create(UserId.Create(5), "ahmed", hasher.Hash("main"), hasher.Hash("sec"), isAbsolutePermission: true);
        user.GrantPermission(PermissionId.Create(10));
        user.RecordLogin(new DateTime(2026, 5, 1, 10, 0, 0, DateTimeKind.Utc));
        db.Users.Add(user);
        db.UserPermissionGrants.Add(new UserPermissionGrant(UserId.Create(5), PermissionId.Create(10)));

        var currentUser = new FakeCurrentUserService { IsAuthenticated = true, UserId = 5, UserName = "ahmed", IsAbsolutePermission = true };
        currentUser.GrantedPermissions.Add("ADD_EDIT_PATIENT");

        var handler = new GetCurrentSessionQueryHandler(db, currentUser);
        var result = await handler.Handle(new GetCurrentSessionQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.Value!.Id);
        Assert.Equal("ahmed", result.Value.UserName);
        Assert.True(result.Value.IsAbsolutePermission);
        Assert.Contains("ADD_EDIT_PATIENT", result.Value.GrantedPermissionCodes);
        Assert.Equal(user.LastLoginAtUtc, result.Value.LastLoginAtUtc);
    }

    [Fact]
    public async Task Unauthenticated_ReturnsFailure()
    {
        var db = new FakeApplicationDbContext();
        var currentUser = new FakeCurrentUserService { IsAuthenticated = false };
        var handler = new GetCurrentSessionQueryHandler(db, currentUser);

        var result = await handler.Handle(new GetCurrentSessionQuery(), CancellationToken.None);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void NoHashMaterial_PresentOnDtoType()
    {
        var dtoType = typeof(TopLab.Application.Features.UsersAndPermissions.Common.CurrentUserSessionDto);
        var props = dtoType.GetProperties().Select(p => p.Name).ToList();
        Assert.DoesNotContain(props, p => p.Contains("Hash", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(props, p => p.Contains("Password", StringComparison.OrdinalIgnoreCase));
    }
}
