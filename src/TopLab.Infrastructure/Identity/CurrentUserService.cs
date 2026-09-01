using Microsoft.Extensions.DependencyInjection;
using TopLab.Application.Common.Interfaces;

namespace TopLab.Infrastructure.Identity;

/// <summary>
/// Default Infrastructure implementation of <see cref="ICurrentUserService"/>.
/// Holds the current user's identifier, absolute-permission flag, and granted
/// permission codes in memory. The composition root (App.xaml.cs) replaces the
/// active session at login time (M01) and on every permission change.
/// </summary>
/// <remarks>
/// The service intentionally does not read from the database on every access;
/// the caller (the WPF shell after authentication) refreshes the snapshot when
/// the user logs in or when permissions change. A persistent lookup belongs in
/// a dedicated query, not on this hot path.
/// </remarks>
public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IServiceProvider _services;

    public CurrentUserService(IServiceProvider services)
    {
        _services = services;
    }

    public bool IsAuthenticated { get; private set; }

    public int UserId { get; private set; }

    public string UserName { get; private set; } = string.Empty;

    public bool IsAbsolutePermission { get; private set; }

    private ISet<string> _grantedPermissions = new HashSet<string>(StringComparer.Ordinal);

    public bool HasPermission(string code) => _grantedPermissions.Contains(code);

    /// <summary>
    /// Replaces the active session with the supplied values. Called by the
    /// composition root at sign-in, sign-out, and after permission changes.
    /// </summary>
    public void SetSession(int userId, string userName, bool isAbsolutePermission, IEnumerable<string> grantedPermissions)
    {
        UserId = userId;
        UserName = userName;
        IsAbsolutePermission = isAbsolutePermission;
        _grantedPermissions = new HashSet<string>(grantedPermissions, StringComparer.Ordinal);
        IsAuthenticated = true;
    }

    /// <summary>
    /// Clears the active session. Called at sign-out.
    /// </summary>
    public void ClearSession()
    {
        UserId = 0;
        UserName = string.Empty;
        IsAbsolutePermission = false;
        _grantedPermissions = new HashSet<string>(StringComparer.Ordinal);
        IsAuthenticated = false;
    }
}
