using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TopLab.Application;
using TopLab.Infrastructure;

namespace TopLab.Presentation;

/// <summary>
/// Composition root — Host builds Application + Infrastructure + Presentation.
/// The composition root (this file) is the one place permitted to reference
/// Infrastructure directly (Architecture §2.2, Coding Standards §3.1, ADR-0024).
/// No ViewModel, View, or other Presentation class may reference Infrastructure.
/// </summary>
public partial class App : System.Windows.Application
{
    private IHost? _host;

    protected override void OnStartup(StartupEventArgs e)
    {
        var builder = Host.CreateApplicationBuilder(e.Args);

        builder.Services.AddApplication();
        builder.Services.AddInfrastructure(builder.Configuration);
        builder.Services.AddPresentation();

        _host = builder.Build();
        _host.Start();

        var main = _host.Services.GetRequiredService<MainWindow>();
        main.Show();

        base.OnStartup(e);
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
