using System.Collections.ObjectModel;
using MediatR;
using TopLab.Application.Features.ResultDelivery.Commands.DeliverWithSettlement;
using TopLab.Application.Features.ResultDelivery.Common;
using TopLab.Application.Features.ResultDelivery.Queries.GetDeliveryAccount;
using TopLab.Application.Features.ResultDelivery.Queries.GetDeliveryGrid;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.Dialogs;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.Common.Navigation;

namespace TopLab.Presentation.ViewModels.Patients;

/// <summary>
/// Selectable row wrapper for delivery grid lines.
/// CheckBox enabled only when IsPrinted && !IsDelivered.
/// </summary>
public sealed class DeliveryGridRow : ViewModelBase
{
    private bool _isSelected;

    public int PatientTestId { get; init; }
    public string TestName { get; init; } = string.Empty;
    public string TestCode { get; init; } = string.Empty;
    public string? ResultValue { get; init; }
    public int? Flag { get; init; }
    public int Status { get; init; }
    public bool IsEntered { get; init; }
    public bool IsReviewed { get; init; }
    public bool IsPrinted { get; init; }
    public bool IsDelivered { get; init; }
    public decimal Price { get; init; }

    public bool CanDeliver => IsPrinted && !IsDelivered;

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
}

/// <summary>
/// S-05 Slice 4: Delivery handover screen (M09) — account panel + delivery grid + atomic deliver-with-settlement.
/// </summary>
public sealed class DeliveryHandoverViewModel : ViewModelBase
{
    private readonly ISender _mediator;
    private readonly ResultErrorPresenter _presenter;
    private readonly IDialogService _dialogs;
    private readonly INavigationService _navigation;

    private int _patientId;
    private DeliveryAccountDto? _account;
    private ObservableCollection<DeliveryGridRow> _gridRows = new();
    private string _settleAmountText = string.Empty;
    private bool _settleInFull;
    private bool _isBusy;
    private string _errorMessage = string.Empty;
    private string _statusMessage = string.Empty;

    public DeliveryHandoverViewModel(
        ISender mediator,
        ResultErrorPresenter presenter,
        IDialogService dialogs,
        INavigationService navigation)
    {
        _mediator = mediator;
        _presenter = presenter;
        _dialogs = dialogs;
        _navigation = navigation;

        DeliverCommand = new AsyncRelayCommand(async (_, ct) => await DeliverAsync(ct));
        BackCommand = new RelayCommand(_ => _navigation.NavigateTo<ResultDeliveryViewModel>());
    }

    public int PatientId => _patientId;

    public DeliveryAccountDto? Account
    {
        get => _account;
        private set
        {
            if (SetProperty(ref _account, value))
            {
                OnPropertyChanged(nameof(HasAccount));
            }
        }
    }

    public bool HasAccount => _account is not null;

    public ObservableCollection<DeliveryGridRow> GridRows
    {
        get => _gridRows;
        private set
        {
            if (SetProperty(ref _gridRows, value))
            {
                OnPropertyChanged(nameof(HasRows));
                OnPropertyChanged(nameof(ShowEmpty));
                OnPropertyChanged(nameof(SelectedCount));
            }
        }
    }

    public bool HasRows => GridRows.Count > 0;
    public bool ShowEmpty => GridRows.Count == 0 && !IsBusy && string.IsNullOrEmpty(ErrorMessage);
    public int SelectedCount => GridRows.Count(r => r.IsSelected && r.CanDeliver);

    public string SettleAmountText
    {
        get => _settleAmountText;
        set => SetProperty(ref _settleAmountText, value);
    }

    public bool SettleInFull
    {
        get => _settleInFull;
        set => SetProperty(ref _settleInFull, value);
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

    public AsyncRelayCommand DeliverCommand { get; }
    public RelayCommand BackCommand { get; }

    public async Task LoadAsync(int patientId, CancellationToken cancellationToken = default)
    {
        _patientId = patientId;
        IsBusy = true;
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;
        SettleAmountText = string.Empty;
        SettleInFull = false;

        try
        {
            var accountResult = await _mediator.Send(new GetDeliveryAccountQuery(patientId), cancellationToken);
            if (accountResult.IsSuccess && accountResult.Value is not null)
            {
                Account = accountResult.Value;
            }
            else if (accountResult.Error is not null)
            {
                ErrorMessage = _presenter.Present(accountResult.Error);
            }

            var gridResult = await _mediator.Send(new GetDeliveryGridQuery(patientId), cancellationToken);
            if (gridResult.IsSuccess && gridResult.Value is not null)
            {
                var rows = new ObservableCollection<DeliveryGridRow>();
                foreach (var item in gridResult.Value)
                {
                    rows.Add(new DeliveryGridRow
                    {
                        PatientTestId = item.PatientTestId,
                        TestName = item.TestName,
                        TestCode = item.TestCode,
                        ResultValue = item.ResultValue,
                        Flag = item.Flag,
                        Status = item.Status,
                        IsEntered = item.IsEntered,
                        IsReviewed = item.IsReviewed,
                        IsPrinted = item.IsPrinted,
                        IsDelivered = item.IsDelivered,
                        Price = item.Price
                    });
                }

                GridRows = rows;
            }
            else if (gridResult.Error is not null)
            {
                ErrorMessage = _presenter.Present(gridResult.Error);
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task DeliverAsync(CancellationToken cancellationToken)
    {
        var selectedIds = GridRows.Where(r => r.IsSelected && r.CanDeliver).Select(r => r.PatientTestId).ToList();
        if (selectedIds.Count == 0)
        {
            ErrorMessage = "حدد نتيجة واحدة على الأقل للتسليم.";
            return;
        }

        decimal? settleAmount = null;
        if (!string.IsNullOrWhiteSpace(SettleAmountText))
        {
            if (!decimal.TryParse(SettleAmountText, out var parsed) || parsed <= 0)
            {
                ErrorMessage = "مبلغ التسوية غير صالح.";
                return;
            }

            settleAmount = parsed;
        }

        if (SettleInFull && settleAmount.HasValue && settleAmount.Value > 0)
        {
            ErrorMessage = "مبلغ التسوية غير صالح.";
            return;
        }

        // Build confirmation message
        var settlementDesc = SettleInFull
            ? "تسوية كاملة (خلاص)"
            : settleAmount.HasValue
                ? $"دفعة جزئية: {settleAmount.Value}"
                : "بدون تسوية";

        var confirmed = await _dialogs.ShowConfirmationAsync(
            "تأكيد التسليم والتسوية",
            $"سيتم تسليم {selectedIds.Count} نتيجة مع {settlementDesc}. هل تريد المتابعة؟");
        if (!confirmed)
        {
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;

        try
        {
            var result = await _mediator.Send(
                new DeliverWithSettlementCommand(_patientId, selectedIds, settleAmount, SettleInFull),
                cancellationToken);

            if (result.IsSuccess)
            {
                StatusMessage = "تم التسليم والتسوية بنجاح.";
                // Refresh both queries
                await LoadAsync(_patientId, cancellationToken);
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
