using TopLab.Application.Common.Behaviors;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.UsersAndPermissions.Commands.SaveUserPermissions;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Users;

namespace TopLab.Application.Tests.Features.UsersAndPermissions;

public class SaveUserPermissionsCommandHandlerTests
{
    private static FakeApplicationDbContext SeedDb()
    {
        var db = new FakeApplicationDbContext();
        var codes = new[] { "ADD_EDIT_PATIENT", "DELETE_PATIENT", "PT_AUDIT_ACCESS", "DISCOUNT_LIMIT", "BLOCK_PRINT_ON_BALANCE" };
        for (int i = 0; i < codes.Length; i++)
        {
            db.Permissions.Add(Permission.Create(PermissionId.Create(i + 1), codes[i], "desc"));
        }
        return db;
    }

    [Fact]
    public async Task GrantSetReplacement_IsAtomic_OneSaveChanges()
    {
        var db = SeedDb();
        var handler = new SaveUserPermissionsCommandHandler(db, new FakeCurrentUserService { IsAbsolutePermission = true });
        var hasher = new FakePasswordHasher();
        var user = User.Create(UserId.Create(1), "ahmed", hasher.Hash("p"), hasher.Hash("s"));
        user.GrantPermission(PermissionId.Create(1));
        db.Users.Add(user);
        db.UserPermissionGrants.Add(new UserPermissionGrant(UserId.Create(1), PermissionId.Create(1)));

        var cmd = new SaveUserPermissionsCommand(1, new[] { "ADD_EDIT_PATIENT", "DELETE_PATIENT" });
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, db.SaveChangesCallCount);
        var remaining = db.UserPermissionGrants.Where(g => g.UserId.Value == 1).ToList();
        Assert.Equal(2, remaining.Count);
        Assert.Contains(remaining, g => g.PermissionId.Value == 1);
        Assert.Contains(remaining, g => g.PermissionId.Value == 2);
        Assert.Equal(2, user.PermissionGrants.Count);
    }

    [Fact]
    public async Task ValidationFailureMidSet_PersistsNothing()
    {
        var db = SeedDb();
        var handler = new SaveUserPermissionsCommandHandler(db, new FakeCurrentUserService { IsAbsolutePermission = true });
        var hasher = new FakePasswordHasher();
        var user = User.Create(UserId.Create(1), "ahmed", hasher.Hash("p"), hasher.Hash("s"));
        user.GrantPermission(PermissionId.Create(1));
        db.Users.Add(user);
        db.UserPermissionGrants.Add(new UserPermissionGrant(UserId.Create(1), PermissionId.Create(1)));

        var cmd = new SaveUserPermissionsCommand(1, new[] { "ADD_EDIT_PATIENT", "UNKNOWN_CODE" });
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(0, db.SaveChangesCallCount);
        // grant set unchanged
        Assert.Single(user.PermissionGrants);
    }

    [Fact]
    public async Task AfterRevoking_AuthorizationFails()
    {
        var db = SeedDb();
        var hasher = new FakePasswordHasher();
        var user = User.Create(UserId.Create(1), "limited", hasher.Hash("p"), hasher.Hash("s"), false);
        user.GrantPermission(PermissionId.Create(1)); // ADD_EDIT_PATIENT
        db.Users.Add(user);
        db.UserPermissionGrants.Add(new UserPermissionGrant(UserId.Create(1), PermissionId.Create(1)));

        // Same fake is used for the handler, the AuthorizationBehavior, and the revoke admin.
        var limitedUser = new FakeCurrentUserService
        {
            UserId = 1,
            IsAuthenticated = true,
            IsAbsolutePermission = false
        };
        limitedUser.GrantedPermissions.Add("ADD_EDIT_PATIENT");

        var behavior = new AuthorizationBehavior<TestAuthorizedRequest, Result>(limitedUser);
        var allowed = await behavior.Handle(
            new TestAuthorizedRequest("ADD_EDIT_PATIENT"),
            _ => Task.FromResult(Result.Success()),
            CancellationToken.None);
        Assert.True(allowed.IsSuccess);

        // Absolute admin revokes everything for user 1 via the real handler.
        var admin = new FakeCurrentUserService
        {
            UserId = 99,
            IsAuthenticated = true,
            IsAbsolutePermission = true
        };
        var handler = new SaveUserPermissionsCommandHandler(db, admin);
        var revokeResult = await handler.Handle(
            new SaveUserPermissionsCommand(1, Array.Empty<string>()),
            CancellationToken.None);
        Assert.True(revokeResult.IsSuccess);

        // Round-trip the same limitedUser fake from the domain grants (no self-clearing HashSet).
        limitedUser.GrantedPermissions.Clear();
        foreach (var grant in user.PermissionGrants)
        {
            var perm = db.Permissions.FirstOrDefault(p => p.Id == grant.PermissionId);
            if (perm is not null)
            {
                limitedUser.GrantedPermissions.Add(perm.Code);
            }
        }
        Assert.Empty(limitedUser.GrantedPermissions);

        var denied = await behavior.Handle(
            new TestAuthorizedRequest("ADD_EDIT_PATIENT"),
            _ => Task.FromResult(Result.Success()),
            CancellationToken.None);
        Assert.False(denied.IsSuccess);
        Assert.Equal(ErrorType.Forbidden, denied.Error!.Type);
    }

    private sealed record TestAuthorizedRequest(string RequiredPermissionCode)
        : MediatR.IRequest<Result>, TopLab.Application.Common.Authorization.IAuthorizedRequest;

    [Fact]
    public async Task SavingAuditAccess_SucceedsAtDataLayer()
    {
        var db = SeedDb();
        var handler = new SaveUserPermissionsCommandHandler(db, new FakeCurrentUserService { IsAbsolutePermission = true });
        var hasher = new FakePasswordHasher();
        var user = User.Create(UserId.Create(1), "ahmed", hasher.Hash("p"), hasher.Hash("s"));
        db.Users.Add(user);

        var cmd = new SaveUserPermissionsCommand(1, new[] { "PT_AUDIT_ACCESS" });
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(db.UserPermissionGrants);
    }
}
