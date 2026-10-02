using System.Collections.ObjectModel;
using MediatR;
using TopLab.Application.Features.CultureResults.Commands.SaveCultureResults;
using TopLab.Application.Features.CultureResults.Commands.UnverifyCultureResult;
using TopLab.Application.Features.CultureResults.Commands.VerifyCultureResult;
using TopLab.Application.Features.CultureResults.Common;
using TopLab.Application.Features.CultureResults.Queries.GetCultureEntryGrid;
using TopLab.Application.Features.CultureResults.Queries.GetCultureReport;
using TopLab.Application.Features.ResultsEntry.Common;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.Dialogs;
using TopLab.Presentation.Common.ErrorPresentation;

namespace TopLab.Presentation.ViewModels.Patients;

/// <summary>
/// Editable sensitivity row wrapper for the culture entry grid.
/// </summary>
public sealed class CultureSensitivityRow : ViewModelBase
{
    private int? _sensitivityCategory;

    public int AntibioticId { get; init; }
    public string AntibioticName { get; init; } = string.Empty;
    public bool IsPregnancyFlagged { get; init; }
    public bool IsChildrenFlagged { get; init; }

    public int? SensitivityCategory
    {
        get => _sensitivityCategory;
        set => SetProperty(ref _sensitivityCategory, value);
    }

    private decimal? _inhibitionZoneMm;
    private string? _inhibitionZoneText;

    public decimal? InhibitionZoneMm
    {
        get => _inhibitionZoneMm;
        set
        {
            if (SetProperty(ref _inhibitionZoneMm, value))
            {
                InhibitionZoneText = value?.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
        }
    }

    /// <summary>Zone text as typed; parsed with the invariant culture on save (VG-11).</summary>
    public string? InhibitionZoneText
    {
        get => _inhibitionZoneText;
        set => SetProperty(ref _inhibitionZoneText, value);
    }
}

/// <summary>
/// S-04 Slice 7: Culture entry grid (C1) for culture sensitivity results.
/// Routing: R1 → C1 for Culture/IsCultureType rows (settled D4).
/// Uses CultureResultsAccessPolicy (module-own policy — audit-verified).
/// Sensitivity rows come from CultureAntibioticAttachment + Antibiotic internally
/// (NOT GetCultureAntibioticsQuery — SD-7 grep gate).
/// </summary>
public sealed class CultureEntryViewModel : ViewModelBase
{
    private readonly ISender _mediator;
    private readonly ResultErrorPresenter _presenter;
    private readonly IDialogService _dialogs;

    // W-02 S5 (WP-06): the honest print path. The screen never marks a result itself.
    private readonly IResultPrintCoordinator _printCoordinator;
    private readonly IPrintedStateRecorder _printedState;

    private int _patientTestId;
    private string _testName = string.Empty;
    private string _testCode = string.Empty;

    /// <summary>WP-03 / SD-3: five English-labelled choices; Value null = Unspecified.</summary>
    public static IReadOnlyList<TopLab.Domain.Results.SensitivityOption> SensitivityOptions { get; } =
    [
        new(null, "Unspecified"),
        new((int)TopLab.Domain.Common.Enums.SensitivityCategory.HighlyFor, "Sensitive"),
        new((int)TopLab.Domain.Common.Enums.SensitivityCategory.ModerateFor, "Intermediate"),
        new((int)TopLab.Domain.Common.Enums.SensitivityCategory.LowFor, "Low Sensitivity"),
        new((int)TopLab.Domain.Common.Enums.SensitivityCategory.ResistantFor, "Resistant")
    ];
    private string? _sample;
    private string? _organismA;
    private string? _organismB;
    private string? _organismC;
    private string? _cultureCondition;
    private string? _colonyCount;
    private bool _parentIsReviewed;
    private bool _parentIsPrinted;
    private ObservableCollection<CultureSensitivityRow> _sensitivityRows = new();
    private CultureReportDto? _report;
    private bool _isBusy;
    private string _errorMessage = string.Empty;
    private string _statusMessage = string.Empty;
    private string? _pickedCommentPreview;

    public CultureEntryViewModel(
        ISender mediator,
        ResultErrorPresenter presenter,
        IDialogService dialogs,
        IResultPrintCoordinator printCoordinator,
        IPrintedStateRecorder printedState)
    {
        _mediator = mediator;
        _presenter = presenter;
        _dialogs = dialogs;
        _printCoordinator = printCoordinator;
        _printedState = printedState;

        SaveCommand = new AsyncRelayCommand(async (_, ct) => await SaveAsync(ct));
        VerifyCommand = new AsyncRelayCommand(async (_, ct) => await VerifyAsync(ct));
        UnverifyCommand = new AsyncRelayCommand(async (_, ct) => await UnverifyAsync(ct));
        PrintCommand = new AsyncRelayCommand(async (_, ct) => await PrintAsync(ct));
        ShowReportCommand = new AsyncRelayCommand(async (_, ct) => await ShowReportAsync(ct));
        OpenCommentPickerCommand = new AsyncRelayCommand(async (_, ct) => await OpenCommentPickerAsync(ct));
    }

