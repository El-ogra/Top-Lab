using TopLab.Application.Common.Interfaces;
using TopLab.Domain.Common;

namespace TopLab.Infrastructure.Tests.Common.Fakes;

/// <summary>
/// Test entity deriving from <see cref="AuditableEntity{TId}"/> with an
/// <c>int</c> identifier — the shape used across most Top-Lab entities.
/// </summary>
public sealed class AuditableTestEntity : AuditableEntity<int>
{
    public string Name { get; set; } = string.Empty;

    public AuditableTestEntity()
    {
    }

    public AuditableTestEntity(int id)
        : base(id)
    {
    }
}

public sealed class NonAuditableTestEntity : Entity<int>
{
    public string Name { get; set; } = string.Empty;

    public NonAuditableTestEntity()
    {
    }

    public NonAuditableTestEntity(int id)
        : base(id)
    {
    }
}

/// <summary>
/// Deterministic fake <see cref="ICurrentUserService"/> returning a fixed
/// authenticated user; the interceptor reads user/clock values from it.
/// </summary>
public sealed class FakeCurrentUserService : ICurrentUserService
{
    public bool IsAuthenticated { get; set; } = true;

    public int UserId { get; set; } = 42;

    public string UserName { get; set; } = "fakeuser";

    public bool IsAbsolutePermission { get; set; }

    public HashSet<string> GrantedPermissions { get; set; } = new();

    public bool HasPermission(string code) => GrantedPermissions.Contains(code);

    public void SetSession(int userId, string userName, bool isAbsolutePermission, IEnumerable<string> grantedPermissions)
    {
        UserId = userId;
        UserName = userName;
        IsAbsolutePermission = isAbsolutePermission;
        GrantedPermissions = new HashSet<string>(grantedPermissions, StringComparer.Ordinal);
        IsAuthenticated = true;
    }

    public void ClearSession()
    {
        UserId = 0;
        UserName = string.Empty;
        IsAbsolutePermission = false;
        GrantedPermissions = new HashSet<string>(StringComparer.Ordinal);
        IsAuthenticated = false;
    }
}

/// <summary>
/// Deterministic fake <see cref="IDateTimeProvider"/> returning a fixed UTC value.
/// </summary>
public sealed class FakeDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow { get; set; } = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
}
