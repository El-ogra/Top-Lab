using System.Collections.Generic;
using TopLab.Application.Common.Interfaces;

namespace TopLab.Application.Tests.Common.Fakes;

/// <summary>
/// Deterministic fake of <see cref="ICurrentUserService"/> for Application-layer tests
/// (Test Strategy §5 / decision #2 — hand-rolled fakes, no mocking library).
/// </summary>
public sealed class FakeCurrentUserService : ICurrentUserService
{
    public bool IsAuthenticated { get; init; } = true;

    public int UserId { get; init; } = 1;

    public bool IsAbsolutePermission { get; init; }

    public HashSet<string> GrantedPermissions { get; init; } = new();

    public bool HasPermission(string code) => GrantedPermissions.Contains(code);
}
