using System.Collections.ObjectModel;
using MediatR;
using TopLab.Application.Features.CultureResults.Commands.MarkCultureReportPrinted;
using TopLab.Application.Features.CultureResults.Commands.SaveCultureResults;
using TopLab.Application.Features.CultureResults.Commands.UnverifyCultureResult;
using TopLab.Application.Features.CultureResults.Commands.VerifyCultureResult;
using TopLab.Application.Features.CultureResults.Common;
using TopLab.Application.Features.CultureResults.Queries.GetCultureEntryGrid;
using TopLab.Application.Features.CultureResults.Queries.GetCultureReport;
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

    private int _patientTestId;
    private string _testName = string.Empty;
    private string _testCode = string.Empty;
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

    public CultureEntryViewModel(
        ISender mediator,
        ResultErrorPresenter presenter,
        IDialogService dialogs)
    {
        _mediator = mediator;
        _presenter = presenter;
        _dialogs = dialogs;

        SaveCommand = new AsyncRelayCommand(async (_, ct) => await SaveAsync(ct));
        VerifyCommand = new AsyncRelayCommand(async (_, ct) => await VerifyAsync(ct));
        UnverifyCommand = new AsyncRelayCommand(async (_, ct) => await UnverifyAsync(ct));
        PrintCommand = new AsyncRelayCommand(async (_, ct) => await PrintAsync(ct));
        ShowReportCommand = new AsyncRelayCommand(async (_, ct) => await ShowReportAsync(ct));
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

    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        if (_patientTestId == 0 || IsLocked)
        {
            return;
        }

        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;

        var sensitivities = SensitivityRows
            .Where(r => r.SensitivityCategory.HasValue)
            .Select(r => new CultureSensitivityInput(r.AntibioticId, r.SensitivityCategory!.Value))
            .ToList();

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
                sensitivities), cancellationToken);

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
            var result = await _mediator.Send(new MarkCultureReportPrintedCommand(_patientTestId), cancellationToken);
            if (result.IsSuccess)
            {
                StatusMessage = "تمت الطباعة.";
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
