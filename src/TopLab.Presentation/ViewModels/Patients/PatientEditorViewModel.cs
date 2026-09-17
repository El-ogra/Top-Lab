using System.Collections.ObjectModel;
using System.Globalization;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.AnalyteProfiles.Common;
using TopLab.Application.Features.AnalyteProfiles.Queries.GetProfileDefinitions;
using TopLab.Application.Features.PatientBilling.Commands.PrintInvoice;
using TopLab.Application.Features.PatientBilling.Commands.PrintReceipt;
using TopLab.Application.Features.PatientBilling.Commands.RecordPayment;
using TopLab.Application.Features.PatientBilling.Commands.SettleAccountInFull;
using TopLab.Application.Features.PatientBilling.Queries.GetPatientAccount;
using TopLab.Application.Features.PatientRegistration.Commands.AddCustomGroupToVisit;
using TopLab.Application.Features.PatientRegistration.Commands.AddMedicalCondition;
using TopLab.Application.Features.PatientRegistration.Commands.AddProfileToVisit;
using TopLab.Application.Features.PatientRegistration.Commands.AddTestsToVisit;
using TopLab.Application.Features.PatientRegistration.Commands.ClearAllTests;
using TopLab.Application.Features.PatientRegistration.Commands.CreatePatient;
using TopLab.Application.Features.PatientRegistration.Commands.PrintBarcode;
using TopLab.Application.Features.PatientRegistration.Commands.RemoveMedicalCondition;
using TopLab.Application.Features.PatientRegistration.Commands.RemoveTestFromVisit;
using TopLab.Application.Features.PatientRegistration.Commands.SoftDeletePatient;
using TopLab.Application.Features.PatientRegistration.Commands.UpdatePatient;
using TopLab.Application.Features.PatientRegistration.Commands.UpdatePatientTestSampleFlags;
using TopLab.Application.Features.PatientRegistration.Common;
using TopLab.Application.Features.PatientRegistration.Queries.GetNextLabId;
using TopLab.Application.Features.PatientRegistration.Queries.GetPatientById;
using TopLab.Application.Features.PatientRegistration.Queries.GetPatientVisitHistory;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Common;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Queries.GetCustomGroups;
using TopLab.Application.Features.PatientRegistration.Queries.GetRegistrationCatalog;
using TopLab.Application.Features.PatientRegistration.Queries.SearchPatients;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Common;
using TopLab.Application.Features.WorkSheets.Commands.PrintWorkSheet;
using TopLab.Application.Features.WorkSheets.Queries.GetVisitWorkSheet;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Patients;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.Dialogs;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.Common.Navigation;

namespace TopLab.Presentation.ViewModels.Patients;

/// <summary>
/// Unified Add/Edit Patient Data screen (S-01 slice S6): demographics +
/// test ordering with sample-type flags + live billing + four print actions.
/// Single source of truth for money is always <c>GetPatientAccountQuery</c>;
/// edit-mode test changes go through the granular M02 delta commands.
/// Server-side gates enforce authorization (SD-9: no Presentation duplication).
/// </summary>
public sealed class PatientEditorViewModel : ViewModelBase
{
    private readonly ISender _mediator;
    private readonly ResultErrorPresenter _presenter;
    private readonly IDialogService _dialogs;
    private readonly IServiceProvider _services;

    private bool _isEditMode;
    private int? _patientId;
    private string _labId = string.Empty;
    private string _fullName = string.Empty;
    private string? _title;
    private bool _isMale = true;
    private int _ageValue;
    private AgeUnit _ageUnit = AgeUnit.Year;
    private string _phoneNumber = string.Empty;
    private string? _nationalId;
    private string? _address;
    private AccountType _accountType = AccountType.Individual;
    private bool _isVip;
    private string? _treatingDoctorIdText;
    private string? _treatingDoctorName;
    private string? _referralEntityIdText;
    private string? _referralEntityName;
    private bool _isFastingIndicated;
    private string? _fastingHoursText;
    private bool _recentContrastImaging;
    private string? _notes;
    private string _paymentAmountText = string.Empty;
    private string? _paymentDiscountText;
    private decimal _totalCharged;
    private decimal _totalDiscount;
    private decimal _totalPaid;
    private decimal _balance;
    private bool _isBusy;
    private string _errorMessage = string.Empty;
    private string _statusMessage = string.Empty;
    private string _searchTerm = string.Empty;
    private int _searchPage = 1;
    private int _searchPageSize = 20;
    private bool _searchExecuted;
    private ProfileDefinitionDto? _selectedProfile;
    private bool _profileIsUrine;
    private bool _profileIsStool;
    private bool _profileIsBlood = true;
    private bool _profileIsSemen;
    private bool _profileIsCsf;
    private bool _profileIsTakenOutsideLab;
    private CustomGroupSummaryDto? _selectedCustomGroup;

