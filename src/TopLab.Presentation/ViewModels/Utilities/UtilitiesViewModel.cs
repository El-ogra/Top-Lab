using System.Collections.ObjectModel;
using MediatR;
using TopLab.Application.Features.Utilities.Commands.AddPhoneBookEntry;
using TopLab.Application.Features.Utilities.Commands.AddPurchaseItem;
using TopLab.Application.Features.Utilities.Commands.RemovePhoneBookEntry;
using TopLab.Application.Features.Utilities.Commands.RemovePurchaseItem;
using TopLab.Application.Features.Utilities.Commands.TogglePurchaseItemDone;
using TopLab.Application.Features.Utilities.Common;
using TopLab.Application.Features.Utilities.Queries.ComputeStopwatchElapsed;
using TopLab.Application.Features.Utilities.Queries.ConvertMeasurementUnit;
using TopLab.Application.Features.Utilities.Queries.EvaluateCalculation;
using TopLab.Application.Features.Utilities.Queries.GetPhoneBook;
using TopLab.Application.Features.Utilities.Queries.GetPurchasesList;
using TopLab.Application.Features.Utilities.Queries.GetTestLibrary;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.Dialogs;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.Common.Navigation;

namespace TopLab.Presentation.ViewModels.Utilities;

/// <summary>
/// S-06 Slice 6: Utilities screen (M23) — six tabs.
/// D11 closed: all elapsed-time computation via ComputeStopwatchElapsedQuery; no local arithmetic.
/// </summary>
public sealed class UtilitiesViewModel : ViewModelBase
{
    private readonly ISender _mediator;
    private readonly ResultErrorPresenter _presenter;
    private readonly IDialogService _dialogs;
    private readonly INavigationService _navigation;

    private int _selectedTab; // 0=calculator, 1=converter, 2=stopwatch, 3=phone book, 4=purchases, 5=test library
    private bool _isBusy;
    private string _errorMessage = string.Empty;
    private string _statusMessage = string.Empty;

    // Calculator
    private string _expression = string.Empty;
    private string _calcResult = string.Empty;

    // Converter
    private string _convertValue = string.Empty;
    private string _fromUnit = string.Empty;
    private string _toUnit = string.Empty;
    private string _convertResult = string.Empty;

    // Stopwatch (D11: all elapsed via query)
    private DateTime _stopwatchStart = DateTime.UtcNow;
    private DateTime? _stopwatchEnd;
    private string _stopwatchResult = string.Empty;

    // Phone book
    private ObservableCollection<PhoneBookEntryDto> _phoneBook = new();
    private string _phoneBookName = string.Empty;
    private string _phoneBookPhone = string.Empty;
    private string _phoneBookNotes = string.Empty;

    // Purchases
    private ObservableCollection<PurchaseItemDto> _purchases = new();
    private string _purchaseText = string.Empty;

    // Test library
    private ObservableCollection<TestLibraryEntryDto> _testLibrary = new();
    private string _testLibraryFilter = string.Empty;

    public UtilitiesViewModel(
        ISender mediator,
        ResultErrorPresenter presenter,
        IDialogService dialogs,
        INavigationService navigation)
    {
        _mediator = mediator;
        _presenter = presenter;
        _dialogs = dialogs;
        _navigation = navigation;

        EvaluateCommand = new AsyncRelayCommand(async (_, ct) => await EvaluateAsync(ct));
        ConvertCommand = new AsyncRelayCommand(async (_, ct) => await ConvertAsync(ct));
        ComputeStopwatchCommand = new AsyncRelayCommand(async (_, ct) => await ComputeStopwatchAsync(ct));
        AddPhoneBookCommand = new AsyncRelayCommand(async (_, ct) => await AddPhoneBookAsync(ct));
        RemovePhoneBookCommand = new AsyncRelayCommand(async (param, ct) => await RemovePhoneBookAsync(param as PhoneBookEntryDto, ct));
        AddPurchaseCommand = new AsyncRelayCommand(async (_, ct) => await AddPurchaseAsync(ct));
        RemovePurchaseCommand = new AsyncRelayCommand(async (param, ct) => await RemovePurchaseAsync(param as PurchaseItemDto, ct));
        TogglePurchaseCommand = new AsyncRelayCommand(async (param, ct) => await TogglePurchaseAsync(param as PurchaseItemDto, ct));
        LoadTestLibraryCommand = new AsyncRelayCommand(async (_, ct) => await LoadTestLibraryAsync(ct));
        BackCommand = new RelayCommand(_ => _navigation.NavigateTo<Shell.HomeViewModel>());
    }

    public int SelectedTab
    {
        get => _selectedTab;
        set
        {
            if (SetProperty(ref _selectedTab, value))
            {
                OnPropertyChanged(nameof(IsCalculatorTab));
                OnPropertyChanged(nameof(IsConverterTab));
                OnPropertyChanged(nameof(IsStopwatchTab));
                OnPropertyChanged(nameof(IsPhoneBookTab));
                OnPropertyChanged(nameof(IsPurchasesTab));
                OnPropertyChanged(nameof(IsTestLibraryTab));
            }
        }
    }

