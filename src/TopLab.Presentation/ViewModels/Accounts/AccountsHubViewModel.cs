using System.Collections.ObjectModel;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using TopLab.Application.Features.ExternalEntities.Queries.SearchExternalEntities;
using TopLab.Application.Features.InventoryAndAccounting.Commands.RecordCashDeposit;
using TopLab.Application.Features.InventoryAndAccounting.Commands.RecordCashDisbursement;
using TopLab.Application.Features.InventoryAndAccounting.Common;
using TopLab.Application.Features.InventoryAndAccounting.Queries.GetCashDrawerInventory;
using TopLab.Application.Features.InventoryAndAccounting.Queries.GetCompanyDelegateAccounts;
using TopLab.Application.Features.InventoryAndAccounting.Queries.GetElementInventory;
using TopLab.Application.Features.InventoryAndAccounting.Queries.GetPatientSamplesDetail;
using TopLab.Application.Features.InventoryAndAccounting.Queries.ListCashMovements;
using TopLab.Domain.Common.Enums;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.Dialogs;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.Common.Navigation;

namespace TopLab.Presentation.ViewModels.Accounts;

/// <summary>
/// S-06 Slice 3/4: Accounts hub (M20) — four tabs + cash movement dialog + M14/M16 routes.
/// «الحسابات» shell activation: secondary password then NavigateTo&lt;AccountsHubViewModel&gt;.
/// </summary>
public sealed class AccountsHubViewModel : ViewModelBase
{
    private readonly ISender _mediator;
    private readonly ResultErrorPresenter _presenter;
    private readonly IDialogService _dialogs;
    private readonly IServiceProvider _services;
    private readonly INavigationService _navigation;

    private DateOnly? _from;
    private DateOnly? _to;
    private CashDrawerInventoryDto? _drawer;
    private ObservableCollection<CashMovementDto> _movements = new();
    private int _selectedTab;
    private bool _isBusy;
    private string _errorMessage = string.Empty;
    private string _statusMessage = string.Empty;

    // S4: Element inventory
    private InventoryElementKind _elementKind = InventoryElementKind.User;
    private int? _elementId;
    private AccountType? _accountTypeFilter;
    private InventoryReportType _reportType = InventoryReportType.Summary;
    private ElementInventoryDto? _elementStats;

    // S4: Patient samples
    private ObservableCollection<PatientSampleDetailDto> _patientSamples = new();

    // S4: Company/delegate accounts
    private int? _externalEntityId;
    private ObservableCollection<CompanyDelegateAccountDto> _companyAccounts = new();

    public AccountsHubViewModel(
        ISender mediator,
        ResultErrorPresenter presenter,
        IDialogService dialogs,
        IServiceProvider services,
        INavigationService navigation)
    {
        _mediator = mediator;
        _presenter = presenter;
        _dialogs = dialogs;
        _services = services;
        _navigation = navigation;

        LoadCommand = new AsyncRelayCommand(async (_, ct) => await LoadAsync(ct));
        OpenDepositCommand = new AsyncRelayCommand(async (_, ct) => await OpenCashMovementAsync(isDeposit: true, ct));
        OpenDisbursementCommand = new AsyncRelayCommand(async (_, ct) => await OpenCashMovementAsync(isDeposit: false, ct));
        OpenExternalEntitiesCommand = new RelayCommand(_ =>
        {
            _navigation.NavigateTo<External.ExternalEntitiesViewModel>();
            if (_navigation.CurrentViewModel is External.ExternalEntitiesViewModel vm)
            {
                _ = vm.LoadAsync();
            }
        });
        OpenSentOutSamplesCommand = new RelayCommand(_ =>
        {
            _navigation.NavigateTo<Patients.SentOutSamplesViewModel>();
            if (_navigation.CurrentViewModel is Patients.SentOutSamplesViewModel vm)
            {
                _ = vm.LoadAsync();
            }
        });
        BackCommand = new RelayCommand(_ => _navigation.NavigateTo<Shell.HomeViewModel>());
    }

    public DateOnly? From { get => _from; set => SetProperty(ref _from, value); }
    public DateOnly? To { get => _to; set => SetProperty(ref _to, value); }

    public int SelectedTab
    {
        get => _selectedTab;
        set
        {
            if (SetProperty(ref _selectedTab, value))
            {
                OnPropertyChanged(nameof(IsCashDrawerTab));
                OnPropertyChanged(nameof(IsElementInventoryTab));
                OnPropertyChanged(nameof(IsPatientSamplesTab));
                OnPropertyChanged(nameof(IsCompanyDelegateTab));
            }
        }
    }

    public bool IsCashDrawerTab => SelectedTab == 0;
    public bool IsElementInventoryTab => SelectedTab == 1;
    public bool IsPatientSamplesTab => SelectedTab == 2;
    public bool IsCompanyDelegateTab => SelectedTab == 3;

    public CashDrawerInventoryDto? Drawer
    {
        get => _drawer;
        private set
        {
            if (SetProperty(ref _drawer, value))
            {
                OnPropertyChanged(nameof(HasDrawer));
            }
        }
    }

    public bool HasDrawer => _drawer is not null;

