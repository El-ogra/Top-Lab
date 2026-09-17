using System.Collections.ObjectModel;
using System.Globalization;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientBilling.Commands.PrintInvoice;
using TopLab.Application.Features.PatientBilling.Commands.PrintReceipt;
using TopLab.Application.Features.PatientBilling.Commands.RecordPayment;
using TopLab.Application.Features.PatientBilling.Commands.SettleAccountInFull;
using TopLab.Application.Features.PatientBilling.Commands.VoidPaymentOperation;
using TopLab.Application.Features.PatientBilling.Common;
using TopLab.Application.Features.PatientBilling.Queries.GetPatientAccount;
using TopLab.Application.Features.PatientBilling.Queries.ListPatientPayments;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.Dialogs;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.Common.Navigation;

namespace TopLab.Presentation.ViewModels.Patients;

/// <summary>
/// S-03 Slice 4: patient account screen (P2 M03 S-M03-2). Entered from the patient
/// editor's totals row via <c>NavigateTo&lt;PatientAccountViewModel&gt;()</c> +
/// <c>LoadAsync(patientId)</c> (PatientsHub → editor idiom). All money comes from
/// <c>GetPatientAccountQuery</c> exclusively (SD-9: no client-side arithmetic);
/// the operations grid is paged through <c>ListPatientPaymentsQuery</c>.
/// No balance-gated print enforcement (owner-pending — never added).
/// </summary>
public sealed class PatientAccountViewModel : ViewModelBase
{
    private readonly ISender _mediator;
    private readonly INavigationService _navigation;
    private readonly ResultErrorPresenter _presenter;
    private readonly IDialogService _dialogs;
    private readonly IServiceProvider _services;

    private int _patientId;
    private string _patientFullName = string.Empty;
    private string? _labId;
    private string _accountType = string.Empty;
    private decimal _totalCharged;
    private decimal _totalDiscount;
    private decimal _totalPaid;
    private decimal _balance;
    private int _operationsPage = 1;
    private bool _operationsLoaded;
    private PaymentOperationDto? _selectedOperation;
    private string _paymentAmountText = string.Empty;
    private string? _paymentDiscountText;
    private bool _isBusy;
    private string _errorMessage = string.Empty;
    private string _statusMessage = string.Empty;

    public PatientAccountViewModel(
        ISender mediator,
        INavigationService navigation,
        ResultErrorPresenter presenter,
        IDialogService dialogs,
        IServiceProvider services)
    {
        _mediator = mediator;
        _navigation = navigation;
        _presenter = presenter;
        _dialogs = dialogs;
        _services = services;

        RefreshCommand = new AsyncRelayCommand(async (_, ct) => await LoadAsync(_patientId, ct));
        NextOperationsPageCommand = new AsyncRelayCommand(async (_, ct) => await LoadOperationsPageAsync(nextPage: true, ct));
        PrevOperationsPageCommand = new AsyncRelayCommand(async (_, ct) => await LoadOperationsPageAsync(nextPage: false, ct));
        RecordPaymentCommand = new AsyncRelayCommand(async (_, ct) => await RecordPaymentAsync(ct));
        SettleCommand = new AsyncRelayCommand(async (_, ct) => await SettleAsync(ct));
        PrintReceiptCommand = new AsyncRelayCommand(async (_, ct) => await PrintReceiptAsync(ct));
        OpenCorrectionCommand = new AsyncRelayCommand(async (_, ct) => await OpenCorrectionAsync(ct));
        OpenExtraChargeCommand = new AsyncRelayCommand(async (_, ct) => await OpenExtraChargeAsync(ct));
        VoidSelectedOperationCommand = new AsyncRelayCommand(async (_, ct) => await VoidSelectedOperationAsync(ct));
        BackCommand = new AsyncRelayCommand(async (_, ct) => await BackAsync(ct));

        Operations.CollectionChanged += (_, _) => OnPropertyChanged(nameof(ShowOperationsEmpty));
    }

    /// <summary>Operations grid (paged via ListPatientPaymentsQuery; voided rows stay visible, flagged).</summary>
    public ObservableCollection<PaymentOperationDto> Operations { get; } = new();

    public int PatientId => _patientId;

    public string PatientFullName { get => _patientFullName; private set => SetProperty(ref _patientFullName, value); }

    public string? LabId { get => _labId; private set => SetProperty(ref _labId, value); }

    public string AccountType { get => _accountType; private set => SetProperty(ref _accountType, value); }

    public decimal TotalCharged { get => _totalCharged; private set => SetProperty(ref _totalCharged, value); }

