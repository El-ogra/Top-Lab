using MediatR;
using Microsoft.Extensions.DependencyInjection;
using TopLab.Application.Common.Behaviors;

namespace TopLab.Application;

/// <summary>
/// Registers Application-layer services. Pipeline behaviors run in the order they are
/// added here: Validation → Authorization → Logging, matching ADR-0009.
/// Infrastructure and Presentation ports are NOT resolved here (they live in their own
/// layer's DependencyInjection).
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        });

        return services;
    }
}
