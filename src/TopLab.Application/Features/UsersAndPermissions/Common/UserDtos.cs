namespace TopLab.Application.Features.UsersAndPermissions.Common;

public sealed record UserSummaryDto(
    int Id,
    string UserName,
    bool IsAbsolutePermission,
    bool IsActive,
    DateTime? LastLoginAtUtc);

public sealed record CurrentUserSessionDto(
    int Id,
    string UserName,
    bool IsAbsolutePermission,
    IReadOnlyList<string> GrantedPermissionCodes,
    DateTime? LastLoginAtUtc);

public sealed record PermissionDto(
    int Id,
    string Code,
    string Description);
