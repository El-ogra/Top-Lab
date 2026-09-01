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

    public void Reactivate()
    {
        IsActive = true;
    }

    public void RecordLogin(DateTime atUtc)
    {
        LastLoginAtUtc = atUtc;
    }

    public void ChangeUserName(string userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            throw new ArgumentException("UserName is required.", nameof(userName));
        }

        UserName = userName.Trim();
    }

    public void ChangePasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new ArgumentException("PasswordHash is required.", nameof(passwordHash));
        }

        PasswordHash = passwordHash;
    }

    public void ChangeInternalWindowsPasswordHash(string hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
        {
            throw new ArgumentException("InternalWindowsPasswordHash is required.", nameof(hash));
        }

        InternalWindowsPasswordHash = hash;
    }

    public void SetAbsolutePermission(bool isAbsolute)
    {
        IsAbsolutePermission = isAbsolute;
    }

    public void SetPolicy(decimal discountLimitPercent, bool blockPrintOnRemainingBalance)
    {
        if (discountLimitPercent < 0 || discountLimitPercent > 100)
        {
            throw new ArgumentException("DiscountLimitPercent must be between 0 and 100.", nameof(discountLimitPercent));
        }

        DiscountLimitPercent = discountLimitPercent;
        BlockPrintOnRemainingBalance = blockPrintOnRemainingBalance;
    }

    public void SetWorkingHours(TimeOnly? start, TimeOnly? end)
    {
        if (start is null && end is null)
        {
            WorkStartTime = null;
            WorkEndTime = null;
            return;
        }

        if (start is null || end is null)
        {
            throw new ArgumentException("Both WorkStartTime and WorkEndTime must be either null or set together.");
        }

        if (start.Value >= end.Value)
        {
            throw new ArgumentException("WorkStartTime must be earlier than WorkEndTime. Overnight ranges are not supported.");
        }

        WorkStartTime = start;
        WorkEndTime = end;
    }

    public void SetBreakPeriod(bool hasBreak, int? durationMinutes)
    {
        if (hasBreak)
        {
            if (durationMinutes is null || durationMinutes.Value <= 0)
            {
                throw new ArgumentException("BreakDurationMinutes must be positive when HasBreakPeriod is true.", nameof(durationMinutes));
            }

            HasBreakPeriod = true;
            BreakDurationMinutes = durationMinutes;
        }
        else
        {
            HasBreakPeriod = false;
            BreakDurationMinutes = null;
        }
    }

    public void GrantPermission(PermissionId permissionId)
    {
        if (permissionId is null)
        {
            throw new ArgumentNullException(nameof(permissionId));
        }

        if (_grants.Any(g => g.PermissionId.Equals(permissionId)))
        {
            return;
        }

        _grants.Add(new UserPermissionGrant(Id, permissionId));
    }

    public void RevokePermission(PermissionId permissionId)
    {
        if (permissionId is null)
        {
            throw new ArgumentNullException(nameof(permissionId));
        }

        var existing = _grants.FirstOrDefault(g => g.PermissionId.Equals(permissionId));
        if (existing is not null)
        {
            _grants.Remove(existing);
        }
    }

    public void ClearPermissions()
    {
        _grants.Clear();
    }
}