    public PatientEditorViewModel(
        ISender mediator,
        INavigationService navigation,
        ResultErrorPresenter presenter,
        IDialogService dialogs,
        IServiceProvider services)
    {
        _mediator = mediator;
        _presenter = presenter;
        _dialogs = dialogs;
        _services = services;

        NewPatientCommand = new AsyncRelayCommand(async (_, ct) => await NewPatientAsync());
        SaveCommand = new AsyncRelayCommand(async (_, ct) => await SaveAsync(ct));
        DeleteCommand = new AsyncRelayCommand(async (_, ct) => await DeleteAsync());
        UndoCommand = new AsyncRelayCommand(async (_, ct) => await UndoAsync());
        RecordPaymentCommand = new AsyncRelayCommand(async (_, ct) => await RecordPaymentAsync(ct));
        SettleCommand = new AsyncRelayCommand(async (_, ct) => await SettleAsync(ct));
        PrintBarcodeCommand = new AsyncRelayCommand(async (_, ct) => await PrintAsync(ct, "barcode"));
        PrintWorkSheetCommand = new AsyncRelayCommand(async (_, ct) => await PrintAsync(ct, "worksheet"));
        PrintReceiptCommand = new AsyncRelayCommand(async (_, ct) => await PrintAsync(ct, "receipt"));
        PrintInvoiceCommand = new AsyncRelayCommand(async (_, ct) => await PrintAsync(ct, "invoice"));
        AddTestToSelectionCommand = new RelayCommand(param =>
        {
            if (param is int testId)
            {
                AddTestToSelection(testId);
            }
        });
        RemoveSelectedTestCommand = new RelayCommand(param =>
        {
            if (param is SelectedTestItem item)
            {
                SelectedTests.Remove(item);
            }
        });
        PickTreatingDoctorCommand = new AsyncRelayCommand(_ => PickTreatingDoctorAsync());
        PickReferralEntityCommand = new AsyncRelayCommand(_ => PickReferralEntityAsync());
        SearchPatientsCommand = new AsyncRelayCommand(async (_, ct) => await SearchPatientsAsync(resetPage: true, cancellationToken: ct));
        NextSearchPageCommand = new AsyncRelayCommand(async (_, ct) => await SearchPatientsAsync(resetPage: false, cancellationToken: ct, nextPage: true));
        PrevSearchPageCommand = new AsyncRelayCommand(async (_, ct) => await SearchPatientsAsync(resetPage: false, cancellationToken: ct));
        ClearSearchCommand = new RelayCommand(ClearSearch);
        OpenSearchResultCommand = new AsyncRelayCommand(async (param, ct) => await OpenSearchResultAsync(param as PatientSummaryDto, ct));
        AddProfileCommand = new AsyncRelayCommand(async (_, ct) => await AddProfileAsync(ct));
        AddCustomGroupCommand = new AsyncRelayCommand(async (_, ct) => await AddCustomGroupAsync(ct));
        ClearAllVisitTestsCommand = new AsyncRelayCommand(async (_, ct) => await ClearAllVisitTestsAsync(ct));

        // S-03 Slice 0: empty-state visibility follows the S-01/S-02 Show*Empty idiom.
        SelectedTests.CollectionChanged += (_, _) => RefreshEmptyStates();
        CatalogTests.CollectionChanged += (_, _) => RefreshEmptyStates();
        SearchResults.CollectionChanged += (_, _) => OnPropertyChanged(nameof(ShowSearchEmpty));
        VisitHistory.CollectionChanged += (_, _) => OnPropertyChanged(nameof(ShowVisitHistoryEmpty));
    }

    public sealed class SelectedTestItem : ViewModelBase
    {
        private bool _isUrine;
        private bool _isStool;
        private bool _isBlood = true;
        private bool _isSemen;
        private bool _isCsf;
        private bool _isTakenOutsideLab;

        public int PatientTestId { get; init; }

        public int TestId { get; init; }

        public string TestName { get; init; } = string.Empty;

        public decimal Price { get; init; }

        public bool IsNew => PatientTestId == 0;

        public bool IsUrine { get => _isUrine; set => SetProperty(ref _isUrine, value); }

        public bool IsStool { get => _isStool; set => SetProperty(ref _isStool, value); }

        public bool IsBlood { get => _isBlood; set => SetProperty(ref _isBlood, value); }

        public bool IsSemen { get => _isSemen; set => SetProperty(ref _isSemen, value); }

        public bool IsCsf { get => _isCsf; set => SetProperty(ref _isCsf, value); }

        public bool IsTakenOutsideLab { get => _isTakenOutsideLab; set => SetProperty(ref _isTakenOutsideLab, value); }
    }

    public sealed class ConditionItem : ViewModelBase
    {
        private bool _isSelected;

        public int MedicalConditionTypeId { get; init; }

        public string Name { get; init; } = string.Empty;

        public bool IsSelected { get => _isSelected; set => SetProperty(ref _isSelected, value); }
    }

    public ObservableCollection<TestSummaryDto> CatalogTests { get; } = new();

    public ObservableCollection<TestGroupDto> TestGroups { get; } = new();

    public ObservableCollection<string> Titles { get; } = new();

    public ObservableCollection<ConditionItem> Conditions { get; } = new();

    public ObservableCollection<SelectedTestItem> SelectedTests { get; } = new();

    public IReadOnlyList<AgeUnit> AgeUnits { get; } =
        Enum.GetValues<AgeUnit>();

    public IReadOnlyList<AccountType> AccountTypes { get; } =
        Enum.GetValues<AccountType>();

    public bool IsEditMode { get => _isEditMode; private set => SetProperty(ref _isEditMode, value); }

    public string PatientIdText => _patientId?.ToString(CultureInfo.InvariantCulture) ?? "جديد";

    public string LabId { get => _labId; set => SetProperty(ref _labId, value); }

    public string FullName { get => _fullName; set => SetProperty(ref _fullName, value); }

    public string? Title { get => _title; set => SetProperty(ref _title, value); }

    public bool IsMale { get => _isMale; set => SetProperty(ref _isMale, value); }

    public int AgeValue { get => _ageValue; set => SetProperty(ref _ageValue, value); }

    public AgeUnit AgeUnit { get => _ageUnit; set => SetProperty(ref _ageUnit, value); }

    public string PhoneNumber { get => _phoneNumber; set => SetProperty(ref _phoneNumber, value); }

    public string? NationalId { get => _nationalId; set => SetProperty(ref _nationalId, value); }

    public string? Address { get => _address; set => SetProperty(ref _address, value); }

    public AccountType AccountType { get => _accountType; set => SetProperty(ref _accountType, value); }

    public bool IsVip { get => _isVip; set => SetProperty(ref _isVip, value); }

    public string? TreatingDoctorIdText { get => _treatingDoctorIdText; set => SetProperty(ref _treatingDoctorIdText, value); }

    public string? TreatingDoctorName { get => _treatingDoctorName; private set => SetProperty(ref _treatingDoctorName, value); }

    public string? ReferralEntityIdText { get => _referralEntityIdText; set => SetProperty(ref _referralEntityIdText, value); }

    public string? ReferralEntityName { get => _referralEntityName; private set => SetProperty(ref _referralEntityName, value); }

