using System.Reflection;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TopLab.Application;

namespace TopLab.Presentation;

/// <summary>
/// Composition root — Host builds Application + Infrastructure + Presentation.
/// Presentation has no compile-time reference to Infrastructure (Architecture §2.2,
/// Coding Standards §3.1). Infrastructure is loaded at startup via reflection so the
/// dependency direction (Infrastructure → Application, Presentation → Application)
/// is preserved while still composing all services in one Host.
/// </summary>
public partial class App : System.Windows.Application
{
    private IHost? _host;

    protected override void OnStartup(StartupEventArgs e)
    {
        var builder = Host.CreateApplicationBuilder(e.Args);

        builder.Services.AddApplication();
        AddInfrastructure(builder.Services, builder.Configuration);
        builder.Services.AddPresentation();

        _host = builder.Build();
        _host.Start();

        var main = _host.Services.GetRequiredService<MainWindow>();
        main.Show();

        base.OnStartup(e);
    }

    private static void AddInfrastructure(IServiceCollection services, Microsoft.Extensions.Configuration.IConfiguration configuration)
    {
        const string assemblyName = "TopLab.Infrastructure";
        const string typeName = "TopLab.Infrastructure.DependencyInjection";

        Assembly assembly;
        try
        {
            assembly = Assembly.Load(assemblyName);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to load Infrastructure assembly '{assemblyName}'. Ensure TopLab.Infrastructure.dll is present in the output directory.", ex);
        }

        var type = assembly.GetType(typeName)
            ?? throw new InvalidOperationException($"Type '{typeName}' not found in assembly '{assemblyName}'.");

        var method = type.GetMethod("AddInfrastructure", BindingFlags.Public | BindingFlags.Static)
            ?? throw new InvalidOperationException($"Method 'AddInfrastructure' not found on '{typeName}'.");

        try
        {
            method.Invoke(null, new object[] { services, configuration });
        }
        catch (TargetInvocationException tie) when (tie.InnerException is not null)
        {
            throw tie.InnerException;
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        if (_host is not null)
        {
            _host.Dispose();
        }

        base.OnExit(e);
    }
}
