using System.Collections.ObjectModel;
using System.Globalization;
using MediatR;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Common;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Queries.GetTestGroups;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Queries.GetWorkGroupLogs;
using TopLab.Application.Features.WorkSheets.Commands.PrintWorkSheet;
using TopLab.Application.Features.WorkSheets.Common;
using TopLab.Application.Features.WorkSheets.Queries.GetVisitWorkSheet;
using TopLab.Application.Features.WorkSheets.Queries.GetWorkSheetByTestGroup;
using TopLab.Application.Features.WorkSheets.Queries.GetWorkSheetByWorkGroupLog;
using TopLab.Application.Features.WorkSheets.Queries.GetWorkSheetSummary;
using TopLab.Application.Features.WorkSheets.Queries.GetWorkSheetTestCountByPeriod;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.ErrorPresentation;

namespace TopLab.Presentation.ViewModels.WorkSheets;

/// <summary>
/// S-03 Slice 7: WorkSheets screen (P2 S-WS-1/S-WS-2) + «ورقة العمل» shell debt.
/// Three exclusive modes: visit by integer PatientId (no LabId entry —
/// owner-pending), test group, work-group log; shared From/To period (UTC-today
/// default, backend resolves nulls and validates the range). Visit mode prints
/// via the existing PrintWorkSheetCommand; group/log print ships disabled
/// (owner-pending — no print command exists, none is created).
/// </summary>
public sealed class WorkSheetsViewModel : ViewModelBase
{
    private readonly ISender _mediator;
    private readonly ResultErrorPresenter _presenter;

    private bool _isVisitMode = true;
    private bool _isTestGroupMode;
    private bool _isWorkGroupLogMode;
    private string _visitPatientIdText = string.Empty;
    private TestGroupDto? _selectedTestGroup;
    private WorkGroupLogDto? _selectedWorkGroupLog;
    private DateTime? _fromDate;
    private DateTime? _toDate;
    private int _totalTests;
    private string _modeText = string.Empty;
    private bool _sheetLoaded;
    private int _totalCount;
    private bool _isBusy;
    private string _errorMessage = string.Empty;
    private string _statusMessage = string.Empty;

    public WorkSheetsViewModel(ISender mediator, ResultErrorPresenter presenter)
    {
        _mediator = mediator;
        _presenter = presenter;

        RunCommand = new AsyncRelayCommand(async (_, ct) => await RunAsync(ct));
        PrintVisitSheetCommand = new AsyncRelayCommand(async (_, ct) => await PrintVisitSheetAsync(ct));

        Sections.CollectionChanged += (_, _) => OnPropertyChanged(nameof(ShowSheetEmpty));
        SummaryRows.CollectionChanged += (_, _) => OnPropertyChanged(nameof(ShowSummaryEmpty));
        CountRows.CollectionChanged += (_, _) => OnPropertyChanged(nameof(ShowCountEmpty));

        var today = DateTime.UtcNow.Date;
        _fromDate = today;
        _toDate = today;
    }

    public ObservableCollection<WorkSheetSectionDto> Sections { get; } = new();

    public ObservableCollection<WorkSheetSummaryRowDto> SummaryRows { get; } = new();

    public ObservableCollection<WorkSheetTestCountRowDto> CountRows { get; } = new();

    public ObservableCollection<TestGroupDto> TestGroups { get; } = new();

    public ObservableCollection<WorkGroupLogDto> WorkGroupLogs { get; } = new();

    public bool IsVisitMode
    {
        get => _isVisitMode;
        set
        {
            if (SetProperty(ref _isVisitMode, value) && value)
            {
                IsTestGroupMode = false;
                IsWorkGroupLogMode = false;
            }
        }
    }

    public bool IsTestGroupMode
    {
        get => _isTestGroupMode;
        set
        {
            if (SetProperty(ref _isTestGroupMode, value) && value)
            {
                IsVisitMode = false;
                IsWorkGroupLogMode = false;
            }
        }
    }

    public bool IsWorkGroupLogMode
    {
        get => _isWorkGroupLogMode;
        set
        {
            if (SetProperty(ref _isWorkGroupLogMode, value) && value)
            {
                IsVisitMode = false;
                IsTestGroupMode = false;
            }
        }
    }

    /// <summary>Visit mode accepts an integer PatientId only (no LabId — owner-pending).</summary>
    public string VisitPatientIdText { get => _visitPatientIdText; set => SetProperty(ref _visitPatientIdText, value); }

    public TestGroupDto? SelectedTestGroup { get => _selectedTestGroup; set => SetProperty(ref _selectedTestGroup, value); }

    public WorkGroupLogDto? SelectedWorkGroupLog { get => _selectedWorkGroupLog; set => SetProperty(ref _selectedWorkGroupLog, value); }

    public DateTime? FromDate { get => _fromDate; set => SetProperty(ref _fromDate, value); }

    public DateTime? ToDate { get => _toDate; set => SetProperty(ref _toDate, value); }

    public int TotalTests { get => _totalTests; private set => SetProperty(ref _totalTests, value); }

    public string ModeText { get => _modeText; private set => SetProperty(ref _modeText, value); }

    public bool SheetLoaded { get => _sheetLoaded; private set => SetProperty(ref _sheetLoaded, value); }

    public int TotalCount { get => _totalCount; private set => SetProperty(ref _totalCount, value); }

    /// <summary>S-03 Slice 7: empty-state flags (S-01/S-02 Show*Empty idiom).</summary>
    public bool ShowSheetEmpty => SheetLoaded && Sections.Count == 0;

    public bool ShowSummaryEmpty => SheetLoaded && SummaryRows.Count == 0;

