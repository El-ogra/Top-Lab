using TopLab.Domain.Common;
using TopLab.Domain.Common.Ids;

namespace TopLab.Domain.Users;

public sealed class User : AuditableEntity<UserId>
{
    public string UserName { get; private set; } = default!;

    public string PasswordHash { get; private set; } = default!;

    public string InternalWindowsPasswordHash { get; private set; } = default!;

    public bool IsAbsolutePermission { get; private set; }

    public decimal DiscountLimitPercent { get; private set; }

    public bool BlockPrintOnRemainingBalance { get; private set; }

    public TimeOnly? WorkStartTime { get; private set; }

    public TimeOnly? WorkEndTime { get; private set; }

    public bool HasBreakPeriod { get; private set; }

    public int? BreakDurationMinutes { get; private set; }

    public DateTime? LastLoginAtUtc { get; private set; }

    public bool IsActive { get; private set; } = true;

    private readonly List<UserPermissionGrant> _grants = [];
    public IReadOnlyCollection<UserPermissionGrant> PermissionGrants => _grants.AsReadOnly();

    private User()
    {
    }

    private User(
        UserId id,
        string userName,
        string passwordHash,
        string internalWindowsPasswordHash,
        bool isAbsolutePermission,
        decimal discountLimitPercent,
        bool blockPrintOnRemainingBalance,
        TimeOnly? workStartTime,
        TimeOnly? workEndTime,
        bool hasBreakPeriod,
        int? breakDurationMinutes,
        DateTime? lastLoginAtUtc,
        bool isActive)
        : base(id)
    {
        UserName = userName;
        PasswordHash = passwordHash;
        InternalWindowsPasswordHash = internalWindowsPasswordHash;
        IsAbsolutePermission = isAbsolutePermission;
        DiscountLimitPercent = discountLimitPercent;
        BlockPrintOnRemainingBalance = blockPrintOnRemainingBalance;
        WorkStartTime = workStartTime;
        WorkEndTime = workEndTime;
        HasBreakPeriod = hasBreakPeriod;
        BreakDurationMinutes = breakDurationMinutes;
        LastLoginAtUtc = lastLoginAtUtc;
        IsActive = isActive;
    }

    public static User Create(
        UserId id,
        string userName,
        string passwordHash,
        string internalWindowsPasswordHash,
        bool isAbsolutePermission = false,
        decimal discountLimitPercent = 0,
        bool blockPrintOnRemainingBalance = false,
        TimeOnly? workStartTime = null,
        TimeOnly? workEndTime = null,
        bool hasBreakPeriod = false,
        int? breakDurationMinutes = null)
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            throw new ArgumentException("UserName is required.", nameof(userName));
        }

        return new User(id, userName.Trim(), passwordHash, internalWindowsPasswordHash, isAbsolutePermission, discountLimitPercent, blockPrintOnRemainingBalance, workStartTime, workEndTime, hasBreakPeriod, breakDurationMinutes, null, true);
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void RecordLogin(DateTime atUtc)
    {
        LastLoginAtUtc = atUtc;
    }
}
