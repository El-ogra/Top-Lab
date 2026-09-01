using TopLab.Application.Common.Interfaces;
using TopLab.Application.Features.SystemAndPrintSettings.Common;

namespace TopLab.Application.Tests.Common.Fakes;

public sealed class FakeWorkstationConnectionSettingsProvider : IWorkstationConnectionSettingsProvider
{
    public string? EffectiveConnectionString { get; set; }

    public string? SavedServer { get; private set; }
    public string? SavedDatabase { get; private set; }
    public bool SavedIntegratedSecurity { get; private set; }
    public string? SavedLogin { get; private set; }
    public string? SavedPassword { get; private set; }
    public bool WasSaved { get; private set; }
    public bool TestResult { get; set; } = true;

    public string? GetEffectiveConnectionString() => EffectiveConnectionString;

    public Task<bool> TestConnectionStringAsync(string candidateConnectionString, CancellationToken cancellationToken = default)
        => Task.FromResult(TestResult);

    public Task SaveConnectionStringAsync(
        string server,
        string database,
        bool integratedSecurity,
        string login,
        string password,
        CancellationToken cancellationToken = default)
    {
        SavedServer = server;
        SavedDatabase = database;
        SavedIntegratedSecurity = integratedSecurity;
        SavedLogin = login;
        SavedPassword = password;
        WasSaved = true;
        return Task.CompletedTask;
    }
}