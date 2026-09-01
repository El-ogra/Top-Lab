using MediatR;
using TopLab.Application.Features.SystemAndPrintSettings.Commands.SaveLabPrintText;
using TopLab.Application.Features.SystemAndPrintSettings.Commands.SavePrinterAssignments;
using TopLab.Application.Features.SystemAndPrintSettings.Commands.UpdateReceiptSettings;
using TopLab.Application.Features.SystemAndPrintSettings.Common;
using TopLab.Application.Features.SystemAndPrintSettings.Queries.GetLabPrintText;
using TopLab.Application.Features.SystemAndPrintSettings.Queries.GetPrinterAssignments;
using TopLab.Application.Features.SystemAndPrintSettings.Queries.GetReceiptSettings;
using TopLab.Application.Common.Interfaces;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.Common.Navigation;
using TopLab.Presentation.Services;
using TopLab.Domain.Common.Enums;

namespace TopLab.Presentation.ViewModels.Settings;

/// <summary>S-30: receipt-print settings, the receipt printer assignment, and the
/// receipt lab identification text block and font.</summary>
public sealed class ReceiptSettingsViewModel : ViewModelBase
{
    private readonly ISender _mediator;
    private readonly ResultErrorPresenter _presenter;
    private readonly INavigationService _navigation;

    private string _errorMessage = string.Empty;
    private string _statusMessage = string.Empty;
    private bool _isBusy;
    private bool _hasLoaded;

    private decimal _topMarginCm = 1.0m;
    private string _currency = "L.E.";
    private string _pickupTimeDefault = string.Empty;
    private bool _printOnce;
    private TestDetailDisplayMode _testDetailDisplayMode = TestDetailDisplayMode.Show;
    private bool _cashierPrinterEnabled;
    private HeaderFooterMode _headerFooterMode = HeaderFooterMode.None;
    private string _receiptPrinter = string.Empty;

    private string _labName = string.Empty;
    private string _labAddress = string.Empty;
    private string _labPhone = string.Empty;
    private string _fontFamily = "Arial";
    private int _fontSizePt = 12;

    public ReceiptSettingsViewModel(
        ISender mediator,
        IPrinterCatalogService printers,
        ResultErrorPresenter presenter,
        INavigationService navigation)
    {
        _mediator = mediator;
        _presenter = presenter;
        _navigation = navigation;

        var printerList = printers.GetInstalledPrinters().ToList();
        if (printerList.Count == 0)
        {
            printerList.Add(string.Empty);
        }

        InstalledPrinters = printerList;

        TestDetailDisplayModeOptions = new List<TestDetailDisplayMode>
        {
            TestDetailDisplayMode.Hide,
            TestDetailDisplayMode.Show,
            TestDetailDisplayMode.ShowWithCode
        };
        HeaderFooterModeOptions = new List<HeaderFooterMode> { HeaderFooterMode.None, HeaderFooterMode.Words, HeaderFooterMode.Images };
        FontFamilyOptions = new List<string> { "Arial", "Tahoma", "Calibri", "Times New Roman", "Palatino Linotype" };

        LoadCommand = new AsyncRelayCommand(_ => LoadAsync());
        SaveCommand = new AsyncRelayCommand(_ => SaveAsync());
        SaveCashierPrinterCommand = new AsyncRelayCommand(_ => SaveCashierPrinterAsync());
        BackToDashboardCommand = new RelayCommand(_ => _navigation.NavigateTo<SettingsDashboardViewModel>());
    }

    public IReadOnlyList<string> InstalledPrinters { get; }
    public IReadOnlyList<TestDetailDisplayMode> TestDetailDisplayModeOptions { get; }
    public IReadOnlyList<HeaderFooterMode> HeaderFooterModeOptions { get; }
    public IReadOnlyList<string> FontFamilyOptions { get; }

    public string ErrorMessage { get => _errorMessage; private set => SetProperty(ref _errorMessage, value); }
    public string StatusMessage { get => _statusMessage; private set => SetProperty(ref _statusMessage, value); }
    public bool IsBusy { get => _isBusy; private set => SetProperty(ref _isBusy, value); }

    public AsyncRelayCommand LoadCommand { get; }
    public AsyncRelayCommand SaveCommand { get; }
    public AsyncRelayCommand SaveCashierPrinterCommand { get; }
    public RelayCommand BackToDashboardCommand { get; }

