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

public sealed record UserDetailDto(
    int Id,
    string UserName,
    bool IsAbsolutePermission,
    bool IsActive,
    DateTime? LastLoginAtUtc,
    decimal DiscountLimitPercent,
    bool BlockPrintOnRemainingBalance,
    TimeOnly? WorkStartTime,
    TimeOnly? WorkEndTime,
    bool HasBreakPeriod,
    int? BreakDurationMinutes,
    IReadOnlyList<string> GrantedPermissionCodes);
