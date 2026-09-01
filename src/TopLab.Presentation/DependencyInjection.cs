using Microsoft.Extensions.DependencyInjection;
using TopLab.Application.Common.Interfaces;
using TopLab.Presentation.Common.Navigation;
using TopLab.Presentation.Common.Dialogs;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.Services;
using TopLab.Presentation.ViewModels.Shell;
using TopLab.Presentation.ViewModels.Setup;
using TopLab.Presentation.ViewModels.Settings;
using TopLab.Presentation.ViewModels.Users;
using TopLab.Presentation.Views.Setup;

namespace TopLab.Presentation;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        // Services
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<ResultErrorPresenter>();
        services.AddSingleton<IAppLogger, WpfAppLogger>();
        services.AddSingleton<IPrinterCatalogService, PrinterCatalogService>();
        services.AddSingleton<TopLab.Presentation.Services.Configuration.ConfigurationFileService>();
        services.AddSingleton<TopLab.Application.Common.Interfaces.IWorkstationConnectionSettingsProvider, TopLab.Presentation.Services.Configuration.WorkstationConnectionSettingsProvider>();
        services.AddSingleton<TopLab.Application.Common.Interfaces.ILabPrintTextStore, TopLab.Presentation.Services.Configuration.JsonLabPrintTextStore>();

        // ViewModels
        services.AddTransient<ShellViewModel>();
        services.AddTransient<HomeViewModel>();
        services.AddTransient<DatabaseSetupViewModel>();
        services.AddTransient<FirstRunAdminViewModel>();
        services.AddTransient<UserManagementViewModel>();
        services.AddTransient<SettingsDashboardViewModel>();
        services.AddTransient<SystemSettingsViewModel>();
        services.AddTransient<ReportSettingsViewModel>();
        services.AddTransient<ReceiptSettingsViewModel>();
        services.AddTransient<EnvelopeSettingsViewModel>();

        // Windows
        services.AddTransient<FirstRunAdminWindow>();
        services.AddSingleton<MainWindow>();

        return services;
    }
}

internal sealed class WpfAppLogger : IAppLogger
{
    public void Log(string requestName, string outcome, TimeSpan duration)
    {
        // Minimal console logging; can be replaced with proper logger later
        System.Diagnostics.Debug.WriteLine($"[{requestName}] {outcome} {duration.TotalMilliseconds:F0}ms");
    }
}