    public int PatientTestId => _patientTestId;
    public string TestName { get => _testName; private set => SetProperty(ref _testName, value); }
    public string TestCode { get => _testCode; private set => SetProperty(ref _testCode, value); }

    public string? Sample { get => _sample; set => SetProperty(ref _sample, value); }
    public string? OrganismA { get => _organismA; set => SetProperty(ref _organismA, value); }
    public string? OrganismB { get => _organismB; set => SetProperty(ref _organismB, value); }
    public string? OrganismC { get => _organismC; set => SetProperty(ref _organismC, value); }
    public string? CultureCondition { get => _cultureCondition; set => SetProperty(ref _cultureCondition, value); }
    public string? ColonyCount { get => _colonyCount; set => SetProperty(ref _colonyCount, value); }

    private string? _pusCells;
    private string? _redBloodCells;
    private string? _epithelialCells;
    private string? _crystals;
    private string? _fungi;
    private string? _othersOne;
    private string? _othersTwo;
    private string? _othersThree;
    private bool _isDirectMicroscopy;

    public string? PusCells { get => _pusCells; set => SetProperty(ref _pusCells, value); }
    public string? RedBloodCells { get => _redBloodCells; set => SetProperty(ref _redBloodCells, value); }
    public string? EpithelialCells { get => _epithelialCells; set => SetProperty(ref _epithelialCells, value); }
    public string? Crystals { get => _crystals; set => SetProperty(ref _crystals, value); }
    public string? Fungi { get => _fungi; set => SetProperty(ref _fungi, value); }
    public string? OthersOne { get => _othersOne; set => SetProperty(ref _othersOne, value); }
    public string? OthersTwo { get => _othersTwo; set => SetProperty(ref _othersTwo, value); }
    public string? OthersThree { get => _othersThree; set => SetProperty(ref _othersThree, value); }
    public bool IsDirectMicroscopy { get => _isDirectMicroscopy; set => SetProperty(ref _isDirectMicroscopy, value); }

    public bool ParentIsReviewed { get => _parentIsReviewed; private set => SetProperty(ref _parentIsReviewed, value); }
    public bool ParentIsPrinted { get => _parentIsPrinted; private set => SetProperty(ref _parentIsPrinted, value); }

    public bool IsLocked => ParentIsReviewed || ParentIsPrinted;

    public ObservableCollection<CultureSensitivityRow> SensitivityRows
    {
        get => _sensitivityRows;
        private set
        {
            if (SetProperty(ref _sensitivityRows, value))
            {
                OnPropertyChanged(nameof(HasRows));
                OnPropertyChanged(nameof(ShowEmpty));
            }
        }
    }

    public bool HasRows => SensitivityRows.Count > 0;
    public bool ShowEmpty => SensitivityRows.Count == 0 && _patientTestId > 0 && !IsBusy;

    public CultureReportDto? Report
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
    public AsyncRelayCommand OpenCommentPickerCommand { get; }

    /// <summary>W-02 S8 (WP-13): picked standard comment, preview only — never saved.</summary>
    public string? PickedCommentPreview
    {
        get => _pickedCommentPreview;
        private set
        {
            if (SetProperty(ref _pickedCommentPreview, value))
            {
                OnPropertyChanged(nameof(HasPickedComment));
            }
        }
    }

    public bool HasPickedComment => !string.IsNullOrWhiteSpace(_pickedCommentPreview);

