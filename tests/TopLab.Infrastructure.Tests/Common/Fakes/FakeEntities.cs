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
    public bool IsAuthenticated { get; init; } = true;

    public int UserId { get; init; } = 42;

    public bool IsAbsolutePermission { get; init; }

    public HashSet<string> GrantedPermissions { get; init; } = new();

    public bool HasPermission(string code) => GrantedPermissions.Contains(code);
}

/// <summary>
/// Deterministic fake <see cref="IDateTimeProvider"/> returning a fixed UTC value.
/// </summary>
public sealed class FakeDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow { get; init; } = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
}
