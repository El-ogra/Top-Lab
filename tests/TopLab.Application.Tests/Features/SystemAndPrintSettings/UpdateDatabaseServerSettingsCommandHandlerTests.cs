using TopLab.Application.Common.Results;
using TopLab.Application.Features.SystemAndPrintSettings.Commands.UpdateDatabaseServerSettings;
using TopLab.Application.Tests.Common.Fakes;

namespace TopLab.Application.Tests.Features.SystemAndPrintSettings;

public sealed class UpdateDatabaseServerSettingsCommandHandlerTests
{
    [Fact]
    public async Task Handle_UnreachableServer_ReturnsFailureAndLeavesStoredConnectionUnchanged()
    {
        var provider = new FakeWorkstationConnectionSettingsProvider { TestResult = false, EffectiveConnectionString = "old" };
        var handler = new UpdateDatabaseServerSettingsCommandHandler(provider);

        var result = await handler.Handle(
            new UpdateDatabaseServerSettingsCommand("bad-server", "TopLab", false, "sa", "secret"),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Unexpected, result.Error?.Type);
        Assert.False(provider.WasSaved);
        Assert.Equal("old", provider.EffectiveConnectionString);
    }

    [Fact]
    public async Task Handle_ReachableServer_SavesNewSettings()
    {
        var provider = new FakeWorkstationConnectionSettingsProvider { TestResult = true };
        var handler = new UpdateDatabaseServerSettingsCommandHandler(provider);

        var result = await handler.Handle(
            new UpdateDatabaseServerSettingsCommand("good-server", "TopLab", true, "", ""),
            CancellationToken.None);

        Assert.True(result.IsSuccess, result.Error?.Message);
        Assert.True(provider.WasSaved);
        Assert.Equal("good-server", provider.SavedServer);
        Assert.Equal("TopLab", provider.SavedDatabase);
        Assert.True(provider.SavedIntegratedSecurity);
    }
}