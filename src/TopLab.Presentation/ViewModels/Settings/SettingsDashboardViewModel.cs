using MediatR;
using TopLab.Application.Features.SystemAndPrintSettings.Commands.ApplyDatabaseUpdates;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.Dialogs;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.Common.Navigation;

namespace TopLab.Presentation.ViewModels.Settings;

/// <summary>S-27: launcher into the settings sections and the system-initialization
/// action (Database Maintenance entry added in S7).</summary>
public sealed class SettingsDashboardViewModel : ViewModelBase
{
    private readonly ISender _mediator;
    private readonly IDialogService _dialogs;
    private readonly ResultErrorPresenter _presenter;
    private string _statusMessage = string.Empty;
    private bool _isBusy;

    public SettingsDashboardViewModel(
        ISender mediator,
        INavigationService navigation,
        IDialogService dialogs,
        ResultErrorPresenter presenter)
    {
        _mediator = mediator;
        _dialogs = dialogs;
        _presenter = presenter;

        OpenSystemSettingsCommand = new RelayCommand(_ => navigation.NavigateTo<SystemSettingsViewModel>());
        OpenReportSettingsCommand = new RelayCommand(_ => navigation.NavigateTo<ReportSettingsViewModel>());
        OpenReceiptSettingsCommand = new RelayCommand(_ => navigation.NavigateTo<ReceiptSettingsViewModel>());
        OpenEnvelopeSettingsCommand = new RelayCommand(_ => navigation.NavigateTo<EnvelopeSettingsViewModel>());
        OpenDatabaseMaintenanceCommand = new AsyncRelayCommand(_ => OpenDatabaseMaintenanceAsync(navigation));
        RunSystemInitializationCommand = new AsyncRelayCommand(_ => RunSystemInitializationAsync());
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set => SetProperty(ref _isBusy, value);
    }

    public RelayCommand OpenSystemSettingsCommand { get; }
    public RelayCommand OpenReportSettingsCommand { get; }
    public RelayCommand OpenReceiptSettingsCommand { get; }
    public RelayCommand OpenEnvelopeSettingsCommand { get; }
    public AsyncRelayCommand OpenDatabaseMaintenanceCommand { get; }
    public AsyncRelayCommand RunSystemInitializationCommand { get; }

    public async Task OpenDatabaseMaintenanceAsync(INavigationService navigation)
    {
        bool granted = await _dialogs.ShowSecondaryPasswordDialogAsync();
        if (!granted)
        {
            await _dialogs.ShowErrorAsync("لم يتم التحقق من كلمة المرور الثانوية. لا يمكن فتح صيانة قاعدة البيانات.");
            return;
        }

        navigation.NavigateTo<DatabaseMaintenanceViewModel>();
    }

    public async Task RunSystemInitializationAsync()
    {
        bool confirm = await _dialogs.ShowConfirmationAsync(
            "تهيئة النظام",
            "سيتم تطبيق تحديثات قاعدة البيانات المعلقة والتحقق من بيانات التهيئة. هل تريد المتابعة؟");
        if (!confirm)
        {
            return;
        }

        IsBusy = true;
        StatusMessage = string.Empty;
        try
        {
            var result = await _mediator.Send(new ApplyDatabaseUpdatesCommand());
            if (result.IsSuccess && result.Value is not null)
            {
                StatusMessage = result.Value.MigrationsApplied == 0
                    ? "تم تطبيق التحديثات والتحقق من بيانات التهيئة."
                    : $"تم تطبيق {result.Value.MigrationsApplied} تحديث (تحديثات) وفحص بيانات التهيئة.";
            }
            else if (result.Error is not null)
            {
                StatusMessage = _presenter.Present(result.Error);
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
}