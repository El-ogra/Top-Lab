using System.Collections.ObjectModel;
using MediatR;
using TopLab.Application.Features.ReportProduction.Commands.PrintHistoryReport;
using TopLab.Application.Features.ReportProduction.Common;
using TopLab.Application.Features.ReportProduction.Queries.GetMultiPatientHistory;
using TopLab.Application.Features.ReportProduction.Queries.GetPatientTestHistory;
using TopLab.Application.Features.ReportProduction.Queries.GetSeparateHistoryReport;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.Dialogs;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.Common.Navigation;

namespace TopLab.Presentation.ViewModels.Patients;

/// <summary>
/// S-05 Slice 3: History reports screen (M07) — three modes: single patient, multi-patient, separate.
/// </summary>
public sealed class HistoryReportsViewModel : ViewModelBase
{
    private readonly ISender _mediator;
    private readonly ResultErrorPresenter _presenter;
    private readonly IDialogService _dialogs;
    private readonly INavigationService _navigation;

    private int _patientId;
    private int _selectedMode; // 0=single, 1=multi, 2=separate
    private PatientHistoryDto? _singleHistory;
    private MultiPatientHistoryDto? _multiHistory;
    private ObservableCollection<HistoryEntryDto> _entries = new();
    private DateTime? _fromDate;
    private DateTime? _toDate;
    private string? _testIdText;
    private bool _printSeparately;
    private bool _isBusy;
    private string _errorMessage = string.Empty;
    private string _statusMessage = string.Empty;

    public HistoryReportsViewModel(
        ISender mediator,
        ResultErrorPresenter presenter,
        IDialogService dialogs,
        INavigationService navigation)
    {
        _mediator = mediator;
        _presenter = presenter;
        _dialogs = dialogs;
        _navigation = navigation;

        LoadSingleCommand = new AsyncRelayCommand(async (_, ct) => await LoadSingleAsync(ct));
        LoadMultiCommand = new AsyncRelayCommand(async (_, ct) => await LoadMultiAsync(ct));
        LoadSeparateCommand = new AsyncRelayCommand(async (_, ct) => await LoadSeparateAsync(ct));
        PrintHistoryCommand = new AsyncRelayCommand(async (_, ct) => await PrintHistoryAsync(ct));
        BackCommand = new RelayCommand(_ => _navigation.NavigateTo<PatientVisitHistoryViewModel>());
    }

    public int PatientId => _patientId;

    public int SelectedMode
    {
        get => _selectedMode;
        set
        {
            if (SetProperty(ref _selectedMode, value))
            {
                OnPropertyChanged(nameof(IsSingleMode));
                OnPropertyChanged(nameof(IsMultiMode));
                OnPropertyChanged(nameof(IsSeparateMode));
            }
        }
    }

    public bool IsSingleMode => SelectedMode == 0;
    public bool IsMultiMode => SelectedMode == 1;
    public bool IsSeparateMode => SelectedMode == 2;

    public string HistorySortMode => _singleHistory?.HistorySortMode ?? _multiHistory?.HistorySortMode ?? string.Empty;
    public bool HistoryAutoDisplayEnabled => _singleHistory?.HistoryAutoDisplayEnabled ?? _multiHistory?.HistoryAutoDisplayEnabled ?? false;

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

    /// <summary>W-02 S12 (WP-10, C-17): the multi-patient list is public so the
    /// view (and tests) can populate it — previously it was always empty.</summary>
    public ObservableCollection<int> MultiPatientIds { get; } = new();

    public DateTime? FromDate
    {
        get => _fromDate;
        set => SetProperty(ref _fromDate, value);
    }

    public DateTime? ToDate
    {
        get => _toDate;
        set => SetProperty(ref _toDate, value);
    }

    public string? TestIdText
    {
        get => _testIdText;
        set => SetProperty(ref _testIdText, value);
    }

    public bool PrintSeparately
    {
        get => _printSeparately;
        set => SetProperty(ref _printSeparately, value);
    }

    private DateOnly? FromFilter => FromDate is null ? null : DateOnly.FromDateTime(FromDate.Value);
    private DateOnly? ToFilter => ToDate is null ? null : DateOnly.FromDateTime(ToDate.Value);

