using System.Collections.Generic;
using TopLab.Application.Common.Interfaces;

namespace TopLab.Application.Tests.Common.Fakes;

/// <summary>
/// Deterministic fake of <see cref="ICurrentUserService"/> for Application-layer tests
/// (Test Strategy §5 / decision #2 — hand-rolled fakes, no mocking library).
/// </summary>
public sealed class FakeCurrentUserService : ICurrentUserService
{
    public bool IsAuthenticated { get; set; } = true;

    public int UserId { get; set; } = 1;

    public string UserName { get; set; } = "testuser";

    public bool IsAbsolutePermission { get; set; }

    public HashSet<string> GrantedPermissions { get; init; } = new();

    public bool HasPermission(string code) => GrantedPermissions.Contains(code);

    public void SetSession(int userId, string userName, bool isAbsolutePermission, IEnumerable<string> grantedPermissions)
    {
        UserId = userId;
        UserName = userName;
        IsAbsolutePermission = isAbsolutePermission;
        GrantedPermissions.Clear();
        foreach (var code in grantedPermissions)
        {
            GrantedPermissions.Add(code);
        }

        IsAuthenticated = true;
    }

    public void ClearSession()
    {
        UserId = 0;
        UserName = string.Empty;
        IsAbsolutePermission = false;
        GrantedPermissions.Clear();
        IsAuthenticated = false;
    }
}
