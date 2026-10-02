using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using TopLab.Application.Common.Behaviors;
using TopLab.Application.Features.ResultsEntry.Common;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.CreateTest;

namespace TopLab.Application;

/// <summary>
/// Registers Application-layer services. Pipeline behaviors run in the order they are
/// added here: Logging → Validation → Authorization, matching the registrations below and ADR-0009.
/// Infrastructure and Presentation ports are NOT resolved here (they live in their own
/// layer's DependencyInjection).
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddValidatorsFromAssemblyContaining<CreateTestCommandValidator>();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));
        });

        // W-02 S4 (WP-06): the one honest print path. Scoped, because it depends on the
                // scoped IReportPrintingService.
                services.AddScoped<IResultPrintCoordinator, ResultPrintCoordinator>();

                // W-02 post-implementation fix (owner decision 1: printed = successful printing).
                // The single place where printed state is recorded, called only after the printer
                // succeeds. Scoped because it depends on the scoped IApplicationDbContext.
                services.AddScoped<IPrintedStateRecorder, PrintedStateRecorder>();

        return services;
    }
}
