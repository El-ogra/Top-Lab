namespace TopLab.Application.Common.Interfaces;

/// <summary>
/// Port for the workstation-local SQL Server connection settings (stored only in
/// the ProgramData appsettings.json, never a database table). Keeps the Application
/// layer free of file-system concerns (ADR-0021/0025 precedent).
/// </summary>
public interface IWorkstationConnectionSettingsProvider
{
    string? GetEffectiveConnectionString();

    Task<bool> TestConnectionStringAsync(string candidateConnectionString, CancellationToken cancellationToken = default);

    Task SaveConnectionStringAsync(
        string server,
        string database,
        bool integratedSecurity,
        string login,
        string password,
        CancellationToken cancellationToken = default);
}