    public AsyncRelayCommand PickTreatingDoctorCommand { get; }
    public AsyncRelayCommand PickReferralEntityCommand { get; }

    /// <summary>S-02 Slice 6: fills the doctor fields from the entity picker.</summary>
    public void SetTreatingDoctor(int id, string name)
    {
        TreatingDoctorIdText = id.ToString(CultureInfo.InvariantCulture);
        TreatingDoctorName = name;
    }

    /// <summary>S-02 Slice 6: fills the referral fields from the entity picker.</summary>
    public void SetReferralEntity(int id, string name)
    {
        ReferralEntityIdText = id.ToString(CultureInfo.InvariantCulture);
        ReferralEntityName = name;
    }

    private async Task PickTreatingDoctorAsync()
    {
        var vm = _services.GetRequiredService<ViewModels.External.ExternalEntityPickerViewModel>();
        vm.PresetType(Domain.Common.Enums.EntityType.TreatingDoctor);
        await vm.LoadAsync();
        var window = new Views.External.ExternalEntityPickerWindow(vm)
        {
            Owner = System.Windows.Application.Current?.MainWindow
        };
        bool? picked = window.ShowDialog();
        if (picked == true && vm.SelectedEntity is not null)
        {
            SetTreatingDoctor(vm.SelectedEntity.Id, vm.SelectedEntity.Name);
        }
    }

    private async Task PickReferralEntityAsync()
    {
        var vm = _services.GetRequiredService<ViewModels.External.ExternalEntityPickerViewModel>();
        await vm.LoadAsync();
        var window = new Views.External.ExternalEntityPickerWindow(vm)
        {
            Owner = System.Windows.Application.Current?.MainWindow
        };
        bool? picked = window.ShowDialog();
        if (picked == true && vm.SelectedEntity is not null)
        {
            SetReferralEntity(vm.SelectedEntity.Id, vm.SelectedEntity.Name);
        }
    }

    public bool IsFastingIndicated { get => _isFastingIndicated; set => SetProperty(ref _isFastingIndicated, value); }

    public string? FastingHoursText { get => _fastingHoursText; set => SetProperty(ref _fastingHoursText, value); }

    public bool RecentContrastImaging { get => _recentContrastImaging; set => SetProperty(ref _recentContrastImaging, value); }

    public string? Notes { get => _notes; set => SetProperty(ref _notes, value); }

    public string PaymentAmountText { get => _paymentAmountText; set => SetProperty(ref _paymentAmountText, value); }

    public string? PaymentDiscountText { get => _paymentDiscountText; set => SetProperty(ref _paymentDiscountText, value); }

    public decimal TotalCharged { get => _totalCharged; private set => SetProperty(ref _totalCharged, value); }

    public decimal TotalDiscount { get => _totalDiscount; private set => SetProperty(ref _totalDiscount, value); }

    public decimal TotalPaid { get => _totalPaid; private set => SetProperty(ref _totalPaid, value); }

    public decimal Balance { get => _balance; private set => SetProperty(ref _balance, value); }

    public bool IsBusy { get => _isBusy; private set => SetProperty(ref _isBusy, value); }

    /// <summary>S-03 Slice 0: empty-state flags (S-01/S-02 Show*Empty idiom).</summary>
    public bool ShowSelectedTestsEmpty => SelectedTests.Count == 0;

    /// <summary>S-03 Slice 0: empty-state flags (S-01/S-02 Show*Empty idiom).</summary>
    public bool ShowCatalogEmpty => CatalogTests.Count == 0;

    private void RefreshEmptyStates()
    {
        OnPropertyChanged(nameof(ShowSelectedTestsEmpty));
        OnPropertyChanged(nameof(ShowCatalogEmpty));
    }

    public string ErrorMessage { get => _errorMessage; private set => SetProperty(ref _errorMessage, value); }

    public string StatusMessage { get => _statusMessage; private set => SetProperty(ref _statusMessage, value); }

    /// <summary>S-03 Slice 1: patient search-to-edit state (consumes SearchPatientsQuery).</summary>
    public string SearchTerm { get => _searchTerm; set => SetProperty(ref _searchTerm, value); }

    public ObservableCollection<PatientSummaryDto> SearchResults { get; } = new();

    /// <summary>S-03 Slice 2: read-only visit history (sibling visits sharing the LabId).</summary>
    public ObservableCollection<VisitHistoryDto> VisitHistory { get; } = new();

    /// <summary>S-03 Slice 3: profile picker source (existing GetProfileDefinitionsQuery).</summary>
    public ObservableCollection<ProfileDefinitionDto> Profiles { get; } = new();

    /// <summary>S-03 Slice 3: custom-group picker source (existing GetCustomGroupsQuery).</summary>
    public ObservableCollection<CustomGroupSummaryDto> CustomGroups { get; } = new();

    public int SearchPage { get => _searchPage; private set => SetProperty(ref _searchPage, value); }

    public int SearchPageSize { get => _searchPageSize; private set => SetProperty(ref _searchPageSize, value); }

    public bool SearchExecuted { get => _searchExecuted; private set => SetProperty(ref _searchExecuted, value); }

    /// <summary>S-03 Slice 1: empty-state flag (S-01/S-02 Show*Empty idiom).</summary>
    public bool ShowSearchEmpty => SearchExecuted && SearchResults.Count == 0;

    /// <summary>S-03 Slice 2: history section is visible in edit mode only; shows the
    /// empty text when there is no prior visit (the current visit alone).</summary>
    public bool IsVisitHistoryVisible => IsEditMode;

    /// <summary>S-03 Slice 2: empty-state flag (S-01/S-02 Show*Empty idiom).</summary>
    public bool ShowVisitHistoryEmpty => VisitHistory.Count <= 1;

    /// <summary>S-03 Slice 3: profile / custom-group picker state (edit mode only).</summary>
    public ProfileDefinitionDto? SelectedProfile { get => _selectedProfile; set => SetProperty(ref _selectedProfile, value); }