    public bool ShowCountEmpty => SheetLoaded && CountRows.Count == 0;

    public bool IsBusy { get => _isBusy; private set => SetProperty(ref _isBusy, value); }

    public string ErrorMessage { get => _errorMessage; private set => SetProperty(ref _errorMessage, value); }

    public string StatusMessage { get => _statusMessage; private set => SetProperty(ref _statusMessage, value); }

    public AsyncRelayCommand RunCommand { get; }

    public AsyncRelayCommand PrintVisitSheetCommand { get; }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            var groups = await _mediator.Send(new GetTestGroupsQuery(), cancellationToken);
            if (!groups.IsSuccess)
            {
                ErrorMessage = _presenter.Present(groups.Error!);
                return;
            }

            TestGroups.Clear();
            foreach (var group in groups.Value!)
            {
                TestGroups.Add(group);
            }

            var logs = await _mediator.Send(new GetWorkGroupLogsQuery(), cancellationToken);
            if (!logs.IsSuccess)
            {
                ErrorMessage = _presenter.Present(logs.Error!);
                return;
            }

            WorkGroupLogs.Clear();
            foreach (var log in logs.Value!)
            {
                WorkGroupLogs.Add(log);
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task RunAsync(CancellationToken cancellationToken)
    {
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;

        IsBusy = true;
        try
        {
            if (IsVisitMode)
            {
                if (!int.TryParse(VisitPatientIdText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var patientId) || patientId <= 0)
                {
                    ErrorMessage = "معرّف المريض غير صالح.";
                    return;
                }

                var visit = await _mediator.Send(new GetVisitWorkSheetQuery(patientId), cancellationToken);
                if (!visit.IsSuccess)
                {
                    ErrorMessage = _presenter.Present(visit.Error!);
                    return;
                }

                FillSections(visit.Value!.Sections, visit.Value.TotalTests, "زيارة");
            }
            else if (IsTestGroupMode)
            {
                if (SelectedTestGroup is null)
                {
                    ErrorMessage = "اختر مجموعة تحاليل أولًا.";
                    return;
                }

                var sheet = await _mediator.Send(
                    new GetWorkSheetByTestGroupQuery(SelectedTestGroup.Id, null, ToDateOnly(FromDate), ToDateOnly(ToDate)),
                    cancellationToken);
                if (!sheet.IsSuccess)
                {
                    ErrorMessage = _presenter.Present(sheet.Error!);
                    return;
                }

                FillSections(sheet.Value!.Sections, sheet.Value.TotalTests, sheet.Value.Mode);
            }
            else
            {
                if (SelectedWorkGroupLog is null)
                {
                    ErrorMessage = "اختر سجل مجموعة عمل أولًا.";
                    return;
                }

                var sheet = await _mediator.Send(
                    new GetWorkSheetByWorkGroupLogQuery(SelectedWorkGroupLog.Id, ToDateOnly(FromDate), ToDateOnly(ToDate)),
                    cancellationToken);
                if (!sheet.IsSuccess)
                {
                    ErrorMessage = _presenter.Present(sheet.Error!);
                    return;
                }

                FillSections(sheet.Value!.Sections, sheet.Value.TotalTests, sheet.Value.Mode);
            }

            await LoadSummaryAndCountAsync(cancellationToken);
            SheetLoaded = true;
            OnPropertyChanged(nameof(ShowSheetEmpty));
            OnPropertyChanged(nameof(ShowSummaryEmpty));
            OnPropertyChanged(nameof(ShowCountEmpty));
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void FillSections(IReadOnlyList<WorkSheetSectionDto> sections, int totalTests, string mode)
    {
        Sections.Clear();
        foreach (var section in sections)
        {
            Sections.Add(section);
        }

        TotalTests = totalTests;
        ModeText = mode;
    }

    /// <summary>S-WS-2: summary + period-count grids share the run's From/To.</summary>
    private async Task LoadSummaryAndCountAsync(CancellationToken cancellationToken)
    {
        var summary = await _mediator.Send(
            new GetWorkSheetSummaryQuery(ToDateOnly(FromDate), ToDateOnly(ToDate)),
            cancellationToken);
        if (!summary.IsSuccess)
        {
            ErrorMessage = _presenter.Present(summary.Error!);
            return;
        }

        SummaryRows.Clear();
        foreach (var row in summary.Value!)
        {
            SummaryRows.Add(row);
        }

        var count = await _mediator.Send(
            new GetWorkSheetTestCountByPeriodQuery(ToDateOnly(FromDate), ToDateOnly(ToDate)),
            cancellationToken);
        if (!count.IsSuccess)
        {
            ErrorMessage = _presenter.Present(count.Error!);
            return;
        }

        CountRows.Clear();
        foreach (var row in count.Value!.Rows)
        {
            CountRows.Add(row);
        }

        TotalCount = count.Value.TotalCount;
    }

    /// <summary>Visit-mode print via the existing command (group/log print ships disabled).</summary>
    private async Task PrintVisitSheetAsync(CancellationToken cancellationToken)
    {
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;

        if (!int.TryParse(VisitPatientIdText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var patientId) || patientId <= 0)
        {
            ErrorMessage = "معرّف المريض غير صالح.";
            return;
        }

        IsBusy = true;
        try
        {
            var result = await _mediator.Send(new PrintWorkSheetCommand(patientId), cancellationToken);
            if (!result.IsSuccess)
            {
                ErrorMessage = _presenter.Present(result.Error!);
                return;
            }

            StatusMessage = "تم إرسال المستند إلى الطابعة.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static DateOnly? ToDateOnly(DateTime? value)
    {
        return value.HasValue ? DateOnly.FromDateTime(value.Value) : null;
    }
}
