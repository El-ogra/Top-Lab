using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TopLab.Application.Features.ExternalEntities.Common.Interfaces;
using TopLab.Infrastructure.Services;
using Xunit;

namespace TopLab.Infrastructure.Tests.DependencyInjection;

public class InfrastructureRegistrationTests
{
    private static ServiceProvider BuildProvider()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:TopLab"] = "Server=(local);Database=TopLab;Trusted_Connection=True;"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddInfrastructure(configuration);
        return services.BuildServiceProvider();
    }

    [Fact]
    public void AddInfrastructure_RegistersEntityIdCodeGeneratorAsSingleton()
    {
        using var provider = BuildProvider();

        var first = provider.GetService<IEntityIdCodeGenerator>();
        var second = provider.GetService<IEntityIdCodeGenerator>();

        Assert.NotNull(first);
        Assert.IsType<SecureEntityIdCodeGenerator>(first);
        Assert.Same(first, second);
    }

    [Fact]
    public void SecureEntityIdCodeGenerator_ProducesDistinctWellFormedCodes()
    {
        var generator = new SecureEntityIdCodeGenerator();

        var first = generator.Generate();
        var second = generator.Generate();

        Assert.Equal(SecureEntityIdCodeGenerator.CodeLength, first.Length);
        Assert.Matches("^[A-HJ-NP-Z2-9]+$", first);
        Assert.DoesNotContain('0', first);
        Assert.DoesNotContain('O', first);
        Assert.DoesNotContain('1', first);
        Assert.DoesNotContain('I', first);
        Assert.NotEqual(first, second);
    }
}
