using System.Collections.ObjectModel;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using TopLab.Application.Features.ProfileResults.Commands.AmendProfileResult;
using TopLab.Application.Features.ProfileResults.Commands.MarkProfilePrinted;
using TopLab.Application.Features.ProfileResults.Commands.SaveProfileResults;
using TopLab.Application.Features.ProfileResults.Commands.UnverifyProfileResults;
using TopLab.Application.Features.ProfileResults.Commands.VerifyProfileResults;
using TopLab.Application.Features.ProfileResults.Common;
using TopLab.Application.Features.ProfileResults.Queries.GetProfileEntryGrid;
using TopLab.Application.Features.ProfileResults.Queries.GetProfileReport;
using TopLab.Application.Features.ProfileResults.Queries.GetProfileResultAmendments;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.Dialogs;
using TopLab.Presentation.Common.ErrorPresentation;

namespace TopLab.Presentation.ViewModels.Patients;

/// <summary>
/// Editable row wrapper for the profile entry grid (P1).
/// </summary>
public sealed class ProfileEntryRow : ViewModelBase
{
    private string? _resultValue;
    private string? _unit;
    private int? _flag;

    public int ProfileResultItemId { get; init; }
    public int AnalyteId { get; init; }
    public string AnalyteName { get; init; } = string.Empty;
    public bool IsVerified { get; init; }
    public bool IsPrinted { get; init; }
    public FrozenProfileRangeDto? FrozenRange { get; init; }

    public string? ResultValue
    {
        get => _resultValue;
        set => SetProperty(ref _resultValue, value);
    }

    public string? Unit
    {
        get => _unit;
        set => SetProperty(ref _unit, value);
    }

    public int? Flag
    {
        get => _flag;
        set => SetProperty(ref _flag, value);
    }

    public bool CanEdit => !IsVerified && !IsPrinted;
    public bool CanAmend => IsPrinted;
}

/// <summary>
/// S-04 Slice 6: Profile entry grid (P1) for specialized profile results.
/// Routing: R1 → P1 for SpecializedProfile rows (settled D4).
/// Reuses ResultsEntryAccessPolicy (no separate policy class — SD-6 audit-verified).
/// </summary>
public sealed class ProfileEntryViewModel : ViewModelBase
{
    private readonly ISender _mediator;
    private readonly ResultErrorPresenter _presenter;
    private readonly IDialogService _dialogs;
    private readonly IServiceProvider _services;

    private int _patientTestId;
    private string _patientFullName = string.Empty;
    private string _profileName = string.Empty;
    private string _comment = string.Empty;
    private ObservableCollection<ProfileEntryRow> _rows = new();
    private ProfileReportDto? _report;
    private bool _isBusy;
    private string _errorMessage = string.Empty;
    private string _statusMessage = string.Empty;
    private ProfileEntryRow? _selectedRow;

    public ProfileEntryViewModel(
        ISender mediator,
        ResultErrorPresenter presenter,
        IDialogService dialogs,
        IServiceProvider services)
    {
        _mediator = mediator;
        _presenter = presenter;
        _dialogs = dialogs;
        _services = services;

        SaveCommand = new AsyncRelayCommand(async (_, ct) => await SaveAsync(ct));
        VerifyCommand = new AsyncRelayCommand(async (_, ct) => await VerifyAsync(ct));
        UnverifyCommand = new AsyncRelayCommand(async (_, ct) => await UnverifyAsync(ct));
        PrintCommand = new AsyncRelayCommand(async (_, ct) => await PrintAsync(ct));
        ShowReportCommand = new AsyncRelayCommand(async (_, ct) => await ShowReportAsync(ct));
        AmendCommand = new AsyncRelayCommand(async (_, ct) => await AmendAsync(ct));
        ShowAmendmentsLogCommand = new AsyncRelayCommand(async (_, ct) => await ShowAmendmentsLogAsync(ct));
    }

    public int PatientTestId => _patientTestId;
    public string PatientFullName { get => _patientFullName; private set => SetProperty(ref _patientFullName, value); }
    public string ProfileName { get => _profileName; private set => SetProperty(ref _profileName, value); }

    public string Comment
    {
        get => _comment;
        set => SetProperty(ref _comment, value);
    }

    public ObservableCollection<ProfileEntryRow> Rows
    {
        get => _rows;
        private set
        {
            if (SetProperty(ref _rows, value))
            {
                OnPropertyChanged(nameof(HasRows));
                OnPropertyChanged(nameof(ShowEmpty));
            }
        }
    }

    public bool HasRows => Rows.Count > 0;
    public bool ShowEmpty => Rows.Count == 0 && _patientTestId > 0;

    public ProfileReportDto? Report
    {
        get => _report;
        private set
        {
            if (SetProperty(ref _report, value))
            {
                OnPropertyChanged(nameof(HasReport));
            }
        }
    }

    public bool HasReport => _report is not null;