    public decimal TotalDiscount { get => _totalDiscount; private set => SetProperty(ref _totalDiscount, value); }

    public decimal TotalPaid { get => _totalPaid; private set => SetProperty(ref _totalPaid, value); }

    public decimal Balance { get => _balance; private set => SetProperty(ref _balance, value); }

    public int OperationsPage { get => _operationsPage; private set => SetProperty(ref _operationsPage, value); }

    public bool OperationsLoaded { get => _operationsLoaded; private set => SetProperty(ref _operationsLoaded, value); }

    /// <summary>S-03 Slice 4: empty-state flag (S-01/S-02 Show*Empty idiom).</summary>
    public bool ShowOperationsEmpty => OperationsLoaded && Operations.Count == 0;

    /// <summary>S-03 Slice 4: staged for the Slice-5 void dialog (grid selection).</summary>
    public PaymentOperationDto? SelectedOperation { get => _selectedOperation; set => SetProperty(ref _selectedOperation, value); }

    public string PaymentAmountText { get => _paymentAmountText; set => SetProperty(ref _paymentAmountText, value); }

    public string? PaymentDiscountText { get => _paymentDiscountText; set => SetProperty(ref _paymentDiscountText, value); }

    public bool IsBusy { get => _isBusy; private set => SetProperty(ref _isBusy, value); }

    public string ErrorMessage { get => _errorMessage; private set => SetProperty(ref _errorMessage, value); }

    public string StatusMessage { get => _statusMessage; private set => SetProperty(ref _statusMessage, value); }

    public AsyncRelayCommand RefreshCommand { get; }

    public AsyncRelayCommand NextOperationsPageCommand { get; }

    public AsyncRelayCommand PrevOperationsPageCommand { get; }

    public AsyncRelayCommand RecordPaymentCommand { get; }

    public AsyncRelayCommand SettleCommand { get; }

    public AsyncRelayCommand PrintReceiptCommand { get; }

    public AsyncRelayCommand OpenCorrectionCommand { get; }

    public AsyncRelayCommand OpenExtraChargeCommand { get; }

    public AsyncRelayCommand VoidSelectedOperationCommand { get; }

    public AsyncRelayCommand BackCommand { get; }

    public async Task LoadAsync(int patientId, CancellationToken cancellationToken = default)
    {
        _patientId = patientId;
        OperationsPage = 1;
        await RefreshAsync(cancellationToken);
    }