    public decimal TopMarginCm { get => _topMarginCm; set => SetProperty(ref _topMarginCm, value); }
    public string Currency { get => _currency; set => SetProperty(ref _currency, value); }
    public string PickupTimeDefault { get => _pickupTimeDefault; set => SetProperty(ref _pickupTimeDefault, value); }
    public bool PrintOnce { get => _printOnce; set => SetProperty(ref _printOnce, value); }
    public TestDetailDisplayMode TestDetailDisplayMode { get => _testDetailDisplayMode; set => SetProperty(ref _testDetailDisplayMode, value); }
    public bool CashierPrinterEnabled { get => _cashierPrinterEnabled; set => SetProperty(ref _cashierPrinterEnabled, value); }
    public HeaderFooterMode HeaderFooterMode { get => _headerFooterMode; set => SetProperty(ref _headerFooterMode, value); }
    public string ReceiptPrinter { get => _receiptPrinter; set => SetProperty(ref _receiptPrinter, value); }

    public string LabName { get => _labName; set => SetProperty(ref _labName, value); }
    public string LabAddress { get => _labAddress; set => SetProperty(ref _labAddress, value); }
    public string LabPhone { get => _labPhone; set => SetProperty(ref _labPhone, value); }
    public string FontFamily { get => _fontFamily; set => SetProperty(ref _fontFamily, value); }
    public int FontSizePt { get => _fontSizePt; set => SetProperty(ref _fontSizePt, value); }

    public async Task LoadAsync()
    {
        if (_hasLoaded)
        {
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            var settings = await _mediator.Send(new GetReceiptSettingsQuery());
            if (settings.IsSuccess && settings.Value is not null)
            {
                var s = settings.Value;
                TopMarginCm = s.TopMarginCm;
                Currency = s.Currency;
                PickupTimeDefault = s.PickupTimeDefault.HasValue ? s.PickupTimeDefault.Value.ToString("HH:mm") : string.Empty;
                PrintOnce = s.PrintOnce;
                TestDetailDisplayMode = s.TestDetailDisplayMode;
                CashierPrinterEnabled = s.CashierPrinterEnabled;
                HeaderFooterMode = s.HeaderFooterMode;
            }
            else if (settings.Error is not null)
            {
                ErrorMessage = _presenter.Present(settings.Error);
            }

            var printers = await _mediator.Send(new GetPrinterAssignmentsQuery());
            if (printers.IsSuccess && printers.Value is not null)
            {
                var receipt = printers.Value.FirstOrDefault(p => p.OutputType == PrinterOutputType.Receipt);
                if (receipt is not null)
                {
                    ReceiptPrinter = receipt.PrinterName;
                }
            }

            var lab = await _mediator.Send(new GetLabPrintTextQuery { Scope = LabPrintTextScope.Receipt });
            if (lab.IsSuccess && lab.Value is not null)
            {
                LabName = lab.Value.LabName;
                LabAddress = lab.Value.Address;
                LabPhone = lab.Value.Phone;
                FontFamily = lab.Value.FontFamily;
                if (lab.Value.FontSizePt > 0)
                {
                    FontSizePt = lab.Value.FontSizePt;
                }
            }

            _hasLoaded = true;
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task SaveAsync()
    {
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(Currency) || Currency.Length > 10)
        {
            ErrorMessage = "العملة يجب ألا تتجاوز 10 أحرف.";
            return;
        }

        if (string.IsNullOrWhiteSpace(LabName))
        {
            ErrorMessage = "اسم المعمل مطلوب.";
            return;
        }

        TimeOnly? pickup = null;
        if (!string.IsNullOrWhiteSpace(PickupTimeDefault)
            && TimeOnly.TryParse(PickupTimeDefault, out var parsed))
        {
            pickup = parsed;
        }

        IsBusy = true;
        try
        {
            var settingsResult = await _mediator.Send(new UpdateReceiptSettingsCommand(
                TopMarginCm,
                Currency,
                pickup,
                PrintOnce,
                TestDetailDisplayMode,
                CashierPrinterEnabled,
                HeaderFooterMode));
            if (!settingsResult.IsSuccess)
            {
                ErrorMessage = settingsResult.Error is not null ? _presenter.Present(settingsResult.Error) : ErrorMessage;
                return;
            }

            var labResult = await _mediator.Send(new SaveLabPrintTextCommand(
                LabPrintTextScope.Receipt,
                LabName,
                LabAddress,
                LabPhone,
                FontFamily,
                FontSizePt));
            if (!labResult.IsSuccess)
            {
                ErrorMessage = labResult.Error is not null ? _presenter.Present(labResult.Error) : ErrorMessage;
                return;
            }

            StatusMessage = "تم حفظ إعدادات الإيصال.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task SaveCashierPrinterAsync()
    {
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;
        IsBusy = true;
        try
        {
            var result = await _mediator.Send(new SavePrinterAssignmentsCommand(
                new List<PrinterAssignmentDto> { new(PrinterOutputType.Receipt, ReceiptPrinter) }));
            if (result.IsSuccess)
            {
                StatusMessage = "تم تحديث طابعة الإيصالات.";
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