    public bool ProfileIsUrine { get => _profileIsUrine; set => SetProperty(ref _profileIsUrine, value); }

    public bool ProfileIsStool { get => _profileIsStool; set => SetProperty(ref _profileIsStool, value); }

    public bool ProfileIsBlood { get => _profileIsBlood; set => SetProperty(ref _profileIsBlood, value); }

    public bool ProfileIsSemen { get => _profileIsSemen; set => SetProperty(ref _profileIsSemen, value); }

    public bool ProfileIsCsf { get => _profileIsCsf; set => SetProperty(ref _profileIsCsf, value); }

    public bool ProfileIsTakenOutsideLab { get => _profileIsTakenOutsideLab; set => SetProperty(ref _profileIsTakenOutsideLab, value); }

    public CustomGroupSummaryDto? SelectedCustomGroup { get => _selectedCustomGroup; set => SetProperty(ref _selectedCustomGroup, value); }

    public AsyncRelayCommand NewPatientCommand { get; }

    public AsyncRelayCommand SaveCommand { get; }

    public AsyncRelayCommand DeleteCommand { get; }

    public AsyncRelayCommand UndoCommand { get; }

    public AsyncRelayCommand RecordPaymentCommand { get; }

    public AsyncRelayCommand SettleCommand { get; }

    public AsyncRelayCommand PrintBarcodeCommand { get; }

    public AsyncRelayCommand PrintWorkSheetCommand { get; }

    public AsyncRelayCommand PrintReceiptCommand { get; }

    public AsyncRelayCommand PrintInvoiceCommand { get; }

    public RelayCommand AddTestToSelectionCommand { get; }

    public RelayCommand RemoveSelectedTestCommand { get; }

    public AsyncRelayCommand SearchPatientsCommand { get; }

    public AsyncRelayCommand NextSearchPageCommand { get; }

    public AsyncRelayCommand PrevSearchPageCommand { get; }

    public RelayCommand ClearSearchCommand { get; }

    public AsyncRelayCommand OpenSearchResultCommand { get; }

    public AsyncRelayCommand AddProfileCommand { get; }

    public AsyncRelayCommand AddCustomGroupCommand { get; }

    public AsyncRelayCommand ClearAllVisitTestsCommand { get; }

    public async Task LoadCatalogAsync(CancellationToken cancellationToken = default)
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            var catalog = await _mediator.Send(new GetRegistrationCatalogQuery(), cancellationToken);
            if (!catalog.IsSuccess)
            {
                ErrorMessage = _presenter.Present(catalog.Error!);
                return;
            }

            CatalogTests.Clear();
            foreach (var test in catalog.Value!.Tests)
            {
                CatalogTests.Add(test);
            }

            TestGroups.Clear();
            foreach (var group in catalog.Value.TestGroups)
            {
                TestGroups.Add(group);
            }

            Titles.Clear();
            foreach (var title in catalog.Value.PatientTitles)
            {
                Titles.Add(title.TitleText);
            }

            Conditions.Clear();
            foreach (var condition in catalog.Value.MedicalConditionTypes)
            {
                Conditions.Add(new ConditionItem { MedicalConditionTypeId = condition.MedicalConditionTypeId, Name = condition.Name });
            }

            AccountType = catalog.Value.DefaultAccountType;

            if (!IsEditMode)
            {
                var nextLabId = await _mediator.Send(new GetNextLabIdQuery(), cancellationToken);
                if (nextLabId.IsSuccess && string.IsNullOrWhiteSpace(LabId))
                {
                    LabId = nextLabId.Value!;
                }
            }

            await LoadPickerSourcesAsync(cancellationToken);
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// S-03 Slice 3 (U-04): picker sources are existing queries only — the registration
    /// catalog carries no profiles/custom-groups. Picker failures are non-fatal: the
    /// error surfaces and the pickers stay empty (add-guards refuse without selection).
    /// Note: profile listing requires EDIT_SYSTEM_SETTINGS; a denial surfaces verbatim.
    /// </summary>
    private async Task LoadPickerSourcesAsync(CancellationToken cancellationToken)
    {
        var profiles = await _mediator.Send(new GetProfileDefinitionsQuery(), cancellationToken);
        if (!profiles.IsSuccess)
        {
            ErrorMessage = _presenter.Present(profiles.Error!);
        }
        else
        {
            Profiles.Clear();
            foreach (var profile in profiles.Value!)
            {
                Profiles.Add(profile);
            }
        }

        var groups = await _mediator.Send(new GetCustomGroupsQuery(), cancellationToken);
        if (!groups.IsSuccess)
        {
            ErrorMessage = _presenter.Present(groups.Error!);
        }
        else
        {
            CustomGroups.Clear();
            foreach (var group in groups.Value!)
            {
                CustomGroups.Add(group);
            }
        }
    }

