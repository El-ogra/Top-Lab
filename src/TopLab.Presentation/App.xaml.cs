using System.IO;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using MediatR;
using TopLab.Application;
using TopLab.Application.Features.UsersAndPermissions.Queries.HasAnyAbsoluteUser;
using TopLab.Infrastructure;
using TopLab.Infrastructure.Persistence;
using TopLab.Presentation.Services.Configuration;
using TopLab.Presentation.ViewModels.Setup;
using TopLab.Presentation.Views.Setup;

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

        var programDataDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "TopLab");
        Directory.CreateDirectory(programDataDir);
        var programDataConfigPath = Path.Combine(programDataDir, "appsettings.json");
        builder.Configuration.AddJsonFile(programDataConfigPath, optional: true, reloadOnChange: true);

        if (string.IsNullOrWhiteSpace(builder.Configuration.GetConnectionString("TopLab")))
        {
            var svc = new ConfigurationFileService();
            var vm = new DatabaseSetupViewModel(svc);
            var win = new DatabaseSetupWindow(vm);
            var r = win.ShowDialog() ?? false;
            if (!r)
            {
                Shutdown(1);
                return;
            }
            builder.Configuration.AddJsonFile(svc.GetProgramDataPath(), optional: false, reloadOnChange: true);
        }

        builder.Services.AddApplication();
        builder.Services.AddInfrastructure(builder.Configuration);
        builder.Services.AddPresentation();

        _host = builder.Build();
        _host.Start();

        try
        {
            using var s = _host.Services.CreateScope();
            var db = s.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.MigrateAsync().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"فشل المهاجرات:\n{ex.Message}", "Top-Lab", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
            return;
        }

        try
        {
            using var scope = _host.Services.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<ISender>();
            var hasAnyResult = mediator.Send(new HasAnyAbsoluteUserQuery()).GetAwaiter().GetResult();
            bool hasAny = hasAnyResult.IsSuccess && hasAnyResult.Value;
            if (!hasAny)
            {
                var vm = scope.ServiceProvider.GetRequiredService<FirstRunAdminViewModel>();
                var window = new FirstRunAdminWindow(vm);
                var dialogResult = window.ShowDialog();
                if (dialogResult != true)
                {
                    Shutdown(1);
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"فشل التحقق من مدير النظام:\n{ex.Message}", "Top-Lab", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
            return;
        }

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
