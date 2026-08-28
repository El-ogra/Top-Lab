using TopLab.Domain.Common.Ids;

namespace TopLab.Domain.Users;

/// <summary>Composite PK: UserId + PermissionId.</summary>
public sealed class UserPermissionGrant
{
    public UserId UserId { get; private set; } = default!;

    public PermissionId PermissionId { get; private set; } = default!;

    private UserPermissionGrant()
    {
    }

    public UserPermissionGrant(UserId userId, PermissionId permissionId)
    {
        UserId = userId;
        PermissionId = permissionId;
    }
}
