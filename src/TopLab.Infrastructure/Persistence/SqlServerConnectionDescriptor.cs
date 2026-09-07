using Microsoft.Extensions.Configuration;
using TopLab.Application.Features.AccessAndNavigation.Common.Interfaces;

namespace TopLab.Infrastructure.Persistence;

/// <summary>
/// Infrastructure implementation of <see cref="IDbConnectionDescriptor"/>.
/// Parses the workstation-local connection string
/// (<c>ConnectionStrings:TopLab</c>) at construction time and exposes only
/// the <c>Server=</c> and <c>Database=</c> segments. The password segment
/// is intentionally never surfaced: this descriptor is consumed by code that
/// displays connection info to the user (the connectivity DTO rendered in
/// a future login/status-bar UI), so any <c>Password=...</c> substring in
/// the raw connection string must be redacted to <c>Password=***</c> before
/// it can leak. The descriptor's <see cref="ServerName"/> and
/// <see cref="DatabaseName"/> properties never carry credentials, so the
/// DTO returned to the caller is credential-free.
/// </summary>
public sealed class SqlServerConnectionDescriptor : IDbConnectionDescriptor
{
    private readonly string? _serverName;
    private readonly string? _databaseName;

    public SqlServerConnectionDescriptor(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("TopLab");

        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            _serverName = ExtractValue(connectionString, "Server");
            _databaseName = ExtractValue(connectionString, "Database");
        }
    }

    public string? ServerName => _serverName;

    public string? DatabaseName => _databaseName;

    private static string? ExtractValue(string connectionString, string key)
    {
        // SQL Server connection strings are semicolon-separated key=value
        // pairs. The parser is intentionally tiny because the descriptor
        // only needs to read two well-known keys; if a real SQL Server
        // builder is later desired, swap this for SqlConnectionStringBuilder
        // behind the same surface.
        foreach (var segment in connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            var equalsIndex = segment.IndexOf('=');
            if (equalsIndex <= 0)
            {
                continue;
            }

            var segmentKey = segment[..equalsIndex].Trim();
            if (!string.Equals(segmentKey, key, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var value = segment[(equalsIndex + 1)..].Trim();
            return value.Length == 0 ? null : value;
        }

        return null;
    }
}