    public ProfileEntryRow? SelectedRow
    {
        get => _selectedRow;
        set => SetProperty(ref _selectedRow, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set => SetProperty(ref _isBusy, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public AsyncRelayCommand SaveCommand { get; }
    public AsyncRelayCommand VerifyCommand { get; }
    public AsyncRelayCommand UnverifyCommand { get; }
    public AsyncRelayCommand PrintCommand { get; }
    public AsyncRelayCommand ShowReportCommand { get; }
    public AsyncRelayCommand AmendCommand { get; }
    public AsyncRelayCommand ShowAmendmentsLogCommand { get; }

    public async Task LoadAsync(int patientTestId, CancellationToken cancellationToken = default)
    {
        _patientTestId = patientTestId;
        IsBusy = true;
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;
        Report = null;

        try
        {
            var result = await _mediator.Send(new GetProfileEntryGridQuery(patientTestId), cancellationToken);
            if (result.IsSuccess && result.Value is not null)
            {
                PatientFullName = result.Value.PatientFullName;
                ProfileName = result.Value.ProfileName;
                Comment = string.Empty;

                var rows = new ObservableCollection<ProfileEntryRow>();
                foreach (var item in result.Value.Items)
                {
                    rows.Add(new ProfileEntryRow
                    {
                        ProfileResultItemId = item.ProfileResultItemId,
                        AnalyteId = item.AnalyteId,
                        AnalyteName = item.AnalyteName,
                        ResultValue = item.ResultValue,
                        Unit = item.Unit,
                        Flag = item.Flag,
                        IsVerified = item.IsVerified,
                        IsPrinted = item.IsPrinted,
                        FrozenRange = item.FrozenRange
                    });
                }

                Rows = rows;
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

    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        if (_patientTestId == 0)
        {
            return;
        }

        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;

        var items = Rows
            .Where(r => !string.IsNullOrWhiteSpace(r.ResultValue))
            .Select(r => new ProfileItemInput(r.AnalyteId, r.ResultValue!, r.Unit, r.Flag))
            .ToList();

        IsBusy = true;
        try
        {
            var result = await _mediator.Send(
                new SaveProfileResultsCommand(_patientTestId, string.IsNullOrWhiteSpace(Comment) ? null : Comment, items),
                cancellationToken);

            if (result.IsSuccess)
            {
                StatusMessage = "تم حفظ النتائج.";
                await LoadAsync(_patientTestId, cancellationToken);
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

    private async Task VerifyAsync(CancellationToken cancellationToken)
    {
        if (_patientTestId == 0)
        {
            return;
        }

        var confirmed = await _dialogs.ShowConfirmationAsync(
            "تأكيد الاعتماد",
            "هل تريد اعتماد نتائج هذا البروفايل؟");
        if (!confirmed)
        {
            return;
        }

        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;
        IsBusy = true;
        try
        {
            var result = await _mediator.Send(new VerifyProfileResultsCommand(_patientTestId), cancellationToken);
            if (result.IsSuccess)
            {
                StatusMessage = "تم اعتماد النتائج.";
                await LoadAsync(_patientTestId, cancellationToken);
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

    private async Task UnverifyAsync(CancellationToken cancellationToken)
    {
        if (_patientTestId == 0)
        {
            return;
        }

        var confirmed = await _dialogs.ShowConfirmationAsync(
            "تأكيد إلغاء الاعتماد",
            "هل تريد إلغاء اعتماد نتائج هذا البروفايل؟");
        if (!confirmed)
        {
            return;
        }

        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;
        IsBusy = true;
        try
        {
            var result = await _mediator.Send(new UnverifyProfileResultsCommand(_patientTestId), cancellationToken);
            if (result.IsSuccess)
            {
                StatusMessage = "تم إلغاء الاعتماد.";
                await LoadAsync(_patientTestId, cancellationToken);
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

    private async Task PrintAsync(CancellationToken cancellationToken)
    {
        if (_patientTestId == 0)
        {
            return;
        }

        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;
        IsBusy = true;
        try
        {
            var result = await _mediator.Send(new MarkProfilePrintedCommand(_patientTestId), cancellationToken);
            if (result.IsSuccess)
            {
                StatusMessage = "تم الطباعة.";
                await LoadAsync(_patientTestId, cancellationToken);
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

    private async Task ShowReportAsync(CancellationToken cancellationToken)
    {
        if (_patientTestId == 0)
        {
            return;
        }

        ErrorMessage = string.Empty;
        IsBusy = true;
        try
        {
            var result = await _mediator.Send(new GetProfileReportQuery(_patientTestId), cancellationToken);
            if (result.IsSuccess && result.Value is not null)
            {
                Report = result.Value;
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

    private async Task AmendAsync(CancellationToken cancellationToken)
    {
        if (SelectedRow is null || !SelectedRow.CanAmend)
        {
            ErrorMessage = "اختر مادة مطبوعة لتعديلها.";
            return;
        }

        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;

        var vm = _services.GetRequiredService<AmendDialogViewModel>();
        vm.Setup(SelectedRow.ProfileResultItemId, SelectedRow.AnalyteName, SelectedRow.ResultValue, SelectedRow.Unit, SelectedRow.Flag);
        var window = new Views.Patients.AmendDialogWindow(vm)
        {
            Owner = System.Windows.Application.Current?.MainWindow
        };

        bool? confirmed = window.ShowDialog();
        if (confirmed == true)
        {
            // Reload after amend
            await LoadAsync(_patientTestId, cancellationToken);
            StatusMessage = "تم تسجيل التعديل.";
        }
    }

    private async Task ShowAmendmentsLogAsync(CancellationToken cancellationToken)
    {
        if (SelectedRow is null)
        {
            ErrorMessage = "اختر مادة لعرض سجل التعديلات.";
            return;
        }

        ErrorMessage = string.Empty;

        var vm = _services.GetRequiredService<AmendmentsLogViewModel>();
        await vm.LoadAsync(SelectedRow.ProfileResultItemId, cancellationToken);
        var window = new Views.Patients.AmendmentsLogWindow(vm)
        {
            Owner = System.Windows.Application.Current?.MainWindow
        };
        window.ShowDialog();
    }
}
