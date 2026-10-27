using System.Collections.ObjectModel;
using MediatR;
using TopLab.Application.Features.ReportProduction.Commands.InsertHistoryResult;
using TopLab.Application.Features.ReportProduction.Common;
using TopLab.Application.Features.ReportProduction.Queries.GetPatientTestHistory;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.Dialogs;
using TopLab.Presentation.Common.ErrorPresentation;

namespace TopLab.Presentation.ViewModels.Patients;

/// <summary>
/// S-05 Slice 2: Insert-history dialog (M07) — select a source history entry and insert it.
/// </summary>
public sealed class InsertHistoryDialogViewModel : ViewModelBase
{
    private readonly ISender _mediator;
    private readonly ResultErrorPresenter _presenter;
    private readonly IDialogService _dialogs;

    private int _patientId;
    private int _targetPatientTestId;
    private string _targetTestName = string.Empty;
    private ObservableCollection<HistoryEntryDto> _entries = new();
    private HistoryEntryDto? _selectedEntry;
    private bool _isBusy;
    private bool _isAbandoned;
    private string _errorMessage = string.Empty;

    public InsertHistoryDialogViewModel(ISender mediator, ResultErrorPresenter presenter, IDialogService dialogs)
    {
        _mediator = mediator;
        _presenter = presenter;
        _dialogs = dialogs;
    }

    public string TargetTestName { get => _targetTestName; private set => SetProperty(ref _targetTestName, value); }

    public CombinedReportDto? InsertedReport { get; private set; }

    public ObservableCollection<HistoryEntryDto> Entries
    {
        get => _entries;
        private set
        {
            if (SetProperty(ref _entries, value))
            {
                OnPropertyChanged(nameof(HasEntries));
                OnPropertyChanged(nameof(ShowEmpty));
            }
        }
    }

    public bool HasEntries => Entries.Count > 0;
    public bool ShowEmpty => Entries.Count == 0 && !IsBusy && string.IsNullOrEmpty(ErrorMessage);

    public HistoryEntryDto? SelectedEntry
    {
        get => _selectedEntry;
        set => SetProperty(ref _selectedEntry, value);
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

    public async Task LoadAsync(int patientId, int targetPatientTestId, string targetTestName, CancellationToken cancellationToken = default)
    {
        if (_isAbandoned)
        {
            return;
        }

        _patientId = patientId;
        _targetPatientTestId = targetPatientTestId;
        TargetTestName = targetTestName;
        InsertedReport = null;
        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            var result = await _mediator.Send(new GetPatientTestHistoryQuery(patientId), cancellationToken);
            if (_isAbandoned)
            {
                // GAP-27: the operation was abandoned while loading — a late
                // result must not repopulate the dialog.
                return;
            }

            if (result.IsSuccess && result.Value is not null)
            {
                // W-02 S12 (WP-10): same-test filter — entries whose test name matches
                // the insertion target. Name-based (the dialog knows the target name,
                // not its id); documented, not silent.
                var allEntries = result.Value.Entries
                    .Where(e => e.TestName == TargetTestName)
                    .ToList();
                Entries = new ObservableCollection<HistoryEntryDto>(allEntries);
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

    public async Task<bool> InsertAsync()
    {
        if (_isAbandoned)
        {
            // GAP-27: an abandoned operation must never insert.
            return false;
        }

        if (SelectedEntry is null)
        {
            ErrorMessage = "اختر نتيجة تاريخية للإدراج.";
            return false;
        }

        var confirmed = await _dialogs.ShowConfirmationAsync(
            "تأكيد الإدراج",
            $"هل تريد إدراج النتيجة التاريخية للتحليل «{TargetTestName}»؟");
        if (!confirmed || _isAbandoned)
        {
            return false;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            var result = await _mediator.Send(
                new InsertHistoryResultCommand(_targetPatientTestId, SelectedEntry.PatientTestId));

            if (_isAbandoned)
            {
                // GAP-27: the dialog was abandoned while the command was in
                // flight — do not surface the report as if it were applied.
                return false;
            }

            if (result.IsSuccess && result.Value is not null)
            {
                InsertedReport = result.Value;
                return true;
            }

            if (result.Error is not null)
            {
                ErrorMessage = _presenter.Present(result.Error);
            }

            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// GAP-27 (1.15.3.4, Arabic label "الغاء الحالى"): abandons the current
    /// history operation. No <see cref="InsertHistoryResultCommand"/> is
    /// dispatched, no report is applied, the pending selection is discarded and
    /// any late load/insert result is ignored. The view closes the dialog after
    /// calling this method.
    /// </summary>
    public void Cancel()
    {
        _isAbandoned = true;
        SelectedEntry = null;
        InsertedReport = null;
        ErrorMessage = string.Empty;
    }
}