    public async Task LoadPatientAsync(int patientId, CancellationToken cancellationToken = default)
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;
        try
        {
            var detail = await _mediator.Send(new GetPatientByIdQuery(patientId), cancellationToken);
            if (!detail.IsSuccess)
            {
                ErrorMessage = _presenter.Present(detail.Error!);
                return;
            }

            var d = detail.Value!;
            _patientId = d.PatientId;
            OnPropertyChanged(nameof(PatientIdText));
            IsEditMode = true;
            LabId = d.LabId ?? string.Empty;
            FullName = d.FullName;
            Title = d.Title;
            IsMale = d.Sex == Sex.Male;
            AgeValue = d.AgeValue;
            AgeUnit = d.AgeUnit;
            PhoneNumber = d.PhoneNumbers.Count > 0 ? d.PhoneNumbers[0].PhoneNumber : string.Empty;
            NationalId = d.NationalId;
            Address = d.Address;
            AccountType = d.AccountType;
            IsVip = d.IsVip;
            TreatingDoctorIdText = d.TreatingDoctorId?.ToString(CultureInfo.InvariantCulture);
            TreatingDoctorName = d.TreatingDoctorName;
            ReferralEntityIdText = d.ReferralEntityId?.ToString(CultureInfo.InvariantCulture);
            ReferralEntityName = d.ReferralEntityName;
            IsFastingIndicated = d.IsFastingIndicated;
            FastingHoursText = d.FastingHours?.ToString(CultureInfo.InvariantCulture);
            RecentContrastImaging = d.RecentContrastImaging;
            Notes = d.Notes;

            var selectedConditions = d.MedicalConditions.Select(m => m.MedicalConditionTypeId).ToHashSet();
            foreach (var condition in Conditions)
            {
                condition.IsSelected = selectedConditions.Contains(condition.MedicalConditionTypeId);
            }

            await LoadVisitTestsAsync(cancellationToken);
            await RefreshBillingAsync(cancellationToken);
            await LoadVisitHistoryAsync(cancellationToken);
            OnPropertyChanged(nameof(IsVisitHistoryVisible));
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// S-03 Slice 2: read-only sibling-visit history. Display uses DTO fields only;
    /// backend «المريض غير موجود.» surfaces via the presenter.
    /// </summary>
    private async Task LoadVisitHistoryAsync(CancellationToken cancellationToken)
    {
        VisitHistory.Clear();
        if (!_patientId.HasValue)
        {
            OnPropertyChanged(nameof(ShowVisitHistoryEmpty));
            return;
        }

        var history = await _mediator.Send(new GetPatientVisitHistoryQuery(_patientId.Value), cancellationToken);
        if (!history.IsSuccess)
        {
            ErrorMessage = _presenter.Present(history.Error!);
            return;
        }

        foreach (var visit in history.Value!)
        {
            VisitHistory.Add(visit);
        }
    }

    private async Task LoadVisitTestsAsync(CancellationToken cancellationToken)
    {
        SelectedTests.Clear();
        if (!_patientId.HasValue)
        {
            return;
        }

        var sheet = await _mediator.Send(new GetVisitWorkSheetQuery(_patientId.Value), cancellationToken);
        if (!sheet.IsSuccess)
        {
            ErrorMessage = _presenter.Present(sheet.Error!);
            return;
        }

        var samples = sheet.Value!.Samples.ToDictionary(s => s.PatientTestId);
        foreach (var section in sheet.Value.Sections)
        {
            foreach (var line in section.Lines)
            {
                samples.TryGetValue(line.PatientTestId, out var flags);
                SelectedTests.Add(new SelectedTestItem
                {
                    PatientTestId = line.PatientTestId,
                    TestId = 0,
                    TestName = line.TestName,
                    IsUrine = flags?.IsUrine ?? false,
                    IsStool = flags?.IsStool ?? false,
                    IsBlood = flags?.IsBlood ?? false,
                    IsSemen = flags?.IsSemen ?? false,
                    IsCsf = flags?.IsCsf ?? false,
                    IsTakenOutsideLab = flags?.IsTakenOutsideLab ?? false
                });
            }
        }
    }

    private void AddTestToSelection(int testId)
    {
        if (SelectedTests.Any(t => t.IsNew && t.TestId == testId))
        {
            return;
        }

        var catalog = CatalogTests.FirstOrDefault(t => t.Id == testId);
        if (catalog is null)
        {
            return;
        }

        SelectedTests.Add(new SelectedTestItem
        {
            PatientTestId = 0,
            TestId = catalog.Id,
            TestName = catalog.Name,
            Price = catalog.PatientPrice
        });
    }

    private async Task NewPatientAsync(CancellationToken cancellationToken = default)
    {
        ResetForm();
        await LoadCatalogAsync(cancellationToken);
        StatusMessage = string.Empty;
    }

    private void ResetForm()
    {
        _patientId = null;
        OnPropertyChanged(nameof(PatientIdText));
        IsEditMode = false;
        LabId = string.Empty;
        FullName = string.Empty;
        Title = null;
        IsMale = true;
        AgeValue = 0;
        AgeUnit = AgeUnit.Year;
        PhoneNumber = string.Empty;
        NationalId = null;
        Address = null;
        AccountType = AccountType.Individual;
        IsVip = false;
        TreatingDoctorIdText = null;
        TreatingDoctorName = null;
        ReferralEntityIdText = null;
        ReferralEntityName = null;
        IsFastingIndicated = false;
        FastingHoursText = null;
        RecentContrastImaging = false;
        Notes = null;
        PaymentAmountText = string.Empty;
        PaymentDiscountText = null;
        SelectedTests.Clear();
        VisitHistory.Clear();
        OnPropertyChanged(nameof(IsVisitHistoryVisible));
        OnPropertyChanged(nameof(ShowVisitHistoryEmpty));
        foreach (var condition in Conditions)
        {
            condition.IsSelected = false;
        }

        TotalCharged = 0;
        TotalDiscount = 0;
        TotalPaid = 0;
        Balance = 0;
        ErrorMessage = string.Empty;
    }

    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;

        if (!TryParseForm(out var form))
        {
            return;
        }

        IsBusy = true;
        try
        {
            if (!IsEditMode || !_patientId.HasValue)
            {
                await SaveNewAsync(form, cancellationToken);
            }
            else
            {
                await SaveEditAsync(form, _patientId.Value, cancellationToken);
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private sealed record ParsedForm(
        int? TreatingDoctorId,
        int? ReferralEntityId,
        int? FastingHours,
        IReadOnlyList<PatientNumberInput> PhoneNumbers,
        IReadOnlyList<int> MedicalConditionIds);

    private bool TryParseForm(out ParsedForm form)
    {
        int? treatingDoctorId = null;
        if (!string.IsNullOrWhiteSpace(TreatingDoctorIdText)
            && (!int.TryParse(TreatingDoctorIdText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var td) || td <= 0))
        {
            ErrorMessage = "رقم الطبيب المعالج غير صالح.";
            form = null!;
            return false;
        }

        treatingDoctorId = string.IsNullOrWhiteSpace(TreatingDoctorIdText)
            ? null
            : int.Parse(TreatingDoctorIdText, CultureInfo.InvariantCulture);

        int? referralEntityId = null;
        if (!string.IsNullOrWhiteSpace(ReferralEntityIdText)
            && (!int.TryParse(ReferralEntityIdText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var re) || re <= 0))
        {
            ErrorMessage = "رقم جهة الإحالة غير صالح.";
            form = null!;
            return false;
        }

        referralEntityId = string.IsNullOrWhiteSpace(ReferralEntityIdText)
            ? null
            : int.Parse(ReferralEntityIdText, CultureInfo.InvariantCulture);

        int? fastingHours = null;
        if (!string.IsNullOrWhiteSpace(FastingHoursText)
            && (!int.TryParse(FastingHoursText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var fh) || fh < 0))
        {
            ErrorMessage = "ساعات الصيام غير صالحة.";
            form = null!;
            return false;
        }

        fastingHours = string.IsNullOrWhiteSpace(FastingHoursText)
            ? null
            : int.Parse(FastingHoursText, CultureInfo.InvariantCulture);

        var phones = string.IsNullOrWhiteSpace(PhoneNumber)
            ? Array.Empty<PatientNumberInput>()
            : new[] { new PatientNumberInput(PhoneNumber.Trim(), 0) };

        form = new ParsedForm(
            treatingDoctorId,
            referralEntityId,
            fastingHours,
            phones,
            Conditions.Where(c => c.IsSelected).Select(c => c.MedicalConditionTypeId).ToList());
        return true;
    }

    private async Task SaveNewAsync(ParsedForm form, CancellationToken cancellationToken)
    {
        var tests = SelectedTests
            .Where(t => t.IsNew)
            .Select(t => new AddTestInput(t.TestId, t.IsUrine, t.IsStool, t.IsBlood, t.IsSemen, t.IsCsf, t.IsTakenOutsideLab))
            .ToList();

        var result = await _mediator.Send(new CreatePatientCommand(
            FullName,
            IsMale ? Sex.Male : Sex.Female,
            AgeValue,
            AgeUnit,
            DateTime.UtcNow,
            AccountType,
            IsVip,
            string.IsNullOrWhiteSpace(LabId) ? null : LabId.Trim(),
            string.IsNullOrWhiteSpace(Title) ? null : Title.Trim(),
            string.IsNullOrWhiteSpace(NationalId) ? null : NationalId.Trim(),
            string.IsNullOrWhiteSpace(Address) ? null : Address.Trim(),
            form.TreatingDoctorId,
            form.ReferralEntityId,
            null,
            IsFastingIndicated,
            form.FastingHours,
            RecentContrastImaging,
            string.IsNullOrWhiteSpace(Notes) ? null : Notes.Trim(),
            form.PhoneNumbers,
            form.MedicalConditionIds,
            tests), cancellationToken);

        if (!result.IsSuccess)
        {
            ErrorMessage = _presenter.Present(result.Error!);
            return;
        }

        _patientId = result.Value;
        OnPropertyChanged(nameof(PatientIdText));
        IsEditMode = true;
        await LoadVisitTestsAsync(cancellationToken);
        await RefreshBillingAsync(cancellationToken);
        StatusMessage = "تم حفظ بيانات المريض.";
    }

    private async Task SaveEditAsync(ParsedForm form, int patientId, CancellationToken cancellationToken)
    {
        var update = await _mediator.Send(new UpdatePatientCommand(
            patientId,
            FullName,
            IsMale ? Sex.Male : Sex.Female,
            AgeValue,
            AgeUnit,
            string.IsNullOrWhiteSpace(NationalId) ? null : NationalId.Trim(),
            string.IsNullOrWhiteSpace(Address) ? null : Address.Trim(),
            string.IsNullOrWhiteSpace(Title) ? null : Title.Trim(),
            IsVip,
            AccountType,
            string.IsNullOrWhiteSpace(Notes) ? null : Notes.Trim(),
            IsFastingIndicated,
            form.FastingHours,
            RecentContrastImaging,
            form.PhoneNumbers), cancellationToken);

        if (!update.IsSuccess)
        {
            ErrorMessage = _presenter.Present(update.Error!);
            return;
        }

        if (!await ApplyConditionDeltasAsync(patientId, form.MedicalConditionIds, cancellationToken))
        {
            return;
        }

        if (!await ApplyTestDeltasAsync(patientId, cancellationToken))
        {
            return;
        }

        await LoadVisitTestsAsync(cancellationToken);
        await RefreshBillingAsync(cancellationToken);
        StatusMessage = "تم حفظ التعديلات.";
    }

    private async Task<bool> ApplyConditionDeltasAsync(int patientId, IReadOnlyList<int> wanted, CancellationToken cancellationToken)
    {
        var detail = await _mediator.Send(new GetPatientByIdQuery(patientId), cancellationToken);
        if (!detail.IsSuccess)
        {
            ErrorMessage = _presenter.Present(detail.Error!);
            return false;
        }

        var current = detail.Value!.MedicalConditions.Select(m => m.MedicalConditionTypeId).ToHashSet();
        var target = wanted.ToHashSet();

        foreach (var typeId in target.Where(t => !current.Contains(t)))
        {
            var added = await _mediator.Send(new AddMedicalConditionCommand(patientId, typeId), cancellationToken);
            if (!added.IsSuccess)
            {
                ErrorMessage = _presenter.Present(added.Error!);
                return false;
            }
        }

        foreach (var typeId in current.Where(t => !target.Contains(t)))
        {
            var removed = await _mediator.Send(new RemoveMedicalConditionCommand(patientId, typeId), cancellationToken);
            if (!removed.IsSuccess)
            {
                ErrorMessage = _presenter.Present(removed.Error!);
                return false;
            }
        }

        return true;
    }

    private async Task<bool> ApplyTestDeltasAsync(int patientId, CancellationToken cancellationToken)
    {
        var sheet = await _mediator.Send(new GetVisitWorkSheetQuery(patientId), cancellationToken);
        if (!sheet.IsSuccess)
        {
            ErrorMessage = _presenter.Present(sheet.Error!);
            return false;
        }

        var persisted = sheet.Value!.Sections.SelectMany(s => s.Lines).ToDictionary(l => l.PatientTestId);
        var persistedSamples = sheet.Value.Samples.ToDictionary(s => s.PatientTestId);
        var currentIds = SelectedTests.Where(t => !t.IsNew).Select(t => t.PatientTestId).ToHashSet();

        foreach (var line in persisted.Values.Where(l => !currentIds.Contains(l.PatientTestId)))
        {
            var removed = await _mediator.Send(new RemoveTestFromVisitCommand(line.PatientTestId), cancellationToken);
            if (!removed.IsSuccess)
            {
                ErrorMessage = _presenter.Present(removed.Error!);
                return false;
            }
        }

        var added = SelectedTests.Where(t => t.IsNew).ToList();
        if (added.Count > 0)
        {
            var addResult = await _mediator.Send(new AddTestsToVisitCommand(
                patientId,
                added.Select(t => new AddTestInput(t.TestId, t.IsUrine, t.IsStool, t.IsBlood, t.IsSemen, t.IsCsf, t.IsTakenOutsideLab)).ToList()), cancellationToken);
            if (!addResult.IsSuccess)
            {
                ErrorMessage = _presenter.Present(addResult.Error!);
                return false;
            }
        }

        foreach (var item in SelectedTests.Where(t => !t.IsNew && persisted.ContainsKey(t.PatientTestId)))
        {
            persistedSamples.TryGetValue(item.PatientTestId, out var flags);
            if (flags is null
                || flags.IsUrine != item.IsUrine
                || flags.IsStool != item.IsStool
                || flags.IsBlood != item.IsBlood
                || flags.IsSemen != item.IsSemen
                || flags.IsCsf != item.IsCsf
                || flags.IsTakenOutsideLab != item.IsTakenOutsideLab)
            {
                var flagged = await _mediator.Send(new UpdatePatientTestSampleFlagsCommand(
                    item.PatientTestId, item.IsUrine, item.IsStool, item.IsBlood, item.IsSemen, item.IsCsf, item.IsTakenOutsideLab), cancellationToken);
                if (!flagged.IsSuccess)
                {
                    ErrorMessage = _presenter.Present(flagged.Error!);
                    return false;
                }
            }
        }

        return true;
    }

    private async Task DeleteAsync(CancellationToken cancellationToken = default)
    {
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;
        if (!IsEditMode || !_patientId.HasValue)
        {
            ErrorMessage = "لا يوجد مريض مسجل للحذف.";
            return;
        }

        var confirm = await _dialogs.ShowConfirmationAsync("حذف المريض", "سيتم حذف بيانات المريض (حذف منطقي). هل تريد المتابعة؟");
        if (!confirm)
        {
            return;
        }

        IsBusy = true;
        try
        {
            var result = await _mediator.Send(new SoftDeletePatientCommand(_patientId.Value), cancellationToken);
            if (!result.IsSuccess)
            {
                ErrorMessage = _presenter.Present(result.Error!);
                return;
            }

            ResetForm();
            await LoadCatalogAsync(cancellationToken);
            StatusMessage = "تم حذف المريض.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task UndoAsync(CancellationToken cancellationToken = default)
    {
        if (IsEditMode && _patientId.HasValue)
        {
            await LoadPatientAsync(_patientId.Value, cancellationToken);
        }
        else
        {
            ResetForm();
            await LoadCatalogAsync(cancellationToken);
        }
    }

    /// <summary>
    /// S-03 Slice 1: search-to-edit. Sends the raw term so backend validation
    /// messages («نص البحث…», «معاملات الترقيم…») surface verbatim via the presenter.
    /// </summary>
    private async Task SearchPatientsAsync(bool resetPage, CancellationToken cancellationToken, bool nextPage = false)
    {
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;

        if (resetPage)
        {
            SearchPage = 1;
        }
        else if (nextPage)
        {
            SearchPage += 1;
        }
        else
        {
            if (SearchPage <= 1)
            {
                return;
            }

            SearchPage -= 1;
        }

        IsBusy = true;
        try
        {
            var result = await _mediator.Send(new SearchPatientsQuery(SearchTerm, SearchPage, SearchPageSize), cancellationToken);
            if (!result.IsSuccess)
            {
                ErrorMessage = _presenter.Present(result.Error!);
                return;
            }

            SearchResults.Clear();
            foreach (var row in result.Value!)
            {
                SearchResults.Add(row);
            }

            SearchExecuted = true;
            OnPropertyChanged(nameof(ShowSearchEmpty));
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ClearSearch()
    {
        SearchTerm = string.Empty;
        SearchResults.Clear();
        SearchExecuted = false;
        SearchPage = 1;
        OnPropertyChanged(nameof(ShowSearchEmpty));
    }

    /// <summary>
    /// S-03 Slice 1 + D8: opening a result loads it via <c>LoadPatientAsync</c> only,
    /// discarding any unsaved edits immediately with no confirmation. Deleted rows
    /// are refused with the backend-verbatim «المريض محذوف.».
    /// </summary>
    private async Task OpenSearchResultAsync(PatientSummaryDto? row, CancellationToken cancellationToken)
    {
        if (row is null)
        {
            return;
        }

        if (row.IsDeleted)
        {
            ErrorMessage = "المريض محذوف.";
            return;
        }

        await LoadPatientAsync(row.PatientId, cancellationToken);
    }

    /// <summary>S-03 Slice 3: wires the orphaned AddProfileToVisitCommand (edit mode only).</summary>
    private async Task AddProfileAsync(CancellationToken cancellationToken)
    {
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;
        if (!IsEditMode || !_patientId.HasValue)
        {
            ErrorMessage = "احفظ بيانات المريض أولًا قبل إضافة بروفايل.";
            return;
        }

        if (SelectedProfile is null)
        {
            ErrorMessage = "اختر بروفايل أولًا.";
            return;
        }

        IsBusy = true;
        try
        {
            var result = await _mediator.Send(new AddProfileToVisitCommand(
                PatientId: _patientId.Value,
                ProfileId: SelectedProfile.ProfileId,
                IsUrine: ProfileIsUrine,
                IsStool: ProfileIsStool,
                IsBlood: ProfileIsBlood,
                IsSemen: ProfileIsSemen,
                IsCsf: ProfileIsCsf,
                IsTakenOutsideLab: ProfileIsTakenOutsideLab), cancellationToken);
            if (!result.IsSuccess)
            {
                ErrorMessage = _presenter.Present(result.Error!);
                return;
            }

            await LoadVisitTestsAsync(cancellationToken);
            await RefreshBillingAsync(cancellationToken);
            StatusMessage = "تمت إضافة البروفايل.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>S-03 Slice 3: wires the orphaned AddCustomGroupToVisitCommand (edit mode only).</summary>
    private async Task AddCustomGroupAsync(CancellationToken cancellationToken)
    {
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;
        if (!IsEditMode || !_patientId.HasValue)
        {
            ErrorMessage = "احفظ بيانات المريض أولًا قبل إضافة مجموعة.";
            return;
        }

        if (SelectedCustomGroup is null)
        {
            ErrorMessage = "اختر مجموعة أولًا.";
            return;
        }

        IsBusy = true;
        try
        {
            var result = await _mediator.Send(new AddCustomGroupToVisitCommand(_patientId.Value, SelectedCustomGroup.Id), cancellationToken);
            if (!result.IsSuccess)
            {
                ErrorMessage = _presenter.Present(result.Error!);
                return;
            }

            await LoadVisitTestsAsync(cancellationToken);
            await RefreshBillingAsync(cancellationToken);
            StatusMessage = "تمت إضافة المجموعة المخصصة.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>S-03 Slice 3: wires the orphaned ClearAllTestsCommand (edit mode only, confirmed).</summary>
    private async Task ClearAllVisitTestsAsync(CancellationToken cancellationToken)
    {
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;
        if (!IsEditMode || !_patientId.HasValue)
        {
            ErrorMessage = "احفظ بيانات المريض أولًا قبل مسح التحاليل.";
            return;
        }

        var confirm = await _dialogs.ShowConfirmationAsync("مسح التحاليل", "سيتم مسح جميع تحاليل هذه الزيارة. هل تريد المتابعة؟");
        if (!confirm)
        {
            return;
        }

        IsBusy = true;
        try
        {
            var result = await _mediator.Send(new ClearAllTestsCommand(_patientId.Value), cancellationToken);
            if (!result.IsSuccess)
            {
                ErrorMessage = _presenter.Present(result.Error!);
                return;
            }

            await LoadVisitTestsAsync(cancellationToken);
            await RefreshBillingAsync(cancellationToken);
            StatusMessage = "تم مسح جميع التحاليل.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task RecordPaymentAsync(CancellationToken cancellationToken)
    {
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;
        if (!IsEditMode || !_patientId.HasValue)
        {
            ErrorMessage = "احفظ بيانات المريض أولًا قبل تسجيل مدفوعات.";
            return;
        }

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
            var result = await _mediator.Send(new RecordPaymentCommand(_patientId.Value, amount, discount), cancellationToken);
            if (!result.IsSuccess)
            {
                ErrorMessage = _presenter.Present(result.Error!);
                return;
            }

            PaymentAmountText = string.Empty;
            PaymentDiscountText = null;
            await RefreshBillingAsync(cancellationToken);
            StatusMessage = "تم تسجيل المدفوع.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SettleAsync(CancellationToken cancellationToken)
    {
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;
        if (!IsEditMode || !_patientId.HasValue)
        {
            ErrorMessage = "احفظ بيانات المريض أولًا قبل التسوية.";
            return;
        }

        IsBusy = true;
        try
        {
            var result = await _mediator.Send(new SettleAccountInFullCommand(_patientId.Value), cancellationToken);
            if (!result.IsSuccess)
            {
                ErrorMessage = _presenter.Present(result.Error!);
                return;
            }

            await RefreshBillingAsync(cancellationToken);
            StatusMessage = "تمت تسوية الحساب بالكامل.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task PrintAsync(CancellationToken cancellationToken, string document)
    {
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;
        if (!IsEditMode || !_patientId.HasValue)
        {
            ErrorMessage = "احفظ بيانات المريض أولًا قبل الطباعة.";
            return;
        }

        IsBusy = true;
        try
        {
            if (document == "invoice")
            {
                var invoice = await _mediator.Send(new PrintInvoiceCommand(_patientId.Value), cancellationToken);
                if (!invoice.IsSuccess)
                {
                    ErrorMessage = _presenter.Present(invoice.Error!);
                    return;
                }
            }
            else
            {
                Result result = document switch
                {
                    "barcode" => await _mediator.Send(new PrintBarcodeCommand(_patientId.Value), cancellationToken),
                    "worksheet" => await _mediator.Send(new PrintWorkSheetCommand(_patientId.Value), cancellationToken),
                    "receipt" => await _mediator.Send(new PrintReceiptCommand(_patientId.Value), cancellationToken),
                    _ => throw new InvalidOperationException($"Unknown print document '{document}'.")
                };

                if (!result.IsSuccess)
                {
                    ErrorMessage = _presenter.Present(result.Error!);
                    return;
                }
            }

            StatusMessage = "تم إرسال المستند إلى الطابعة.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task RefreshBillingAsync(CancellationToken cancellationToken)
    {
        TotalCharged = 0;
        TotalDiscount = 0;
        TotalPaid = 0;
        Balance = 0;
        if (!_patientId.HasValue)
        {
            return;
        }

        var account = await _mediator.Send(new GetPatientAccountQuery(_patientId.Value), cancellationToken);
        if (!account.IsSuccess)
        {
            ErrorMessage = _presenter.Present(account.Error!);
            return;
        }

        TotalCharged = account.Value!.TotalCharged;
        TotalDiscount = account.Value.TotalDiscount;
        TotalPaid = account.Value.TotalPaid;
        Balance = account.Value.Balance;
    }
}