    public bool IsCalculatorTab => SelectedTab == 0;
    public bool IsConverterTab => SelectedTab == 1;
    public bool IsStopwatchTab => SelectedTab == 2;
    public bool IsPhoneBookTab => SelectedTab == 3;
    public bool IsPurchasesTab => SelectedTab == 4;
    public bool IsTestLibraryTab => SelectedTab == 5;

    // Calculator
    public string Expression { get => _expression; set => SetProperty(ref _expression, value); }
    public string CalcResult { get => _calcResult; private set => SetProperty(ref _calcResult, value); }

    // Converter
    public string ConvertValue { get => _convertValue; set => SetProperty(ref _convertValue, value); }
    public string FromUnit { get => _fromUnit; set => SetProperty(ref _fromUnit, value); }
    public string ToUnit { get => _toUnit; set => SetProperty(ref _toUnit, value); }
    public string ConvertResult { get => _convertResult; private set => SetProperty(ref _convertResult, value); }

    // Stopwatch (D11: all elapsed via query)
    public DateTime StopwatchStart { get => _stopwatchStart; set => SetProperty(ref _stopwatchStart, value); }
    public DateTime? StopwatchEnd { get => _stopwatchEnd; set => SetProperty(ref _stopwatchEnd, value); }
    public string StopwatchResult { get => _stopwatchResult; private set => SetProperty(ref _stopwatchResult, value); }

    // Phone book
    public ObservableCollection<PhoneBookEntryDto> PhoneBook
    {
        get => _phoneBook;
        private set
        {
            if (SetProperty(ref _phoneBook, value))
            {
                OnPropertyChanged(nameof(HasPhoneBook));
                OnPropertyChanged(nameof(ShowPhoneBookEmpty));
            }
        }
    }

    public bool HasPhoneBook => PhoneBook.Count > 0;
    public bool ShowPhoneBookEmpty => PhoneBook.Count == 0 && !IsBusy && string.IsNullOrEmpty(ErrorMessage);

    public string PhoneBookName { get => _phoneBookName; set => SetProperty(ref _phoneBookName, value); }
    public string PhoneBookPhone { get => _phoneBookPhone; set => SetProperty(ref _phoneBookPhone, value); }
    public string PhoneBookNotes { get => _phoneBookNotes; set => SetProperty(ref _phoneBookNotes, value); }

    // Purchases
    public ObservableCollection<PurchaseItemDto> Purchases
    {
        get => _purchases;
        private set
        {
            if (SetProperty(ref _purchases, value))
            {
                OnPropertyChanged(nameof(HasPurchases));
                OnPropertyChanged(nameof(ShowPurchasesEmpty));
            }
        }
    }

    public bool HasPurchases => Purchases.Count > 0;
    public bool ShowPurchasesEmpty => Purchases.Count == 0 && !IsBusy && string.IsNullOrEmpty(ErrorMessage);

    public string PurchaseText { get => _purchaseText; set => SetProperty(ref _purchaseText, value); }

    // Test library
    public ObservableCollection<TestLibraryEntryDto> TestLibrary
    {
        get => _testLibrary;
        private set
        {
            if (SetProperty(ref _testLibrary, value))
            {
                OnPropertyChanged(nameof(HasTestLibrary));
                OnPropertyChanged(nameof(ShowTestLibraryEmpty));
            }
        }
    }

    public bool HasTestLibrary => TestLibrary.Count > 0;
    public bool ShowTestLibraryEmpty => TestLibrary.Count == 0 && !IsBusy && string.IsNullOrEmpty(ErrorMessage);

    public string TestLibraryFilter { get => _testLibraryFilter; set => SetProperty(ref _testLibraryFilter, value); }

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