    public async Task LoadAsync(int patientTestId, CancellationToken cancellationToken = default)
    {
        _patientTestId = patientTestId;
        IsBusy = true;
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;
        Report = null;

        try
        {
            var result = await _mediator.Send(new GetCultureEntryGridQuery(patientTestId), cancellationToken);
            if (result.IsSuccess && result.Value is not null)
            {
                var grid = result.Value;
                TestName = grid.TestName;
                TestCode = grid.TestCode;
                Sample = grid.Sample;
                OrganismA = grid.OrganismA;
                OrganismB = grid.OrganismB;
                OrganismC = grid.OrganismC;
                CultureCondition = grid.CultureCondition;
                ColonyCount = grid.ColonyCount;
                PusCells = grid.Microscopy?.PusCells;
                RedBloodCells = grid.Microscopy?.RedBloodCells;
                EpithelialCells = grid.Microscopy?.EpithelialCells;
                Crystals = grid.Microscopy?.Crystals;
                Fungi = grid.Microscopy?.Fungi;
                OthersOne = grid.Microscopy?.OthersOne;
                OthersTwo = grid.Microscopy?.OthersTwo;
                OthersThree = grid.Microscopy?.OthersThree;
                IsDirectMicroscopy = grid.Microscopy?.IsDirect ?? false;
                ParentIsReviewed = grid.ParentIsReviewed;
                ParentIsPrinted = grid.ParentIsPrinted;
                OnPropertyChanged(nameof(IsLocked));

                var rows = new ObservableCollection<CultureSensitivityRow>();
                foreach (var item in grid.Rows)
                {
                    rows.Add(new CultureSensitivityRow
                    {
                        AntibioticId = item.AntibioticId,
                        AntibioticName = item.AntibioticName,
                        SensitivityCategory = item.SensitivityCategory,
                        IsPregnancyFlagged = item.IsPregnancyFlagged,
                        IsChildrenFlagged = item.IsChildrenFlagged
                    });
                    rows[^1].InhibitionZoneMm = item.InhibitionZoneMm;
                }

                SensitivityRows = rows;
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

    /// <summary>W-02 S8 (WP-13): standard test comments — picking never writes to the database.</summary>
    private async Task OpenCommentPickerAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(TestCode))
        {
            ErrorMessage = "احفظ المزرعة أولاً قبل اختيار تعليق.";
            return;
        }

        ErrorMessage = string.Empty;

        var vm = new TestCommentPickerViewModel(_mediator, _presenter);
        await vm.LoadByTestCodeAsync(TestCode, cancellationToken);
        var window = new Views.Patients.TestCommentPickerWindow(vm)
        {
            Owner = System.Windows.Application.Current?.MainWindow
        };

        bool? picked = window.ShowDialog();
        if (picked == true)
        {
            PickedCommentPreview = vm.PickedCommentText;
        }
    }

    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        if (_patientTestId == 0 || IsLocked)
        {
            return;
        }

        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;

        var sensitivities = new List<CultureSensitivityInput>();
        foreach (var r in SensitivityRows)
        {
            decimal? zone = null;
            if (!string.IsNullOrWhiteSpace(r.InhibitionZoneText))
            {
                if (!decimal.TryParse(
                        r.InhibitionZoneText.Trim(),
                        System.Globalization.NumberStyles.Number,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out var parsed))
                {
                    ErrorMessage = "منطقة التثبيط يجب أن تكون رقماً.";
                    return;
                }

                zone = parsed;
            }

            sensitivities.Add(new CultureSensitivityInput(r.AntibioticId, r.SensitivityCategory, zone));
        }

        IsBusy = true;
        try
        {
            var result = await _mediator.Send(new SaveCultureResultsCommand(
                _patientTestId,
                string.IsNullOrWhiteSpace(Sample) ? null : Sample,
                string.IsNullOrWhiteSpace(OrganismA) ? null : OrganismA,
                string.IsNullOrWhiteSpace(OrganismB) ? null : OrganismB,
                string.IsNullOrWhiteSpace(OrganismC) ? null : OrganismC,
                string.IsNullOrWhiteSpace(CultureCondition) ? null : CultureCondition,
                string.IsNullOrWhiteSpace(ColonyCount) ? null : ColonyCount,
                sensitivities,
                new CultureMicroscopyInput(
                    BlankOrNull(PusCells), BlankOrNull(RedBloodCells), BlankOrNull(EpithelialCells),
                    BlankOrNull(Crystals), BlankOrNull(Fungi), BlankOrNull(OthersOne),
                    BlankOrNull(OthersTwo), BlankOrNull(OthersThree), IsDirectMicroscopy)), cancellationToken);

            if (result.IsSuccess)
            {
                StatusMessage = "تم حفظ نتيجة المزرعة.";
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

    private static string? BlankOrNull(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private async Task VerifyAsync(CancellationToken cancellationToken)
    {
        if (_patientTestId == 0)
        {
            return;
        }

        var confirmed = await _dialogs.ShowConfirmationAsync(
            "تأكيد الاعتماد",
            "هل تريد اعتماد نتيجة هذه المزرعة؟");
        if (!confirmed)
        {
            return;
        }

        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;
        IsBusy = true;
        try
        {
            var result = await _mediator.Send(new VerifyCultureResultCommand(_patientTestId), cancellationToken);
            if (result.IsSuccess)
            {
                StatusMessage = "تم اعتماد نتيجة المزرعة.";
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
            "هل تريد إلغاء اعتماد نتيجة هذه المزرعة؟");
        if (!confirmed)
        {
            return;
        }

        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;
        IsBusy = true;
        try
        {
            var result = await _mediator.Send(new UnverifyCultureResultCommand(_patientTestId), cancellationToken);
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
            // W-02 S5: build, then print. Never marks (SD-1).
            var outcome = await _printCoordinator.PrintAsync(
                _patientTestId, ResultPrintKind.CultureReport, cancellationToken);

            if (outcome.Printed)
            {
                // W-02 post-implementation fix (owner decision 1: printed = successful
                // printing). Records PatientTest.IsPrinted after a successful culture report
                // print. A culture test carries no profile result items, so
                // RecordForPatientTestAsync resolves none and only the test row is recorded.
                var recorded = await _printedState.RecordForPatientTestAsync(
                    _patientTestId, cancellationToken);
                if (!recorded.IsSuccess)
                {
                    ErrorMessage = recorded.Error!.Message;
                    return;
                }

                StatusMessage = "تمت الطباعة.";
                await LoadAsync(_patientTestId, cancellationToken);
            }
            else
            {
                ErrorMessage = "تعذّرت الطباعة: " + outcome.ErrorMessage;
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
            var result = await _mediator.Send(new GetCultureReportQuery(_patientTestId), cancellationToken);
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
}
