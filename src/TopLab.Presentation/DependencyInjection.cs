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
using TopLab.Presentation.ViewModels.Patients;
using TopLab.Presentation.ViewModels.Attendance;
using TopLab.Presentation.ViewModels.Statistics;
using TopLab.Presentation.ViewModels.Accounts;
using TopLab.Presentation.ViewModels.Audit;
using TopLab.Presentation.ViewModels.Utilities;
using TopLab.Presentation.ViewModels.WorkSheets;
using TopLab.Presentation.ViewModels.Lab;
using TopLab.Presentation.ViewModels.External;
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
        services.AddTransient<UnlockViewModel>();
        services.AddTransient<DatabaseSetupViewModel>();
        services.AddTransient<FirstRunAdminViewModel>();
        services.AddTransient<LoginViewModel>();
        services.AddTransient<UserManagementViewModel>();
        services.AddTransient<ChangeOwnPasswordViewModel>();
        services.AddTransient<PatientsHubViewModel>();
                services.AddTransient<PatientEditorViewModel>();
                services.AddTransient<PatientAccountViewModel>();
                services.AddTransient<ResultsWorklistViewModel>();
                                services.AddTransient<SimpleResultEntryViewModel>();
                                services.AddTransient<PatientResultSheetViewModel>();
                                services.AddTransient<BulkPrintDialogViewModel>();
                                services.AddTransient<ProfileEntryViewModel>();
                                services.AddTransient<AmendDialogViewModel>();
                                services.AddTransient<AmendmentsLogViewModel>();
                                services.AddTransient<CultureEntryViewModel>();
                                services.AddTransient<PatientSearchViewModel>();
                                services.AddTransient<PatientVisitHistoryViewModel>();
                                services.AddTransient<CombinedReportViewModel>();
                                services.AddTransient<InsertHistoryDialogViewModel>();
                                services.AddTransient<BlankReportViewModel>();
                                services.AddTransient<HistoryReportsViewModel>();
                                services.AddTransient<ResultDeliveryViewModel>();
                                services.AddTransient<DeliveryHandoverViewModel>();
                                services.AddTransient<SentOutSamplesViewModel>();
                                services.AddTransient<SendSampleOutDialogViewModel>();
                                services.AddTransient<SentOutLabAccountViewModel>();
                                services.AddTransient<MyAttendanceViewModel>();
                                services.AddTransient<AttendanceRecordsViewModel>();
                                services.AddTransient<UserAttendanceSummaryViewModel>();
                                services.AddTransient<StatisticsViewModel>();
                                services.AddTransient<AccountsHubViewModel>();
                                services.AddTransient<CashMovementDialogViewModel>();
                                services.AddTransient<AuditViewModel>();
                                services.AddTransient<UtilitiesViewModel>();
                                services.AddTransient<WorkSheetsViewModel>();
        services.AddTransient<CorrectionDialogViewModel>();
        services.AddTransient<ExtraChargeDialogViewModel>();
        services.AddTransient<LabHubViewModel>();
        services.AddTransient<TestCatalogViewModel>();
        services.AddTransient<TestEditorViewModel>();
        services.AddTransient<TestGroupsViewModel>();
        services.AddTransient<WorkGroupLogsViewModel>();
        services.AddTransient<AnalytesViewModel>();
        services.AddTransient<AnalyteEditorViewModel>();
        services.AddTransient<ProfilesViewModel>();
        services.AddTransient<ProfileEditorViewModel>();
        services.AddTransient<AntibioticsViewModel>();
        services.AddTransient<AntibioticEditorViewModel>();
        services.AddTransient<CultureAttachmentViewModel>();
        services.AddTransient<PriceListsViewModel>();
        services.AddTransient<TestCommentsViewModel>();
        services.AddTransient<CustomGroupsViewModel>();
        services.AddTransient<SampleCollectionViewModel>();
        services.AddTransient<ExternalEntitiesViewModel>();
        services.AddTransient<ExternalEntityEditorViewModel>();
        services.AddTransient<ExternalEntityPickerViewModel>();
        services.AddTransient<SettingsDashboardViewModel>();
        services.AddTransient<SystemSettingsViewModel>();
        services.AddTransient<ReportSettingsViewModel>();
        services.AddTransient<ReceiptSettingsViewModel>();
        services.AddTransient<EnvelopeSettingsViewModel>();
        services.AddTransient<DatabaseMaintenanceViewModel>();

        // Windows
        services.AddTransient<FirstRunAdminWindow>();
        services.AddTransient<LoginWindow>();
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
