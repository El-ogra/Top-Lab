using TopLab.Application.Features.SystemAndPrintSettings.Queries.GetDatabaseServerSettings;
using TopLab.Application.Tests.Common.Fakes;
using Xunit;

namespace TopLab.Application.Tests.Features.SystemAndPrintSettings;

public class GetDatabaseServerSettingsQueryHandlerTests
{
    [Fact]
    public async Task GetDatabaseServerSettings_ParsesSimplifiedSecurity()
    {
        var provider = new FakeWorkstationConnectionSettingsProvider
        {
            EffectiveConnectionString = "Server=mi-lap\\sqlexpress;Database=TopLab;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
        };
        var handler = new GetDatabaseServerSettingsQueryHandler(provider);
        var result = await handler.Handle(new GetDatabaseServerSettingsQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("mi-lap\\sqlexpress", result.Value!.ServerName);
        Assert.Equal("TopLab", result.Value.DatabaseName);
        Assert.True(result.Value.IntegratedSecurity);
        Assert.Equal(string.Empty, result.Value.Login);
    }

    [Fact]
    public async Task GetDatabaseServerSettings_NeverExposesPassword()
    {
        var provider = new FakeWorkstationConnectionSettingsProvider
        {
            EffectiveConnectionString = "Server=SRV;Database=TopLab;User Id=sa;Password=SuperSecret123;MultipleActiveResultSets=true;TrustServerCertificate=True"
        };
        var handler = new GetDatabaseServerSettingsQueryHandler(provider);
        var result = await handler.Handle(new GetDatabaseServerSettingsQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("SRV", result.Value!.ServerName);
        Assert.Equal("TopLab", result.Value.DatabaseName);
        Assert.False(result.Value.IntegratedSecurity);
        Assert.Equal("sa", result.Value.Login);

        var props = typeof(TopLab.Application.Features.SystemAndPrintSettings.Common.DatabaseServerSettingsDto)
            .GetProperties()
            .Select(p => p.Name)
            .ToList();
        Assert.DoesNotContain(props, p => p.Contains("Password", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(props, p => p.Contains("Pass", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GetDatabaseServerSettings_NoConnectionString_ReturnsUnexpected()
    {
        var provider = new FakeWorkstationConnectionSettingsProvider { EffectiveConnectionString = null };
        var handler = new GetDatabaseServerSettingsQueryHandler(provider);
        var result = await handler.Handle(new GetDatabaseServerSettingsQuery(), CancellationToken.None);
        Assert.False(result.IsSuccess);
        Assert.Equal(TopLab.Application.Common.Results.ErrorType.Unexpected, result.Error!.Type);
    }
}