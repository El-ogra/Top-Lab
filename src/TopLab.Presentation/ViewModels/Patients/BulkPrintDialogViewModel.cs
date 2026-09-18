using System.Collections.ObjectModel;
using MediatR;
using TopLab.Application.Features.ResultsEntry.Commands.BulkPrint;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.Dialogs;
using TopLab.Presentation.Common.ErrorPresentation;

namespace TopLab.Presentation.ViewModels.Patients;

/// <summary>
/// S-04 Slice 5: Bulk print dialog (R5) — preflight + execute for selected patient IDs.
/// R5 source: BulkPrintPreflightQuery → per-patient RequiresReprintConfirmation/VerifiedCount/TotalCount.
/// Execute: ExecuteBulkPrintCommand with BulkPrintDecision.ConfirmReprint per patient.
/// Frozen message: ReprintConfirmationMessage verbatim for reprint confirmation.
/// </summary>
public sealed class BulkPrintDialogViewModel : ViewModelBase
{
    private readonly ISender _mediator;
    private readonly IDialogService _dialogs;
    private readonly ResultErrorPresenter _presenter;

    private ObservableCollection<BulkPrintPreflightDto> _preflightItems = new();
    private ObservableCollection<BulkPrintOutcomeDto> _outcomes = new();
    private bool _isBusy;
    private bool _isExecuted;
    private string _errorMessage = string.Empty;
    private string _statusMessage = string.Empty;

    public BulkPrintDialogViewModel(ISender mediator, IDialogService dialogs, ResultErrorPresenter presenter)
    {
        _mediator = mediator;
        _dialogs = dialogs;
        _presenter = presenter;

        ExecuteCommand = new AsyncRelayCommand(async (_, ct) => await ExecuteAsync(ct));
    }

    public ObservableCollection<BulkPrintPreflightDto> PreflightItems
    {
        get => _preflightItems;
        private set
        {
            if (SetProperty(ref _preflightItems, value))
            {
                OnPropertyChanged(nameof(HasPreflight));
            }
        }
    }

    public ObservableCollection<BulkPrintOutcomeDto> Outcomes
    {
        get => _outcomes;
        private set
        {
            if (SetProperty(ref _outcomes, value))
            {
                OnPropertyChanged(nameof(HasOutcomes));
            }
        }
    }

    public bool HasPreflight => PreflightItems.Count > 0;
    public bool HasOutcomes => Outcomes.Count > 0;

    public bool IsBusy
    {
        get => _isBusy;
        private set => SetProperty(ref _isBusy, value);
    }

    public bool IsExecuted
    {
        get => _isExecuted;
        private set => SetProperty(ref _isExecuted, value);
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

    public AsyncRelayCommand ExecuteCommand { get; }

    /// <summary>Called by the host window after construction to load preflight data.</summary>
    public async Task LoadPreflightAsync(IReadOnlyList<int> patientIds, CancellationToken cancellationToken = default)
    {
        if (patientIds.Count == 0)
        {
            ErrorMessage = "قائمة المرضى مطلوبة.";
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;
        IsExecuted = false;
        Outcomes = new ObservableCollection<BulkPrintOutcomeDto>();

        try
        {
            var result = await _mediator.Send(new BulkPrintPreflightQuery(patientIds), cancellationToken);
            if (result.IsSuccess && result.Value is not null)
            {
                PreflightItems = new ObservableCollection<BulkPrintPreflightDto>(result.Value);
                if (PreflightItems.Count == 0)
                {
                    ErrorMessage = "لا يوجد مرضى مؤهلون للطباعة.";
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

    private async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        if (PreflightItems.Count == 0)
        {
            ErrorMessage = "قائمة المرضى مطلوبة.";
            return;
        }

        var decisions = new List<BulkPrintDecision>();

        foreach (var item in PreflightItems)
        {
            if (item.RequiresReprintConfirmation)
            {
                var confirmed = await _dialogs.ShowConfirmationAsync(
                    "تأكيد الطباعة",
                    BulkPrintMessages.ReprintConfirmationMessage);

                decisions.Add(new BulkPrintDecision(item.PatientId, confirmed));
            }
            else
            {
                decisions.Add(new BulkPrintDecision(item.PatientId, true));
            }
        }

        if (decisions.Count == 0)
        {
            ErrorMessage = "قائمة القرارات مطلوبة.";
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            var result = await _mediator.Send(new ExecuteBulkPrintCommand(decisions), cancellationToken);
            if (result.IsSuccess && result.Value is not null)
            {
                Outcomes = new ObservableCollection<BulkPrintOutcomeDto>(result.Value);
                IsExecuted = true;
                StatusMessage = $"تم تنفيذ {Outcomes.Count} قرار طباعة.";
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
