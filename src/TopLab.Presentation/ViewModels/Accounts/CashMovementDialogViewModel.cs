using System.Collections.ObjectModel;
using MediatR;
using TopLab.Application.Features.ExternalEntities.Queries.SearchExternalEntities;
using TopLab.Application.Features.InventoryAndAccounting.Commands.RecordCashDeposit;
using TopLab.Application.Features.InventoryAndAccounting.Commands.RecordCashDisbursement;
using TopLab.Domain.Common.Enums;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.Dialogs;
using TopLab.Presentation.Common.ErrorPresentation;

namespace TopLab.Presentation.ViewModels.Accounts;

/// <summary>
/// S-06 Slice 3: Cash movement dialog (M20) — deposit or disbursement.
/// Direction determined by the button pressed; no direction field in the UI.
/// </summary>
public sealed class CashMovementDialogViewModel : ViewModelBase
{
    private readonly ISender _mediator;
    private readonly ResultErrorPresenter _presenter;
    private readonly IDialogService _dialogs;

    private bool _isDeposit = true;
    private string _amountText = string.Empty;
    private int? _relatedExternalEntityId;
    private string? _notes;
    private ObservableCollection<LabFilterItem> _entityItems = new();
    private bool _isBusy;
    private string _errorMessage = string.Empty;

    public CashMovementDialogViewModel(ISender mediator, ResultErrorPresenter presenter, IDialogService dialogs)
    {
        _mediator = mediator;
        _presenter = presenter;
        _dialogs = dialogs;
    }

    public bool IsDeposit { get => _isDeposit; private set => SetProperty(ref _isDeposit, value); }

    public string DirectionLabel => IsDeposit ? "إيداع" : "صرف";

    public string AmountText { get => _amountText; set => SetProperty(ref _amountText, value); }

    public int? RelatedExternalEntityId
    {
        get => _relatedExternalEntityId;
        set => SetProperty(ref _relatedExternalEntityId, value);
    }

    public string? Notes { get => _notes; set => SetProperty(ref _notes, value); }

    public ObservableCollection<LabFilterItem> EntityItems
    {
        get => _entityItems;
        private set => SetProperty(ref _entityItems, value);
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

    public async Task SetupAsync(bool isDeposit, CancellationToken cancellationToken = default)
    {
        IsDeposit = isDeposit;
        OnPropertyChanged(nameof(DirectionLabel));
        AmountText = string.Empty;
        RelatedExternalEntityId = null;
        Notes = null;
        ErrorMessage = string.Empty;
        IsBusy = true;

        try
        {
            var result = await _mediator.Send(
                new SearchExternalEntitiesQuery(null, null, 1, 100), cancellationToken);
            if (result.IsSuccess && result.Value is not null)
            {
                var items = new ObservableCollection<LabFilterItem> { new(null, "بدون جهة") };
                foreach (var entity in result.Value)
                {
                    items.Add(new LabFilterItem(entity.Id, entity.Name));
                }

                EntityItems = items;
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task<bool> SaveAsync()
    {
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(AmountText)
            || !decimal.TryParse(AmountText, out var amount)
            || amount <= 0)
        {
            ErrorMessage = "المبلغ يجب أن يكون أكبر من صفر.";
            return false;
        }

        var confirmed = await _dialogs.ShowConfirmationAsync(
            $"تأكيد {DirectionLabel}",
            $"سيتم تسجيل {DirectionLabel} بمبلغ {amount}. هل تريد المتابعة؟");
        if (!confirmed)
        {
            return false;
        }

        IsBusy = true;
        try
        {
            var notes = string.IsNullOrWhiteSpace(Notes) ? null : Notes.Trim();
            var entityId = RelatedExternalEntityId == 0 ? null : RelatedExternalEntityId;

            var result = IsDeposit
                ? await _mediator.Send(new RecordCashDepositCommand(amount, entityId, notes))
                : await _mediator.Send(new RecordCashDisbursementCommand(amount, entityId, notes));

            if (result.IsSuccess)
            {
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
}
