using MediatR;
using TopLab.Application.Features.SentOutSamples.Commands.RecordSentOutPayment;
using TopLab.Application.Features.SentOutSamples.Commands.SettleSentOutInFull;
using TopLab.Application.Features.SentOutSamples.Common;
using TopLab.Application.Features.SentOutSamples.Queries.GetSentOutLabAccount;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.Dialogs;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.Common.Navigation;

namespace TopLab.Presentation.ViewModels.Patients;

/// <summary>
/// S-05 Slice 5: Sent-out lab account screen (M16) — per-lab totals + payment + full settle.
/// </summary>
public sealed class SentOutLabAccountViewModel : ViewModelBase
{
    private readonly ISender _mediator;
    private readonly ResultErrorPresenter _presenter;
    private readonly IDialogService _dialogs;
    private readonly INavigationService _navigation;

    private int _externalLabEntityId;
    private SentOutLabAccountDto? _account;
    private string _paymentAmountText = string.Empty;
    private int _selectedSampleId;
    private bool _isBusy;
    private string _errorMessage = string.Empty;
    private string _statusMessage = string.Empty;

    public SentOutLabAccountViewModel(
        ISender mediator,
        ResultErrorPresenter presenter,
        IDialogService dialogs,
        INavigationService navigation)
    {
        _mediator = mediator;
        _presenter = presenter;
        _dialogs = dialogs;
        _navigation = navigation;

        RecordPaymentCommand = new AsyncRelayCommand(async (_, ct) => await RecordPaymentAsync(ct));
        SettleInFullCommand = new AsyncRelayCommand(async (_, ct) => await SettleInFullAsync(ct));
        BackCommand = new RelayCommand(_ => _navigation.NavigateTo<SentOutSamplesViewModel>());
    }

    public int ExternalLabEntityId => _externalLabEntityId;

    public SentOutLabAccountDto? Account
    {
        get => _account;
        private set
        {
            if (SetProperty(ref _account, value))
            {
                OnPropertyChanged(nameof(HasAccount));
                OnPropertyChanged(nameof(ShowEmpty));
            }
        }
    }

    public bool HasAccount => _account is not null;
    public bool ShowEmpty => _account is null && !IsBusy && string.IsNullOrEmpty(ErrorMessage);

    public string PaymentAmountText { get => _paymentAmountText; set => SetProperty(ref _paymentAmountText, value); }

    public int SelectedSampleId
    {
        get => _selectedSampleId;
        set => SetProperty(ref _selectedSampleId, value);
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

    public AsyncRelayCommand RecordPaymentCommand { get; }
    public AsyncRelayCommand SettleInFullCommand { get; }
    public RelayCommand BackCommand { get; }

    public async Task LoadAsync(int externalLabEntityId, DateOnly? from = null, DateOnly? to = null, CancellationToken cancellationToken = default)
    {
        _externalLabEntityId = externalLabEntityId;
        IsBusy = true;
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;
        PaymentAmountText = string.Empty;

        try
        {
            var result = await _mediator.Send(
                new GetSentOutLabAccountQuery(externalLabEntityId, from, to), cancellationToken);

            if (result.IsSuccess && result.Value is not null)
            {
                Account = result.Value;
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

    private async Task RecordPaymentAsync(CancellationToken cancellationToken)
    {
        if (SelectedSampleId <= 0)
        {
            ErrorMessage = "العينة المُرسَلة غير موجودة.";
            return;
        }

        if (string.IsNullOrWhiteSpace(PaymentAmountText)
            || !decimal.TryParse(PaymentAmountText, out var amount)
            || amount <= 0)
        {
            ErrorMessage = "مبلغ الدفع يجب أن يكون أكبر من صفر.";
            return;
        }

        var confirmed = await _dialogs.ShowConfirmationAsync(
            "تأكيد تسجيل الدفعة",
            $"سيتم تسجيل دفعة بمبلغ {amount} على العينة رقم {SelectedSampleId}. هل تريد المتابعة؟");
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
                new RecordSentOutPaymentCommand(SelectedSampleId, amount), cancellationToken);

            if (result.IsSuccess)
            {
                StatusMessage = "تم تسجيل الدفعة.";
                await LoadAsync(_externalLabEntityId, cancellationToken: cancellationToken);
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

    private async Task SettleInFullAsync(CancellationToken cancellationToken)
    {
        if (SelectedSampleId <= 0)
        {
            ErrorMessage = "العينة المُرسَلة غير موجودة.";
            return;
        }

        var confirmed = await _dialogs.ShowConfirmationAsync(
            "تأكيد التسوية الكاملة",
            $"سيتم تسوية العينة رقم {SelectedSampleId} بالكامل. هل تريد المتابعة؟");
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
                new SettleSentOutInFullCommand(SelectedSampleId), cancellationToken);

            if (result.IsSuccess)
            {
                StatusMessage = "تمت التسوية الكاملة.";
                await LoadAsync(_externalLabEntityId, cancellationToken: cancellationToken);
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
