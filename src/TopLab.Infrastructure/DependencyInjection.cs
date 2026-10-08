using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Features.AccessAndNavigation.Common.Interfaces;
using TopLab.Application.Features.ExternalEntities.Common.Interfaces;
using TopLab.Application.Features.ResultsEntry.Common;
using TopLab.Infrastructure.Backup;
using TopLab.Infrastructure.Barcode;
using TopLab.Infrastructure.Identity;
using TopLab.Infrastructure.Persistence;
using TopLab.Infrastructure.Persistence.Interceptors;
using TopLab.Infrastructure.Persistence.Maintenance;
using TopLab.Infrastructure.Printing;
using TopLab.Infrastructure.Services;

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
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddSingleton<ICurrentUserService, CurrentUserService>();

        // External entities: stateless cryptographic code generator (M-14).
        services.AddSingleton<IEntityIdCodeGenerator, SecureEntityIdCodeGenerator>();
        services.AddScoped<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddScoped<IPatientReportPdfExporter, PatientReportPdfExporter>();

        // Report printing: PDF-first local writer + OS shell dispatch, routed
        // through PrinterAssignment (OutputType = Reports). Scoped, matching the
        // exporters (depend on the Scoped ApplicationDbContext).
        services.AddScoped<IReportPrintingService, ReportPrintingService>();
        // Barcode printing: Code-128 label renderer + routed dispatch (S-01 S1).
        services.AddScoped<IBarcodeService, BarcodeService>();
        services.AddScoped<BarcodeLabelRenderer>();
        // Receipt printing: cashier receipt document (S-01 S2). Scoped, matching
        // the M-22 printing services (depends on the Scoped ApplicationDbContext).
        services.AddScoped<IReceiptPrintingService, ReceiptPrintingService>();
        services.AddScoped<IReceiptPdfWriter, ReceiptPdfWriter>();
        // Invoice printing: itemized numbered statement of services (S-01 S3).
        services.AddScoped<IInvoicePrintingService, InvoicePrintingService>();
        services.AddScoped<IInvoicePdfWriter, InvoicePdfWriter>();
        // Patient-envelope printing: shared envelope writer + barcode pipeline
        // (Phase 1, REF-066 — owns all shared envelope infrastructure).
        services.AddScoped<IEnvelopePrintingService, EnvelopePrintingService>();
        services.AddScoped<IEnvelopePdfWriter, EnvelopePdfWriter>();
        // Laboratory-order slip printing (Phase 1, REF-068 — consumes REF-066's
        // barcode pipeline; routes to the Reports printer, decision 68-A).
        services.AddScoped<ILabOrderPrintingService, LabOrderPrintingService>();
        services.AddScoped<ILabOrderPdfWriter, LabOrderPdfWriter>();
        // Visit-worksheet printing: per-visit bench sheet (S-01 S4).
        services.AddScoped<IWorkSheetPrintingService, WorkSheetPrintingService>();
        services.AddScoped<IWorkSheetPdfWriter, WorkSheetPdfWriter>();

        // P-01 PP-03 (F8): price-list printing. Deliberately NOT a generic "list writer" —
        // the custom test-group list gets its own port and writer in S6 (C-6, AS-6).
        services.AddScoped<IPriceListPdfWriter, PriceListPdfWriter>();

        // P-01 PP-03 (F9): custom test-group list printing. Its own port and its own writer;
        // no shared class and no shared DTO with the price-list path above (C-6).
        services.AddScoped<ICustomGroupPdfWriter, CustomGroupPdfWriter>();

        // R-F05: banded result monitor printing. Its own port and its own writer over
        // BandedResultMonitorDto; no shared class or DTO with the two list writers above,
        // and no ReportKind / PrinterOutputType value (BR-F05-13).
        services.AddScoped<IBandedResultMonitorPdfWriter, BandedResultMonitorPdfWriter>();
        services.AddScoped<IReportPdfWriter, ReportPdfWriter>();
        services.AddScoped<IPdfPrinterDispatcher, ShellPdfPrinterDispatcher>();
        // WP-01: Arabic report document + on-screen preview (never prints).
        services.AddScoped<TopLab.Application.Features.ReportProduction.Common.IPdfPreviewService, PdfPreviewService>();
        services.AddScoped<TopLab.Application.Features.ReportProduction.Common.ITestDisplayNameResolver, TestDisplayNameResolver>();

        // M-01 redacted connection descriptor: stateless, depends only on
        // IConfiguration, so Singleton is appropriate. The full
        // IWorkstationConnectionSettingsProvider registration in the
        // Presentation layer is unchanged (M-01).
        services.AddSingleton<IDbConnectionDescriptor, SqlServerConnectionDescriptor>();

        // Backup/maintenance: Scoped (depends on the Scoped ApplicationDbContext).
        services.AddScoped<IDatabaseMaintenanceService, SqlServerDatabaseMaintenanceService>();

        // Daily backup hook runs in the background independently of any UI.
        services.AddHostedService<DailyBackupHostedService>();

        // W-02 S15 (WP-29): print-temp janitor for the owned folder only.
        services.AddHostedService<Hosting.TempPdfCleanupService>();

        // M-23 workstation-local utility lists (SD-23-2): no database table,
        // no migration — JSON files under %ProgramData%\TopLab.
        services.AddSingleton<IPurchasesListStore, JsonPurchasesListStore>();
        services.AddSingleton<IPhoneBookStore, JsonPhoneBookStore>();

        services.AddSingleton<IAppLogger, Logging.FileAppLogger>();

        // W-02 S13 (WP-29): dedicated sink for swallowed print exceptions (SD-5).
        services.AddSingleton<IPrintingDiagnostics, Logging.PrintingDiagnostics>();

        // W-02 S14 (WP-29): visit-edit transaction boundary.
        services.AddScoped<TopLab.Application.Common.Interfaces.IAppUnitOfWork, Persistence.AppUnitOfWork>();

        return services;
    }
}
