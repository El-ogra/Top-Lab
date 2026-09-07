namespace TopLab.Application.Features.AccessAndNavigation.Common;

/// <summary>
/// Result of <see cref="TopLab.Application.Features.AccessAndNavigation.Queries.CheckDatabaseConnectivity.CheckDatabaseConnectivityQuery"/>.
/// Captures whether the database is reachable together with the server and
/// database name parsed from the workstation-local connection string. The
/// descriptor never surfaces the password (redacted to <c>Password=***</c>
/// in the Infrastructure implementation), so this DTO is safe to render
/// in a future login/status-bar UI.
/// </summary>
public sealed record CheckDatabaseConnectivityDto(
    bool IsConnected,
    string? ServerName,
    string? DatabaseName,
    DateTime CheckedAtUtc);