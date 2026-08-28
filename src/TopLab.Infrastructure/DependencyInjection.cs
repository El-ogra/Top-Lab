using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TopLab.Application.Common.Interfaces;
using TopLab.Infrastructure.Identity;
using TopLab.Infrastructure.Persistence;
using TopLab.Infrastructure.Persistence.Interceptors;

namespace TopLab.Infrastructure;

/// <summary>
/// Registers the Infrastructure layer in the composition root. The Presentation
/// layer calls <c>AddInfrastructure</c> after <c>AddApplication</c> so every
/// port defined in Application has exactly one production implementation
/// (Architecture §4.3, Coding Standards §6.10).
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Connection settings are workstation-local and never stored in the
        // database (ADR-0021). The expected key is "ConnectionStrings:TopLab".
        var connectionString = configuration.GetConnectionString("TopLab")
            ?? throw new InvalidOperationException(
                "Missing connection string 'TopLab'. Configure it in the "
                + "workstation-local application settings before starting the app.");

        services.AddScoped<ISaveChangesInterceptor, AuditableEntitySaveChangesInterceptor>();

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.UseSqlServer(
                connectionString,
                sql => sql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.GetName().Name));

            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
        });

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        // Identity: Singleton for the desktop single-user session (one signed-in
        // person at a time, session lasts for app lifetime). Scoped would give
        // each request/unit-of-work a separate snapshot, causing stale/empty
        // identity across handlers and the audit interceptor. IDateTimeProvider
        // remains Scoped (stateless, per-operation clock).
        services.AddSingleton<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IDateTimeProvider, SystemDateTimeProvider>();

        return services;
    }
}