    private async Task RefreshAsync(CancellationToken cancellationToken)
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;
        try
        {
            var account = await _mediator.Send(new GetPatientAccountQuery(_patientId), cancellationToken);
            if (!account.IsSuccess)
            {
                ErrorMessage = _presenter.Present(account.Error!);
                return;
            }

            var dto = account.Value!;
            PatientFullName = dto.PatientFullName;
            LabId = dto.LabId;
            AccountType = dto.AccountType;
            TotalCharged = dto.TotalCharged;
            TotalDiscount = dto.TotalDiscount;
            TotalPaid = dto.TotalPaid;
            Balance = dto.Balance;

            await LoadOperationsAsync(cancellationToken);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadOperationsPageAsync(bool nextPage, CancellationToken cancellationToken)
    {
        if (nextPage)
        {
            OperationsPage += 1;
        }
        else
        {
            if (OperationsPage <= 1)
            {
                return;
            }

            OperationsPage -= 1;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            await LoadOperationsAsync(cancellationToken);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadOperationsAsync(CancellationToken cancellationToken)
    {
        var operations = await _mediator.Send(new ListPatientPaymentsQuery(_patientId, OperationsPage, 50), cancellationToken);
        if (!operations.IsSuccess)
        {
            ErrorMessage = _presenter.Present(operations.Error!);
            return;
        }

        Operations.Clear();
        foreach (var operation in operations.Value!)
        {
            Operations.Add(operation);
        }

        OperationsLoaded = true;
        OnPropertyChanged(nameof(ShowOperationsEmpty));
    }

    /// <summary>Reuses the editor's record-payment flow (same texts).</summary>
    private async Task RecordPaymentAsync(CancellationToken cancellationToken)
    {
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;

        if (!decimal.TryParse(PaymentAmountText, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount) || amount <= 0)
        {
            ErrorMessage = "قيمة المدفوع غير صالحة.";
            return;
        }

        decimal? discount = null;
        if (!string.IsNullOrWhiteSpace(PaymentDiscountText))
        {
            if (!decimal.TryParse(PaymentDiscountText, NumberStyles.Number, CultureInfo.InvariantCulture, out var d) || d < 0)
            {
                ErrorMessage = "قيمة الخصم غير صالحة.";
                return;
            }

            discount = d;
        }

        IsBusy = true;
        try
        {
            var result = await _mediator.Send(new RecordPaymentCommand(_patientId, amount, discount), cancellationToken);
            if (!result.IsSuccess)
            {
                ErrorMessage = _presenter.Present(result.Error!);
                return;
            }

            PaymentAmountText = string.Empty;
            PaymentDiscountText = null;
            await RefreshAsync(cancellationToken);
            StatusMessage = "تم تسجيل المدفوع.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>Full settlement (editor flow + Slice-5 mandatory confirmation).</summary>
    private async Task SettleAsync(CancellationToken cancellationToken)
    {
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;

        var confirm = await _dialogs.ShowConfirmationAsync("تسوية الحساب", "سيتم تسوية حساب المريض بالكامل. هل تريد المتابعة؟");
        if (!confirm)
        {
            return;
        }

        IsBusy = true;
        try
        {
            var result = await _mediator.Send(new SettleAccountInFullCommand(_patientId), cancellationToken);
            if (!result.IsSuccess)
            {
                ErrorMessage = _presenter.Present(result.Error!);
                return;
            }

            await RefreshAsync(cancellationToken);
            StatusMessage = "تمت تسوية الحساب بالكامل.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>Receipt printing (no balance gate — owner-pending, never enforced).</summary>
    private async Task PrintReceiptAsync(CancellationToken cancellationToken)
    {
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;

        IsBusy = true;
        try
        {
            var result = await _mediator.Send(new PrintReceiptCommand(_patientId), cancellationToken);
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

    private async Task BackAsync(CancellationToken cancellationToken)
    {
        _navigation.NavigateTo<PatientEditorViewModel>();
        if (_navigation.CurrentViewModel is PatientEditorViewModel editor)
        {
            await editor.LoadCatalogAsync(cancellationToken);
            await editor.LoadPatientAsync(_patientId, cancellationToken);
        }
    }

    /// <summary>S-03 Slice 5: correction dialog (S-M03-3) — amount only, no reason field.</summary>
    private async Task OpenCorrectionAsync(CancellationToken cancellationToken)
    {
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;

        var vm = _services.GetRequiredService<CorrectionDialogViewModel>();
        vm.Setup(_patientId);
        var window = new Views.Patients.CorrectionDialogWindow(vm)
        {
            Owner = System.Windows.Application.Current?.MainWindow
        };
        bool? confirmed = window.ShowDialog();
        if (confirmed == true)
        {
            await RefreshAsync(cancellationToken);
            StatusMessage = "تم تسجيل القيد التصحيحي.";
        }
    }

    /// <summary>S-03 Slice 5: extra-charge dialog (S-M03-4) — amount only, no discount field.</summary>
    private async Task OpenExtraChargeAsync(CancellationToken cancellationToken)
    {
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;

        var vm = _services.GetRequiredService<ExtraChargeDialogViewModel>();
        vm.Setup(_patientId);
        var window = new Views.Patients.ExtraChargeDialogWindow(vm)
        {
            Owner = System.Windows.Application.Current?.MainWindow
        };
        bool? confirmed = window.ShowDialog();
        if (confirmed == true)
        {
            await RefreshAsync(cancellationToken);
            StatusMessage = "تم تسجيل المبلغ الإضافي.";
        }
    }

    /// <summary>
    /// S-03 Slice 5: void flow (S-M03-5) — flag only, no reverse operation, no delete,
    /// no edit. The void itself is the dialog (confirmation idiom, cf. delete):
    /// a wrong amount is corrected by void-and-reissue only.
    /// </summary>
    private async Task VoidSelectedOperationAsync(CancellationToken cancellationToken)
    {
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;

        if (SelectedOperation is null)
        {
            ErrorMessage = "اختر عملية أولًا.";
            return;
        }

        var confirm = await _dialogs.ShowConfirmationAsync(
            "إلغاء عملية",
            "سيتم إلغاء العملية المحددة (تبقى ظاهرة بعلامة ملغاة). هل تريد المتابعة؟");
        if (!confirm)
        {
            return;
        }

        IsBusy = true;
        try
        {
            Result result = await _mediator.Send(new VoidPaymentOperationCommand(SelectedOperation.PaymentOperationId), cancellationToken);
            if (!result.IsSuccess)
            {
                ErrorMessage = _presenter.Present(result.Error!);
                return;
            }

            await RefreshAsync(cancellationToken);
            StatusMessage = "تم إلغاء العملية.";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