    public ObservableCollection<CashMovementDto> Movements
    {
        get => _movements;
        private set
        {
            if (SetProperty(ref _movements, value))
            {
                OnPropertyChanged(nameof(HasMovements));
                OnPropertyChanged(nameof(ShowMovementsEmpty));
            }
        }
    }

    public bool HasMovements => Movements.Count > 0;
    public bool ShowMovementsEmpty => Movements.Count == 0 && !IsBusy && string.IsNullOrEmpty(ErrorMessage);

    // S4: Element inventory
    public InventoryElementKind ElementKind { get => _elementKind; set => SetProperty(ref _elementKind, value); }
    public int? ElementId { get => _elementId; set => SetProperty(ref _elementId, value); }
    public AccountType? AccountTypeFilter { get => _accountTypeFilter; set => SetProperty(ref _accountTypeFilter, value); }
    public InventoryReportType ReportType { get => _reportType; set => SetProperty(ref _reportType, value); }

    public ElementInventoryDto? ElementStats
    {
        get => _elementStats;
        private set
        {
            if (SetProperty(ref _elementStats, value))
            {
                OnPropertyChanged(nameof(HasElementStats));
            }
        }
    }

    public bool HasElementStats => _elementStats is not null;

    // S4: Patient samples
    public ObservableCollection<PatientSampleDetailDto> PatientSamples
    {
        get => _patientSamples;
        private set
        {
            if (SetProperty(ref _patientSamples, value))
            {
                OnPropertyChanged(nameof(HasPatientSamples));
                OnPropertyChanged(nameof(ShowPatientSamplesEmpty));
            }
        }
    }

    public bool HasPatientSamples => PatientSamples.Count > 0;
    public bool ShowPatientSamplesEmpty => PatientSamples.Count == 0 && !IsBusy && string.IsNullOrEmpty(ErrorMessage);

    // S4: Company/delegate
    public int? ExternalEntityId { get => _externalEntityId; set => SetProperty(ref _externalEntityId, value); }

    public ObservableCollection<CompanyDelegateAccountDto> CompanyAccounts
    {
        get => _companyAccounts;
        private set
        {
            if (SetProperty(ref _companyAccounts, value))
            {
                OnPropertyChanged(nameof(HasCompanyAccounts));
                OnPropertyChanged(nameof(ShowCompanyEmpty));
            }
        }
    }

    public bool HasCompanyAccounts => CompanyAccounts.Count > 0;
    public bool ShowCompanyEmpty => CompanyAccounts.Count == 0 && !IsBusy && string.IsNullOrEmpty(ErrorMessage);

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

    public AsyncRelayCommand LoadCommand { get; }
    public AsyncRelayCommand OpenDepositCommand { get; }
    public AsyncRelayCommand OpenDisbursementCommand { get; }
    public RelayCommand OpenExternalEntitiesCommand { get; }
    public RelayCommand OpenSentOutSamplesCommand { get; }
    public RelayCommand BackCommand { get; }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;

        try
        {
            var drawerResult = await _mediator.Send(new GetCashDrawerInventoryQuery(From, To), cancellationToken);
            if (drawerResult.IsSuccess && drawerResult.Value is not null)
            {
                Drawer = drawerResult.Value;
            }
            else if (drawerResult.Error is not null)
            {
                ErrorMessage = _presenter.Present(drawerResult.Error);
            }

            var movementsResult = await _mediator.Send(new ListCashMovementsQuery(From, To), cancellationToken);
            if (movementsResult.IsSuccess && movementsResult.Value is not null)
            {
                Movements = new ObservableCollection<CashMovementDto>(movementsResult.Value);
            }

            var elementResult = await _mediator.Send(
                new GetElementInventoryQuery(From, To, ElementKind, ElementId, AccountTypeFilter, ReportType), cancellationToken);
            if (elementResult.IsSuccess && elementResult.Value is not null)
            {
                ElementStats = elementResult.Value;
            }

            var samplesResult = await _mediator.Send(new GetPatientSamplesDetailQuery(From, To), cancellationToken);
            if (samplesResult.IsSuccess && samplesResult.Value is not null)
            {
                PatientSamples = new ObservableCollection<PatientSampleDetailDto>(samplesResult.Value);
            }

            var companyResult = await _mediator.Send(
                new GetCompanyDelegateAccountsQuery(From, To, ExternalEntityId), cancellationToken);
            if (companyResult.IsSuccess && companyResult.Value is not null)
            {
                CompanyAccounts = new ObservableCollection<CompanyDelegateAccountDto>(companyResult.Value);
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task OpenCashMovementAsync(bool isDeposit, CancellationToken cancellationToken)
    {
        var vm = _services.GetRequiredService<CashMovementDialogViewModel>();
        await vm.SetupAsync(isDeposit, cancellationToken);
        var window = new Views.Accounts.CashMovementDialogWindow(vm)
        {
            Owner = System.Windows.Application.Current?.MainWindow
        };

        bool? confirmed = window.ShowDialog();
        if (confirmed == true)
        {
            StatusMessage = isDeposit ? "تم تسجيل الإيداع." : "تم تسجيل الصرف.";
            await LoadAsync(cancellationToken);
        }
    }
}

/// <summary>Lab filter ComboBox item (D10 pattern).</summary>
public sealed record LabFilterItem(int? Id, string Name);

/// <summary>Element filter ComboBox item.</summary>
public sealed record ElementFilterItem(int? Id, string Name);
