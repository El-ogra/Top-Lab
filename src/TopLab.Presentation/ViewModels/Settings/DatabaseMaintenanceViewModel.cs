using MediatR;
using TopLab.Application.Features.SystemAndPrintSettings.Commands.ApplyDatabaseUpdates;
using TopLab.Application.Features.SystemAndPrintSettings.Commands.BackupDatabaseNow;
using TopLab.Application.Features.SystemAndPrintSettings.Commands.RestoreDatabase;
using TopLab.Application.Features.SystemAndPrintSettings.Queries.GetSystemSettings;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.Dialogs;
using TopLab.Presentation.Common.ErrorPresentation;

namespace TopLab.Presentation.ViewModels.Settings;

/// <summary>S-32: database maintenance — backup now, restore with safety notice,
/// and apply updates / system initialization. Entry is gated by the secondary
/// password in the calling dashboard.</summary>
public sealed class DatabaseMaintenanceViewModel : ViewModelBase
{
    private readonly ISender _mediator;
    private readonly IDialogService _dialogs;
    private readonly ResultErrorPresenter _presenter;

    private string _statusMessage = string.Empty;
    private string _errorMessage = string.Empty;
    private bool _isBusy;
    private string _backupFolder = string.Empty;

    public DatabaseMaintenanceViewModel(
        ISender mediator,
        IDialogService dialogs,
        ResultErrorPresenter presenter)
    {
        _mediator = mediator;
        _dialogs = dialogs;
        _presenter = presenter;

        BackupNowCommand = new AsyncRelayCommand(_ => BackupNowAsync());
        RestoreCommand = new AsyncRelayCommand(_ => RestoreAsync());
        ApplyUpdatesCommand = new AsyncRelayCommand(_ => ApplyUpdatesAsync());
        LoadCommand = new AsyncRelayCommand(_ => LoadAsync());
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set => SetProperty(ref _isBusy, value);
    }

    public string BackupFolder
    {
        get => _backupFolder;
        private set => SetProperty(ref _backupFolder, value);
    }

    public AsyncRelayCommand LoadCommand { get; }
    public AsyncRelayCommand BackupNowCommand { get; }
    public AsyncRelayCommand RestoreCommand { get; }
    public AsyncRelayCommand ApplyUpdatesCommand { get; }

    public async Task LoadAsync()
    {
        ErrorMessage = string.Empty;
        var settings = await _mediator.Send(new GetSystemSettingsQuery());
        if (settings.IsSuccess && settings.Value is not null
            && !string.IsNullOrWhiteSpace(settings.Value.DailyBackupPath))
        {
            BackupFolder = settings.Value.DailyBackupPath;
        }
    }

    public async Task BackupNowAsync()
    {
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;

        var folder = await _dialogs.PickBackupFolderAsync(BackupFolder);
        if (string.IsNullOrWhiteSpace(folder))
        {
            return;
        }

        IsBusy = true;
        try
        {
            var result = await _mediator.Send(new BackupDatabaseNowCommand(folder));
            if (result.IsSuccess && !string.IsNullOrWhiteSpace(result.Value))
            {
                StatusMessage = $"تم إنشاء النسخة الاحتياطية: {result.Value}";
            }
            else if (result.Error is not null)
            {
                ErrorMessage = _presenter.Present(result.Error);
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task RestoreAsync()
    {
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;

        var file = await _dialogs.PickBackupFileAsync();
        if (string.IsNullOrWhiteSpace(file))
        {
            return;
        }

        bool confirm = await _dialogs.ShowConfirmationAsync(
            "استعادة النسخة الاحتياطية",
            "سيتم عمل نسخة احتياطية احترازية قبل الاستعادة، ويجب إعادة تشغيل النظام بعدها. هل تريد المتابعة؟");
        if (!confirm)
        {
            return;
        }

        IsBusy = true;
        try
        {
            var result = await _mediator.Send(new RestoreDatabaseCommand(file));
            if (result.IsSuccess)
            {
                StatusMessage = "تمت الاستعادة بنجاح. يرجى إعادة تشغيل النظام.";
            }
            else if (result.Error is not null)
            {
                ErrorMessage = _presenter.Present(result.Error);
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task ApplyUpdatesAsync()
    {
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;

        bool confirm = await _dialogs.ShowConfirmationAsync(
            "تطبيق التحديثات",
            "سيتم تطبيق تحديثات قاعدة البيانات المعلقة والتحقق من بيانات التهيئة. هل تريد المتابعة؟");
        if (!confirm)
        {
            return;
        }

        IsBusy = true;
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
                ErrorMessage = _presenter.Present(result.Error);
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
}