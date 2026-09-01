using MediatR;
using TopLab.Application.Features.SystemAndPrintSettings.Commands.ApplyDatabaseUpdates;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.Dialogs;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.Common.Navigation;

namespace TopLab.Presentation.ViewModels.Settings;

/// <summary>S-27: launcher into the settings sections, the system-initialization
/// action, and (Database Maintenance wired in S7; report/receipt/envelope
/// navigation added in S6 alongside their screens).</summary>
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
    public AsyncRelayCommand RunSystemInitializationCommand { get; }

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