    private int? TestIdFilter
    {
        get
        {
            if (string.IsNullOrWhiteSpace(TestIdText))
            {
                return null;
            }

            return int.TryParse(TestIdText.Trim(), out var id) && id > 0 ? id : null;
        }
    }

    public bool HasEntries => Entries.Count > 0;
    public bool ShowEmpty => Entries.Count == 0 && !IsBusy && string.IsNullOrEmpty(ErrorMessage);

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

    public AsyncRelayCommand LoadSingleCommand { get; }
    public AsyncRelayCommand LoadMultiCommand { get; }
    public AsyncRelayCommand LoadSeparateCommand { get; }
    public AsyncRelayCommand PrintHistoryCommand { get; }
    public RelayCommand BackCommand { get; }

    public async Task LoadAsync(int patientId, CancellationToken cancellationToken = default)
    {
        _patientId = patientId;
        await LoadSingleAsync(cancellationToken);
    }

    private async Task LoadSingleAsync(CancellationToken cancellationToken)
    {
        if (_patientId <= 0)
        {
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;
        SelectedMode = 0;

        try
        {
            var result = await _mediator.Send(new GetPatientTestHistoryQuery(_patientId, FromFilter, ToFilter, TestIdFilter), cancellationToken);
            if (result.IsSuccess && result.Value is not null)
            {
                _singleHistory = result.Value;
                OnPropertyChanged(nameof(HistorySortMode));
                OnPropertyChanged(nameof(HistoryAutoDisplayEnabled));
                Entries = new ObservableCollection<HistoryEntryDto>(result.Value.Entries);
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

    private async Task LoadMultiAsync(CancellationToken cancellationToken)
    {
        if (MultiPatientIds.Count == 0)
        {
            ErrorMessage = "قائمة المرضى مطلوبة.";
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;
        SelectedMode = 1;

        try
        {
            var result = await _mediator.Send(new GetMultiPatientHistoryQuery(MultiPatientIds.ToList(), FromFilter, ToFilter, TestIdFilter), cancellationToken);
            if (result.IsSuccess && result.Value is not null)
            {
                _multiHistory = result.Value;
                OnPropertyChanged(nameof(HistorySortMode));
                OnPropertyChanged(nameof(HistoryAutoDisplayEnabled));
                Entries = new ObservableCollection<HistoryEntryDto>(result.Value.Entries);
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

    private async Task LoadSeparateAsync(CancellationToken cancellationToken)
    {
        if (_patientId <= 0)
        {
            ErrorMessage = "معرّف المريض غير صالح.";
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;
        SelectedMode = 2;

        try
        {
            var result = await _mediator.Send(new GetSeparateHistoryReportQuery(_patientId, FromFilter, ToFilter, TestIdFilter), cancellationToken);
            if (result.IsSuccess)
            {
                if (result.Value is null)
                {
                    // W-02 S12 (C-22): the old always-true type test silently kept
                    // stale entries on a null payload — clear and say so instead.
                    Entries.Clear();
                    ErrorMessage = "لا توجد بيانات تاريخية لهذا المريض.";
                }
                else
                {
                    Entries = new ObservableCollection<HistoryEntryDto>(result.Value.Entries);
                }
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

    private async Task PrintHistoryAsync(CancellationToken cancellationToken)
    {
        // W-02 S12 (WP-10): separate printing — one sheet per patient instead of
        // printing only the focused patient when several are loaded.
        IReadOnlyList<int> targets = PrintSeparately && IsMultiMode && MultiPatientIds.Count > 0
            ? MultiPatientIds.Distinct().ToList()
            : new[] { _patientId };

        if (targets.Any(id => id <= 0))
        {
            ErrorMessage = "معرّف المريض غير صالح.";
            return;
        }

        var confirmed = await _dialogs.ShowConfirmationAsync(
            "تأكيد طباعة التاريخ المرضي",
            "سيتم طباعة التقرير التاريخي وتعليم النتائج كمطبوعة. هل تريد المتابعة؟");
        if (!confirmed)
        {
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;

        try
        {
            foreach (var id in targets)
            {
                var result = await _mediator.Send(new PrintHistoryReportCommand(id), cancellationToken);
                if (!result.IsSuccess)
                {
                    if (result.Error is not null)
                    {
                        ErrorMessage = _presenter.Present(result.Error);
                    }

                    return;
                }
            }

            StatusMessage = "تمت الطباعة.";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
