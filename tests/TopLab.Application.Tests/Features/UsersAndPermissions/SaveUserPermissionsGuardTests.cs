using TopLab.Application.Features.UsersAndPermissions.Commands.SaveUserPermissions;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Users;

namespace TopLab.Application.Tests.Features.UsersAndPermissions;

public sealed class SaveUserPermissionsGuardTests
{
    private static FakeApplicationDbContext SeedDb()
    {
        var db = new FakeApplicationDbContext();
        var codes = new[] { "ADD_EDIT_PATIENT", "EDIT_SYSTEM_SETTINGS" };
        for (int i = 0; i < codes.Length; i++)
        {
            db.Permissions.Add(Permission.Create(PermissionId.Create(i + 1), codes[i], "desc"));
        }
        return db;
    }

    [Fact]
    public async Task AuthenticatedNonAbsolute_IsForbidden_PermissionsUnchanged()
    {
        var db = SeedDb();
        var hasher = new FakePasswordHasher();
        var user = User.Create(UserId.Create(1), "victim", hasher.Hash("p"), hasher.Hash("s"));
        db.Users.Add(user);
        var current = new FakeCurrentUserService { IsAuthenticated = true, IsAbsolutePermission = false };
        var handler = new SaveUserPermissionsCommandHandler(db, current);

        var result = await handler.Handle(new SaveUserPermissionsCommand(1, new[] { "EDIT_SYSTEM_SETTINGS" }), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(TopLab.Application.Common.Results.ErrorType.Forbidden, result.Error!.Type);
        Assert.Equal(0, db.SaveChangesCallCount);
        Assert.Empty(user.PermissionGrants);
    }

    [Fact]
    public async Task AuthenticatedAbsolute_Succeeds()
    {
        var db = SeedDb();
        var hasher = new FakePasswordHasher();
        var user = User.Create(UserId.Create(1), "victim", hasher.Hash("p"), hasher.Hash("s"));
        db.Users.Add(user);
        var current = new FakeCurrentUserService { IsAuthenticated = true, IsAbsolutePermission = true };
        var handler = new SaveUserPermissionsCommandHandler(db, current);

        var result = await handler.Handle(new SaveUserPermissionsCommand(1, new[] { "EDIT_SYSTEM_SETTINGS" }), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(user.PermissionGrants);
    }
}
