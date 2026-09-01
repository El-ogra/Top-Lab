using Microsoft.Extensions.DependencyInjection;
using TopLab.Application.Common.Interfaces;
using TopLab.Presentation.Common.Navigation;
using TopLab.Presentation.Common.Dialogs;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.ViewModels.Shell;
using TopLab.Presentation.ViewModels.Setup;
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
        services.AddSingleton<TopLab.Presentation.Services.Configuration.ConfigurationFileService>();

        // ViewModels
        services.AddTransient<ShellViewModel>();
        services.AddTransient<HomeViewModel>();
        services.AddTransient<DatabaseSetupViewModel>();
        services.AddTransient<FirstRunAdminViewModel>();

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
