using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Features.ExternalEntities.Common.Interfaces;
using TopLab.Infrastructure.Printing;
using TopLab.Infrastructure.Services;
using Xunit;

namespace TopLab.Infrastructure.Tests.DependencyInjection;

public class InfrastructureRegistrationTests
{
    private static ServiceProvider BuildProvider()
    {
        return BuildServices().BuildServiceProvider();
    }

    private static ServiceCollection BuildServices()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:TopLab"] = "Server=(local);Database=TopLab;Trusted_Connection=True;"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddInfrastructure(configuration);
        return services;
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
    public void AddInfrastructure_RegistersReportPrintingPipeline()
    {
        using var provider = BuildProvider();

        var service = provider.GetService<IReportPrintingService>();
        var writer = provider.GetService<IReportPdfWriter>();
        var dispatcher = provider.GetService<IPdfPrinterDispatcher>();

        Assert.NotNull(service);
        Assert.NotNull(writer);
        Assert.NotNull(dispatcher);
    }

    [Fact]
    public void AddInfrastructure_RegistersEnvelopePrintingPipeline()
    {
        // Presence-only: the envelope service activates with ILabPrintTextStore,
        // which the Presentation composition root registers (workstation-local
        // JSON store), so full activation is covered by the service tests' fakes.
        var services = BuildServices();

        Assert.Contains(services, d => d.ServiceType == typeof(IEnvelopePrintingService));
        Assert.Contains(services, d => d.ServiceType == typeof(IEnvelopePdfWriter));
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
