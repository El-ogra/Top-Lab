using TopLab.Application.Common.Interfaces;

namespace TopLab.Presentation.Services.Configuration;

/// <summary>
/// Production implementation of <see cref="IWorkstationConnectionSettingsProvider"/>
/// backed by <see cref="ConfigurationFileService"/> (the ProgramData appsettings.json).
/// Keeps the Application layer free of file-system/first-run concerns.
/// </summary>
public sealed class WorkstationConnectionSettingsProvider : IWorkstationConnectionSettingsProvider
{
    private readonly ConfigurationFileService _fileService;

    public WorkstationConnectionSettingsProvider(ConfigurationFileService fileService)
    {
        _fileService = fileService;
    }

    public string? GetEffectiveConnectionString()
    {
        return _fileService.TryLoadConnectionString(out var cs) ? cs : null;
    }

    public async Task<bool> TestConnectionStringAsync(string candidateConnectionString, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = new Microsoft.Data.SqlClient.SqlConnection(candidateConnectionString);
            await connection.OpenAsync(cancellationToken);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public Task SaveConnectionStringAsync(
        string server,
        string database,
        bool integratedSecurity,
        string login,
        string password,
        CancellationToken cancellationToken = default)
    {
        _fileService.SaveConnectionString(server, database, integratedSecurity, login, password);
        return Task.CompletedTask;
    }
}