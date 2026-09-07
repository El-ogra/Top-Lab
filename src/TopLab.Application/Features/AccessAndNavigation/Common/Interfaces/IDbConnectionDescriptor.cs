namespace TopLab.Application.Features.AccessAndNavigation.Common.Interfaces;

/// <summary>
/// Port that exposes the workstation-local SQL Server connection's server
/// and database name without leaking credentials. Smaller and redacted
/// compared with <see cref="TopLab.Application.Common.Interfaces.IWorkstationConnectionSettingsProvider"/>,
/// which returns the full connection string. Implemented in Infrastructure.
/// </summary>
public interface IDbConnectionDescriptor
{
    string? ServerName { get; }

    string? DatabaseName { get; }
}