    public AsyncRelayCommand EvaluateCommand { get; }
    public AsyncRelayCommand ConvertCommand { get; }
    public AsyncRelayCommand ComputeStopwatchCommand { get; }
    public AsyncRelayCommand AddPhoneBookCommand { get; }
    public AsyncRelayCommand RemovePhoneBookCommand { get; }
    public AsyncRelayCommand AddPurchaseCommand { get; }
    public AsyncRelayCommand RemovePurchaseCommand { get; }
    public AsyncRelayCommand TogglePurchaseCommand { get; }
    public AsyncRelayCommand LoadTestLibraryCommand { get; }
    public RelayCommand BackCommand { get; }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        await LoadPhoneBookAsync(cancellationToken);
        await LoadPurchasesAsync(cancellationToken);
        await LoadTestLibraryAsync(cancellationToken);
    }

    private async Task EvaluateAsync(CancellationToken cancellationToken)
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            var result = await _mediator.Send(new EvaluateCalculationQuery(Expression), cancellationToken);
            if (result.IsSuccess && result.Value is not null)
            {
                CalcResult = result.Value.Result.ToString();
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

    private async Task ConvertAsync(CancellationToken cancellationToken)
    {
        if (!decimal.TryParse(ConvertValue, out var value))
        {
            ErrorMessage = "قيمة غير صالحة.";
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            var result = await _mediator.Send(new ConvertMeasurementUnitQuery(value, FromUnit, ToUnit), cancellationToken);
            if (result.IsSuccess && result.Value is not null)
            {
                ConvertResult = result.Value.ConvertedValue.ToString();
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

    private async Task ComputeStopwatchAsync(CancellationToken cancellationToken)
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            // D11: all elapsed computation via ComputeStopwatchElapsedQuery — no local arithmetic
            var result = await _mediator.Send(
                new ComputeStopwatchElapsedQuery(StopwatchStart, StopwatchEnd), cancellationToken);
            if (result.IsSuccess && result.Value is not null)
            {
                StopwatchResult = result.Value.Elapsed.ToString();
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

    private async Task LoadPhoneBookAsync(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetPhoneBookQuery(), cancellationToken);
        if (result.IsSuccess && result.Value is not null)
        {
            PhoneBook = new ObservableCollection<PhoneBookEntryDto>(result.Value);
        }
    }

    private async Task AddPhoneBookAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(PhoneBookName))
        {
            ErrorMessage = "الاسم مطلوب.";
            return;
        }

        if (string.IsNullOrWhiteSpace(PhoneBookPhone))
        {
            ErrorMessage = "رقم الهاتف مطلوب.";
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            var result = await _mediator.Send(
                new AddPhoneBookEntryCommand(PhoneBookName, PhoneBookPhone, PhoneBookNotes), cancellationToken);
            if (result.IsSuccess)
            {
                PhoneBookName = string.Empty;
                PhoneBookPhone = string.Empty;
                PhoneBookNotes = string.Empty;
                StatusMessage = "تمت الإضافة.";
                await LoadPhoneBookAsync(cancellationToken);
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

    private async Task RemovePhoneBookAsync(PhoneBookEntryDto? entry, CancellationToken cancellationToken)
    {
        if (entry is null)
        {
            return;
        }

        var confirmed = await _dialogs.ShowConfirmationAsync("تأكيد الحذف", "هل تريد حذف هذا القيد؟");
        if (!confirmed)
        {
            return;
        }

        IsBusy = true;
        try
        {
            var result = await _mediator.Send(new RemovePhoneBookEntryCommand(entry.Id), cancellationToken);
            if (result.IsSuccess)
            {
                StatusMessage = "تم الحذف.";
                await LoadPhoneBookAsync(cancellationToken);
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

    private async Task LoadPurchasesAsync(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetPurchasesListQuery(), cancellationToken);
        if (result.IsSuccess && result.Value is not null)
        {
            Purchases = new ObservableCollection<PurchaseItemDto>(result.Value);
        }
    }

    private async Task AddPurchaseAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(PurchaseText))
        {
            ErrorMessage = "نص البند مطلوب.";
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            var result = await _mediator.Send(new AddPurchaseItemCommand(PurchaseText), cancellationToken);
            if (result.IsSuccess)
            {
                PurchaseText = string.Empty;
                StatusMessage = "تمت الإضافة.";
                await LoadPurchasesAsync(cancellationToken);
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

    private async Task RemovePurchaseAsync(PurchaseItemDto? item, CancellationToken cancellationToken)
    {
        if (item is null)
        {
            return;
        }

        var confirmed = await _dialogs.ShowConfirmationAsync("تأكيد الحذف", "هل تريد حذف هذا البند؟");
        if (!confirmed)
        {
            return;
        }

        IsBusy = true;
        try
        {
            var result = await _mediator.Send(new RemovePurchaseItemCommand(item.Id), cancellationToken);
            if (result.IsSuccess)
            {
                StatusMessage = "تم الحذف.";
                await LoadPurchasesAsync(cancellationToken);
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

    private async Task TogglePurchaseAsync(PurchaseItemDto? item, CancellationToken cancellationToken)
    {
        if (item is null)
        {
            return;
        }

        IsBusy = true;
        try
        {
            var result = await _mediator.Send(new TogglePurchaseItemDoneCommand(item.Id), cancellationToken);
            if (result.IsSuccess)
            {
                await LoadPurchasesAsync(cancellationToken);
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

    private async Task LoadTestLibraryAsync(CancellationToken cancellationToken)
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            var filter = string.IsNullOrWhiteSpace(TestLibraryFilter) ? null : TestLibraryFilter.Trim();
            var result = await _mediator.Send(new GetTestLibraryQuery(filter, null), cancellationToken);
            if (result.IsSuccess && result.Value is not null)
            {
                TestLibrary = new ObservableCollection<TestLibraryEntryDto>(result.Value);
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
