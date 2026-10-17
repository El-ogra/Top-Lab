using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TopLab.Application;
using TopLab.Infrastructure;
using TopLab.Presentation;
using Xunit;

namespace TopLab.Presentation.Tests.DependencyInjection;

public class DiScopeValidationTests
{
    [Fact]
    public void FullComposition_BuildsServiceProvider_WithScopeAndBuildValidation()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:TopLab"] = "Server=(local);Database=TopLab;Trusted_Connection=True;"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddApplication();
        services.AddInfrastructure(configuration);
        services.AddPresentation();

        using var provider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateScopes = true,
            ValidateOnBuild = true
        });

        Assert.NotNull(provider);
    }
}
