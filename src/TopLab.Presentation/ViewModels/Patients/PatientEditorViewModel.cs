using System.Collections.ObjectModel;
using System.Globalization;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.AnalyteProfiles.Common;
using TopLab.Application.Features.AnalyteProfiles.Queries.GetProfileDefinitions;
using TopLab.Application.Features.PatientBilling.Commands.ClearPatientAccount;
using TopLab.Application.Features.PatientBilling.Commands.PrintInvoice;
using TopLab.Application.Features.PatientBilling.Commands.PrintReceipt;
using TopLab.Application.Features.PatientBilling.Commands.RecordExtraCharge;
using TopLab.Application.Features.PatientBilling.Commands.RecordPayment;
using TopLab.Application.Features.PatientBilling.Commands.RecordRefund;
using TopLab.Application.Features.PatientBilling.Commands.SettleAccountInFull;
using TopLab.Application.Features.PatientBilling.Queries.GetPatientAccount;
using TopLab.Application.Features.PatientEnvelope.Commands.PrintEnvelope;
using TopLab.Application.Features.PatientEnvelope.Commands.PrintLabOrder;
using TopLab.Application.Features.PatientRegistration.Commands.AddCustomGroupToVisit;
using TopLab.Application.Features.PatientRegistration.Commands.SetVisitFollowUpFlag;
using TopLab.Application.Features.PatientRegistration.Commands.ApplyConditionDeltas;
using TopLab.Application.Features.PatientRegistration.Commands.ApplyVisitDeltas;
using TopLab.Application.Features.PatientRegistration.Commands.AddMedicalCondition;
using TopLab.Application.Features.PatientRegistration.Commands.AddProfileToVisit;
using TopLab.Application.Features.PatientRegistration.Commands.AddTestGroupToVisit;
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
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Queries.GetCustomGroupTestIds;
using TopLab.Application.Features.PatientRegistration.Queries.GetRegistrationCatalog;
using TopLab.Application.Features.PatientRegistration.Queries.SearchPatients;
using TopLab.Application.Features.SystemAndPrintSettings.Commands.UpdatePrintResultPassword;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Common;
using TopLab.Application.Features.WorkSheets.Commands.PrintWorkSheet;
using TopLab.Application.Features.WorkSheets.Queries.GetVisitWorkSheet;
using TopLab.Domain.Billing;
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
    private readonly INavigationService _navigation;

    private bool _isEditMode;
    private bool _isFollowUpRequired;
    private int? _patientId;
    private bool _canClearAllVisitTests;
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
    /// <summary>The date-time formats the registration header accepts (STAGE-01).</summary>
    private static readonly string[] DateTimeFormats =
    {
        "yyyy-MM-dd HH:mm",
        "yyyy-MM-dd HH:mm:ss",
        "yyyy/MM/dd HH:mm",
        "yyyy/MM/dd HH:mm:ss",
        "yyyy-MM-dd",
        "yyyy/MM/dd"
    };

    private bool _isFastingIndicated;
    private string? _fastingHoursText;
    private bool _recentContrastImaging;
    private bool _recentUltrasoundImaging;
    private string? _cardNumber;
    private string _registrationDateText = string.Empty;
    private string _pickupDateText = string.Empty;
    private bool _printResultPasswordEnabled;
    private string? _printResultPasswordText;
    private string? _notes;
    private string _paymentAmountText = string.Empty;
    private string? _paymentDiscountText;
    private string? _paymentPercentageText;
    private string _refundAmountText = string.Empty;
    private readonly AccountEntrySession _accountEntry = new();
    private string _catalogFilterText = string.Empty;
    private CatalogFilterMode _catalogFilterMode = CatalogFilterMode.Tg;
    private HashSet<int> _customGroupTestIds = new();
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
    private TestGroupDto? _selectedTestGroup;

    /// <summary>STAGE-01: the pregnancy question row (baseline 1.2.2.3.9).</summary>
    private ConditionItem? _pregnancyItem;

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
        _navigation = navigation;

        NewPatientCommand = new AsyncRelayCommand(async (_, ct) => await NewPatientAsync());
        SaveCommand = new AsyncRelayCommand(async (_, ct) => await SaveAsync(ct));
        DeleteCommand = new AsyncRelayCommand(async (_, ct) => await DeleteAsync());
        UndoCommand = new AsyncRelayCommand(async (_, ct) => await UndoAsync());
        RecordPaymentCommand = new AsyncRelayCommand(async (_, ct) => await RecordPaymentAsync(ct));
        SettleCommand = new AsyncRelayCommand(async (_, ct) => await SettleAsync(ct));
        AccountEntryEnterCommand = new AsyncRelayCommand(async (_, ct) => await AccountEntryEnterAsync(ct));
        RefundCommand = new AsyncRelayCommand(async (_, ct) => await RefundAsync(ct));
        ClearAccountCommand = new AsyncRelayCommand(async (_, ct) => await ClearAccountAsync(ct));
        PrintBarcodeCommand = new AsyncRelayCommand(async (_, ct) => await PrintAsync(ct, "barcode"));
        PrintWorkSheetCommand = new AsyncRelayCommand(async (_, ct) => await PrintAsync(ct, "worksheet"));
        PrintReceiptCommand = new AsyncRelayCommand(async (_, ct) => await PrintAsync(ct, "receipt"));
        PrintInvoiceCommand = new AsyncRelayCommand(async (_, ct) => await PrintAsync(ct, "invoice"));
        PrintLabOrderCommand = new AsyncRelayCommand(async (_, ct) => await PrintAsync(ct, "laborder"));
        PrintEnvelopeCommand = new AsyncRelayCommand(async (_, ct) => await PrintEnvelopeAsync(ct));
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
        ShowTreatingDoctorDetailsCommand = new AsyncRelayCommand(_ => ShowEntityDetailsAsync(TreatingDoctorIdText, "الطبيب المعالج"));
        ShowReferralEntityDetailsCommand = new AsyncRelayCommand(_ => ShowEntityDetailsAsync(ReferralEntityIdText, "جهة الإحالة"));
        SavePrintResultPasswordCommand = new AsyncRelayCommand(async (param, ct) => await SavePrintResultPasswordAsync(ct));
        SearchPatientsCommand = new AsyncRelayCommand(async (_, ct) => await SearchPatientsAsync(resetPage: true, cancellationToken: ct));
        NextSearchPageCommand = new AsyncRelayCommand(async (_, ct) => await SearchPatientsAsync(resetPage: false, cancellationToken: ct, nextPage: true));
        PrevSearchPageCommand = new AsyncRelayCommand(async (_, ct) => await SearchPatientsAsync(resetPage: false, cancellationToken: ct));
        ClearSearchCommand = new RelayCommand(ClearSearch);
        ClearCatalogFilterCommand = new RelayCommand(_ => CatalogFilterText = string.Empty);
        SetTmModeCommand = new RelayCommand(_ => CatalogFilterMode = CatalogFilterMode.Tm);
        SetTgModeCommand = new RelayCommand(_ => CatalogFilterMode = CatalogFilterMode.Tg);
        SetCgModeCommand = new RelayCommand(_ => CatalogFilterMode = CatalogFilterMode.Cg);
        OpenSearchResultCommand = new AsyncRelayCommand(async (param, ct) => await OpenSearchResultAsync(param as PatientSummaryDto, ct));
        OpenAccountCommand = new AsyncRelayCommand(async (_, ct) => await OpenAccountAsync(ct));
        PatientWindowCommand = new AsyncRelayCommand((_, ct) => PatientWindowAsync(ct));
        LabResultsCommand = new AsyncRelayCommand(async (_, ct) => await LabResultsAsync(ct));
        FollowUpCommand = new AsyncRelayCommand(async (_, ct) => await FollowUpAsync(ct));
        AddProfileCommand = new AsyncRelayCommand(async (_, ct) => await AddProfileAsync(ct));
        AddCustomGroupCommand = new AsyncRelayCommand(async (_, ct) => await AddCustomGroupAsync(ct));
        AddTestGroupCommand = new AsyncRelayCommand(async (_, ct) => await AddTestGroupAsync(ct));
        ClearAllVisitTestsCommand = new AsyncRelayCommand(async (_, ct) => await ClearAllVisitTestsAsync(ct));
        ToggleFollowUpFlagCommand = new AsyncRelayCommand(async (_, ct) => await ToggleFollowUpFlagAsync(ct));

        // S-03 Slice 0: empty-state visibility follows the S-01/S-02 Show*Empty idiom.
        SelectedTests.CollectionChanged += (_, _) =>
        {
            RefreshEmptyStates();
            RefreshSampleTypeSummary();
        };
        CatalogTests.CollectionChanged += (_, _) => RefreshEmptyStates();
        SearchResults.CollectionChanged += (_, _) => OnPropertyChanged(nameof(ShowSearchEmpty));
        // VisitHistory is populated AFTER LoadVisitTestsAsync (:567 then :569), so the
        // button must also re-evaluate when history arrives, not only when tests load.
        VisitHistory.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(ShowVisitHistoryEmpty));
            RefreshCanClearAllVisitTests();
        };
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

        /// <summary>STAGE-01: the category decides the question's group and the pregnancy rule.</summary>
        public MedicalConditionCategory Category { get; init; }

        /// <summary>STAGE-01: seeded rows belong to the migration's snapshot and are read-only.</summary>
        public bool IsSeeded { get; init; }

        public bool IsSelected { get => _isSelected; set => SetProperty(ref _isSelected, value); }
    }

    /// <summary>
    /// The full catalogue as loaded (1.2.2.5.1.1 — the master test list). The filtered view
    /// below is what the ListView binds to; this collection is never narrowed so clearing the
    /// filter restores every test without a round trip.
    /// </summary>
    public ObservableCollection<TestSummaryDto> CatalogTests { get; } = new();

    /// <summary>
    /// 1.2.2.5.1.1 «start» — the test filter field of the master test list. A non-empty value
    /// narrows <see cref="FilteredCatalogTests"/> by test name OR test code, case-insensitively.
    /// </summary>
    public string CatalogFilterText
    {
        get => _catalogFilterText;
        set
        {
            if (SetProperty(ref _catalogFilterText, value))
            {
                ApplyCatalogFilter();
            }
        }
    }

    /// <summary>The catalogue rows the screen shows: all tests, or those matching the filter.</summary>
    public ObservableCollection<TestSummaryDto> FilteredCatalogTests { get; } = new();

    /// <summary>True when a filter is active and it matched nothing (distinct from an empty catalogue).</summary>
    public bool ShowCatalogFilterNoMatch
        => _catalogFilterMode == CatalogFilterMode.Tg
           && !string.IsNullOrWhiteSpace(_catalogFilterText)
           && FilteredCatalogTests.Count == 0;

    /// <summary>
    /// D-02: the active catalogue mode. Exactly one mode is active at any time, and an undefined
    /// value is REJECTED with the property left unchanged (GAP-11 acceptance criterion:
    /// "mode options are mutually exclusive; invalid mode is rejected").
    /// </summary>
    public CatalogFilterMode CatalogFilterMode
    {
        get => _catalogFilterMode;
        set
        {
            if (!Enum.IsDefined(typeof(CatalogFilterMode), value))
            {
                // Rejected: state unchanged. The setter re-raises so a bound control snaps back.
                OnPropertyChanged(nameof(CatalogFilterMode));
                OnPropertyChanged(nameof(IsTmMode));
                OnPropertyChanged(nameof(IsTgMode));
                OnPropertyChanged(nameof(IsCgMode));
                return;
            }

            if (_catalogFilterMode == value)
            {
                return;
            }

            var leavingTg = _catalogFilterMode == CatalogFilterMode.Tg;
            _catalogFilterMode = value;

            // Design detail (D-02): the search box is active in TG only, so leaving TG clears the
            // text — no hidden state survives the switch.
            if (leavingTg)
            {
                _catalogFilterText = string.Empty;
                OnPropertyChanged(nameof(CatalogFilterText));
            }

            OnPropertyChanged(nameof(CatalogFilterMode));
            OnPropertyChanged(nameof(IsTmMode));
            OnPropertyChanged(nameof(IsTgMode));
            OnPropertyChanged(nameof(IsCgMode));
            OnPropertyChanged(nameof(IsCatalogSearchEnabled));

            ApplyCatalogFilter();
        }
    }

    /// <summary>TM «كل التحاليل».</summary>
    public bool IsTmMode
    {
        get => _catalogFilterMode == CatalogFilterMode.Tm;
        set { if (value) CatalogFilterMode = CatalogFilterMode.Tm; }
    }

    /// <summary>TG «بحث بالاسم أو الكود».</summary>
    public bool IsTgMode
    {
        get => _catalogFilterMode == CatalogFilterMode.Tg;
        set { if (value) CatalogFilterMode = CatalogFilterMode.Tg; }
    }

    /// <summary>CG «مجموعاتي المخصصة».</summary>
    public bool IsCgMode
    {
        get => _catalogFilterMode == CatalogFilterMode.Cg;
        set { if (value) CatalogFilterMode = CatalogFilterMode.Cg; }
    }

    /// <summary>D-02: the search box is enabled in TG only; its text is ignored in TM and CG.</summary>
    public bool IsCatalogSearchEnabled => _catalogFilterMode == CatalogFilterMode.Tg;

    /// <summary>TM / TG / CG commands (bound to the three buttons; no code-behind).</summary>
    public RelayCommand SetTmModeCommand { get; private set; } = null!;

    public RelayCommand SetTgModeCommand { get; private set; } = null!;

    public RelayCommand SetCgModeCommand { get; private set; } = null!;

    public ObservableCollection<TestGroupDto> TestGroups { get; } = new();

    public ObservableCollection<string> Titles { get; } = new();

    public ObservableCollection<ConditionItem> Conditions { get; } = new();

    /// <summary>
    /// STAGE-01 (baseline 1.2.2.3.3–1.2.2.3.9): the medication toggles and the
    /// pregnancy toggle — the questions that precede the imaging question.
    /// </summary>
    public ObservableCollection<ConditionItem> MedicationConditions { get; } = new();

    /// <summary>
    /// STAGE-01 (baseline 1.2.2.3.11–1.2.2.3.15): the condition toggles — the
    /// questions that follow the imaging question.
    /// </summary>
    public ObservableCollection<ConditionItem> ConditionToggles { get; } = new();

    public ObservableCollection<SelectedTestItem> SelectedTests { get; } = new();

    public IReadOnlyList<AgeUnit> AgeUnits { get; } =
        Enum.GetValues<AgeUnit>();

    public IReadOnlyList<AccountType> AccountTypes { get; } =
        Enum.GetValues<AccountType>();

    public bool IsEditMode { get => _isEditMode; private set => SetProperty(ref _isEditMode, value); }

    /// <summary>
    /// Durable visit follow-up flag (baseline 1.2.2.6.10). Reflected from the loaded
    /// visit; set by <see cref="ToggleFollowUpFlagCommand"/> and by <see cref="LoadPatientAsync"/>.
    /// </summary>
    public bool IsFollowUpRequired { get => _isFollowUpRequired; private set => SetProperty(ref _isFollowUpRequired, value); }

    /// <summary>
    /// BR-A04-5 / SD-18: true only when the Application guard would pass — i.e. this visit
    /// IS the patient's first registration (BR-A04-1). The Application handler remains the
    /// authority (acceptance criterion 5); this only drives the button's enabled state so the
    /// operator is not led into a refusal.
    ///
    /// <para>
    /// Computed from <see cref="VisitHistory"/>, which is exactly the <c>LabId</c> group
    /// (all non-deleted visits sharing this visit's LabId, or the single visit when LabId is
    /// null) and includes the current visit. A visit is first when NO other entry in that
    /// group sorts before it on (RegistrationDateUtc, PatientId) — the same two-key rule the
    /// handler applies (BR-A04-1, tie broken by the lower PatientId).
    /// </para>
    /// </summary>
    public bool CanClearAllVisitTests
    {
        get => _canClearAllVisitTests;
        private set => SetProperty(ref _canClearAllVisitTests, value);
    }

    private void RefreshCanClearAllVisitTests()
    {
        var can = false;

        if (IsEditMode && _patientId.HasValue)
        {
            var current = VisitHistory.FirstOrDefault(v => v.PatientId == _patientId.Value);
            can = current is null
                || !VisitHistory.Any(v => v.PatientId != _patientId.Value
                    && (v.RegistrationDateUtc < current.RegistrationDateUtc
                        || (v.RegistrationDateUtc == current.RegistrationDateUtc
                            && v.PatientId < current.PatientId)));
        }

        CanClearAllVisitTests = can;
    }

    public string PatientIdText => _patientId?.ToString(CultureInfo.InvariantCulture) ?? "جديد";

    public string LabId { get => _labId; set => SetProperty(ref _labId, value); }

    public string FullName { get => _fullName; set => SetProperty(ref _fullName, value); }

    public string? Title { get => _title; set => SetProperty(ref _title, value); }

    public bool IsMale
    {
        get => _isMale;
        set
        {
            if (SetProperty(ref _isMale, value))
            {
                // STAGE-01 (baseline 1.2.2.3.9): the pregnancy question is offered
                // to female patients only; a male patient never carries the answer.
                ApplyPregnancyVisibility();
            }
        }
    }

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

    /// <summary>STAGE-01 (baseline 1.2.2.2.11.1): the treating doctor's additional information.</summary>
    public AsyncRelayCommand ShowTreatingDoctorDetailsCommand { get; }

    /// <summary>STAGE-01 (baseline 1.2.2.2.10.1): the referring entity's additional information.</summary>
    public AsyncRelayCommand ShowReferralEntityDetailsCommand { get; }

    /// <summary>STAGE-01 (baseline 1.2.2.4.2 / 1.2.2.4.2.1): stores the print-result password.</summary>
    public AsyncRelayCommand SavePrintResultPasswordCommand { get; }

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

    /// <summary>
    /// STAGE-01 (baseline 1.2.2.2.10.1 / 1.2.2.2.11.1): opens the selected
    /// entity's additional information in a read-only window.
    /// </summary>
    private async Task ShowEntityDetailsAsync(string? entityIdText, string entityLabel)
    {
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(entityIdText)
            || !int.TryParse(entityIdText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var entityId)
            || entityId <= 0)
        {
            ErrorMessage = $"اختر {entityLabel} أولًا لعرض معلوماته.";
            return;
        }

        var vm = _services.GetRequiredService<ViewModels.External.ExternalEntityDetailsViewModel>();
        await vm.LoadAsync(entityId);
        var window = new Views.External.ExternalEntityDetailsWindow(vm)
        {
            Owner = System.Windows.Application.Current?.MainWindow
        };
        window.ShowDialog();
    }

    /// <summary>
    /// STAGE-01 (baseline 1.2.2.4.2 / 1.2.2.4.2.1): stores the print-result
    /// password. The command hashes it; the plain value is dropped afterwards.
    /// </summary>
    private async Task SavePrintResultPasswordAsync(CancellationToken cancellationToken)
    {
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;

        var result = await _mediator.Send(
            new UpdatePrintResultPasswordCommand(PrintResultPasswordEnabled, PrintResultPasswordText),
            cancellationToken);

        if (!result.IsSuccess)
        {
            ErrorMessage = result.Error is not null ? _presenter.Present(result.Error) : ErrorMessage;
            return;
        }

        PrintResultPasswordText = null;
        StatusMessage = "تم حفظ كلمة مرور الطباعة.";
    }

    public bool IsFastingIndicated { get => _isFastingIndicated; set => SetProperty(ref _isFastingIndicated, value); }

    public string? FastingHoursText { get => _fastingHoursText; set => SetProperty(ref _fastingHoursText, value); }

    public bool RecentContrastImaging { get => _recentContrastImaging; set => SetProperty(ref _recentContrastImaging, value); }

    /// <summary>
    /// STAGE-01 (baseline 1.2.2.3.10): the ultrasound half of the single imaging
    /// question. The question is answered when either imaging flag is set.
    /// </summary>
    public bool RecentUltrasoundImaging { get => _recentUltrasoundImaging; set => SetProperty(ref _recentUltrasoundImaging, value); }

    /// <summary>STAGE-01 (baseline 1.2.2.2.6): the patient's card / file number.</summary>
    public string? CardNumber { get => _cardNumber; set => SetProperty(ref _cardNumber, value); }

    /// <summary>STAGE-01 (baseline 1.2.2.1.1): the entry date, with its time.</summary>
    public string RegistrationDateText { get => _registrationDateText; set => SetProperty(ref _registrationDateText, value); }

    /// <summary>STAGE-01 (baseline 1.2.2.1.2): the receipt date, with its time.</summary>
    public string PickupDateText { get => _pickupDateText; set => SetProperty(ref _pickupDateText, value); }

    /// <summary>
    /// STAGE-01 (baseline 1.2.2.2.8): a read-only summary of the sample types
    /// set on the visit's registered tests. The per-test control is the only
    /// editable source (VERIFY #3).
    /// </summary>
    public string SampleTypeSummary
    {
        get
        {
            var names = new List<string>(5);

            if (SelectedTests.Any(t => t.IsUrine))
            {
                names.Add("بول");
            }

            if (SelectedTests.Any(t => t.IsStool))
            {
                names.Add("براز");
            }

            if (SelectedTests.Any(t => t.IsBlood))
            {
                names.Add("دم");
            }

            if (SelectedTests.Any(t => t.IsSemen))
            {
                names.Add("سائل منوي");
            }

            if (SelectedTests.Any(t => t.IsCsf))
            {
                names.Add("سائل نخاعي");
            }

            return names.Count == 0 ? "—" : string.Join("، ", names);
        }
    }

    /// <summary>STAGE-01 (baseline 1.2.2.4.2.1): the print-result password requirement.</summary>
    public bool PrintResultPasswordEnabled
    {
        get => _printResultPasswordEnabled;
        set => SetProperty(ref _printResultPasswordEnabled, value);
    }

    /// <summary>The plain password — it is hashed by the command and never stored.</summary>
    public string? PrintResultPasswordText
    {
        get => _printResultPasswordText;
        set => SetProperty(ref _printResultPasswordText, value);
    }

    public string? Notes { get => _notes; set => SetProperty(ref _notes, value); }

    public string PaymentAmountText
    {
        get => _paymentAmountText;
        set
        {
            if (SetProperty(ref _paymentAmountText, value))
            {
                _accountEntry.AmountText = value;
            }
        }
    }

    public string? PaymentDiscountText
    {
        get => _paymentDiscountText;
        set
        {
            if (SetProperty(ref _paymentDiscountText, value))
            {
                // 1.3.1.2 / 1.3.1.3: the value and the percentage are never applied together.
                _accountEntry.DiscountText = value;
                PaymentPercentageText = null;
            }
        }
    }

    /// <summary>1.3.1.3 «%»: converted to an amount against the patient's TOTAL account amount.</summary>
    public string? PaymentPercentageText
    {
        get => _paymentPercentageText;
        set
        {
            if (SetProperty(ref _paymentPercentageText, value))
            {
                _accountEntry.PercentageText = value;
            }
        }
    }

    /// <summary>1.3.2.5 «استرداد» amount entry.</summary>
    public string RefundAmountText { get => _refundAmountText; set => SetProperty(ref _refundAmountText, value); }

    public decimal TotalCharged { get => _totalCharged; private set => SetProperty(ref _totalCharged, value); }

    public decimal TotalDiscount { get => _totalDiscount; private set => SetProperty(ref _totalDiscount, value); }

    public decimal TotalPaid { get => _totalPaid; private set => SetProperty(ref _totalPaid, value); }

    public decimal Balance { get => _balance; private set => SetProperty(ref _balance, value); }

    public bool IsBusy { get => _isBusy; private set => SetProperty(ref _isBusy, value); }

    /// <summary>S-03 Slice 0: empty-state flags (S-01/S-02 Show*Empty idiom).</summary>
    public bool ShowSelectedTestsEmpty => SelectedTests.Count == 0;

    /// <summary>
    /// S-03 Slice 0: empty-state flags (S-01/S-02 Show*Empty idiom).
    /// D-02: this means "the catalogue itself is empty" — it is deliberately based on the FULL
    /// set, so a mode that hides rows (CG with no groups, TG with no match) does not claim the
    /// catalogue is empty. Each mode has its own message for that.
    /// </summary>
    public bool ShowCatalogEmpty => CatalogTests.Count == 0;

    private void RefreshEmptyStates()
    {
        OnPropertyChanged(nameof(ShowSelectedTestsEmpty));
        OnPropertyChanged(nameof(ShowCatalogEmpty));
        OnPropertyChanged(nameof(ShowCatalogFilterNoMatch));
    }

    /// <summary>
    /// 1.2.2.5.1.1: rebuilds <see cref="FilteredCatalogTests"/> from
    /// <see cref="CatalogTests"/> using the current filter text. Matching is on the test name
    /// OR the test code, case-insensitively, so both "CBC" and "صورة دم" find the same test.
    /// An empty or whitespace filter shows the whole catalogue.
    /// </summary>
    private void ApplyCatalogFilter()
    {
        // D-02: the search text only takes effect in TG. In TM and CG it is ignored, so the
        // modes stay mutually exclusive even if a stale text survived somewhere.
        var needle = _catalogFilterMode == CatalogFilterMode.Tg
            ? (_catalogFilterText ?? string.Empty).Trim()
            : string.Empty;

        FilteredCatalogTests.Clear();
        foreach (var test in CatalogTests)
        {
            var visible = _catalogFilterMode switch
            {
                // TM: every test the catalogue already shows — the same base set as before.
                CatalogFilterMode.Tm => true,
                // TG: the search box; an empty box means all tests.
                CatalogFilterMode.Tg => needle.Length == 0 || MatchesCatalogFilter(test, needle),
                // CG: only tests belonging to at least one custom group.
                CatalogFilterMode.Cg => _customGroupTestIds.Contains(test.Id),
                _ => true
            };

            if (visible)
            {
                FilteredCatalogTests.Add(test);
            }
        }

        OnPropertyChanged(nameof(ShowCatalogEmpty));
        OnPropertyChanged(nameof(ShowCatalogFilterNoMatch));
    }

    /// <summary>Name OR code, case-insensitive, substring match.</summary>
    private static bool MatchesCatalogFilter(TestSummaryDto test, string needle)
        => test.Name.Contains(needle, StringComparison.OrdinalIgnoreCase)
           || test.TestCode.Contains(needle, StringComparison.OrdinalIgnoreCase);

    /// <summary>Clears the filter (the «مسح» button next to the filter field).</summary>
    public RelayCommand ClearCatalogFilterCommand { get; }

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
    public TestGroupDto? SelectedTestGroup { get => _selectedTestGroup; set => SetProperty(ref _selectedTestGroup, value); }

    public AsyncRelayCommand NewPatientCommand { get; }

    public AsyncRelayCommand SaveCommand { get; }

    public AsyncRelayCommand DeleteCommand { get; }

    public AsyncRelayCommand UndoCommand { get; }

    public AsyncRelayCommand RecordPaymentCommand { get; }

    public AsyncRelayCommand SettleCommand { get; }

    /// <summary>1.3.1.4.2: the account panel's <c>Enter</c> key (first press computes, second saves).</summary>
    public AsyncRelayCommand AccountEntryEnterCommand { get; }

    /// <summary>1.3.2.5 «استرداد».</summary>
    public AsyncRelayCommand RefundCommand { get; }

    /// <summary>1.3.2.7 «تصفير الحساب».</summary>
    public AsyncRelayCommand ClearAccountCommand { get; }

    public AsyncRelayCommand PrintBarcodeCommand { get; }

    public AsyncRelayCommand PrintWorkSheetCommand { get; }

    public AsyncRelayCommand PrintReceiptCommand { get; }

    public AsyncRelayCommand PrintInvoiceCommand { get; }

    /// <summary>Phase 1 REF-068: laboratory-order slip print action.</summary>
    public AsyncRelayCommand PrintLabOrderCommand { get; }
    public AsyncRelayCommand PrintEnvelopeCommand { get; }

    public RelayCommand AddTestToSelectionCommand { get; }

    public RelayCommand RemoveSelectedTestCommand { get; }

    public AsyncRelayCommand SearchPatientsCommand { get; }

    public AsyncRelayCommand NextSearchPageCommand { get; }

    public AsyncRelayCommand PrevSearchPageCommand { get; }

    public RelayCommand ClearSearchCommand { get; }

    public AsyncRelayCommand OpenSearchResultCommand { get; }

    /// <summary>S-03 Slice 4: totals-row entry into the patient account screen.</summary>
    public AsyncRelayCommand OpenAccountCommand { get; }
    public AsyncRelayCommand PatientWindowCommand { get; }
    public AsyncRelayCommand LabResultsCommand { get; }

    /// <summary>
    /// 1.2.2.6.10 «Follow-up»: opens the case follow-up screen — reference section 1.12
    /// "Case Follow-up and Audit Trail", screen 1.12.1 "Case History Screen", which this
    /// codebase implements as <see cref="TopLab.Presentation.ViewModels.Audit.AuditViewModel"/>
    /// (its P tab is the per-patient case history).
    /// </summary>
    public AsyncRelayCommand FollowUpCommand { get; }

    public AsyncRelayCommand ToggleFollowUpFlagCommand { get; }

    public AsyncRelayCommand AddProfileCommand { get; }

    public AsyncRelayCommand AddCustomGroupCommand { get; }
    public AsyncRelayCommand AddTestGroupCommand { get; }

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

            ApplyCatalogFilter();

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
            MedicationConditions.Clear();
            ConditionToggles.Clear();
            _pregnancyItem = null;

            foreach (var condition in catalog.Value.MedicalConditionTypes)
            {
                var item = new ConditionItem
                {
                    MedicalConditionTypeId = condition.MedicalConditionTypeId,
                    Name = condition.Name,
                    Category = condition.Category,
                    IsSeeded = MedicalConditionCatalogue.IsSeeded(condition.MedicalConditionTypeId)
                };

                Conditions.Add(item);

                if (condition.Category == MedicalConditionCategory.Condition)
                {
                    ConditionToggles.Add(item);
                }
                else
                {
                    MedicationConditions.Add(item);
                }

                if (condition.Category == MedicalConditionCategory.Pregnancy)
                {
                    _pregnancyItem = item;
                }
            }

            // The pregnancy question is shown to female patients only.
            ApplyPregnancyVisibility();

            PrintResultPasswordEnabled = catalog.Value.PrintResultPasswordEnabled;

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

        // D-02 mode CG: the registration catalogue does not carry custom-group membership, so the
        // union of member test ids is read once here. Read-only, no schema change; a failure is
        // non-fatal (CG simply shows nothing) exactly like the other picker sources.
        var memberIds = await _mediator.Send(new GetCustomGroupTestIdsQuery(), cancellationToken);
        if (memberIds.IsSuccess && memberIds.Value is not null)
        {
            _customGroupTestIds = new HashSet<int>(memberIds.Value);
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
            RecentUltrasoundImaging = d.RecentUltrasoundImaging;
            CardNumber = d.CardNumber;
            RegistrationDateText = FormatDateTime(d.RegistrationDateUtc);
            PickupDateText = d.PickupDateUtc.HasValue ? FormatDateTime(d.PickupDateUtc.Value) : string.Empty;
            Notes = d.Notes;

            var selectedConditions = d.MedicalConditions.Select(m => m.MedicalConditionTypeId).ToHashSet();
            foreach (var condition in Conditions)
            {
                condition.IsSelected = selectedConditions.Contains(condition.MedicalConditionTypeId);
            }

            // A male patient never carries the pregnancy answer, whatever the stored
            // rows say (STAGE-01, baseline 1.2.2.3.9).
            ApplyPregnancyVisibility();

            await LoadVisitTestsAsync(cancellationToken);
            await RefreshBillingAsync(cancellationToken);
            await LoadVisitHistoryAsync(cancellationToken);
            OnPropertyChanged(nameof(IsVisitHistoryVisible));
            IsFollowUpRequired = d.IsFollowUpRequired;
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// STAGE-01 (baseline 1.2.2.3.9): the pregnancy question is offered to
    /// female patients only. A male patient never carries the answer, so a
    /// selection that is present when the sex turns male is cleared.
    /// </summary>
    private void ApplyPregnancyVisibility()
    {
        if (_pregnancyItem is null)
        {
            return;
        }

        if (IsMale)
        {
            if (_pregnancyItem.IsSelected)
            {
                _pregnancyItem.IsSelected = false;
            }

            if (MedicationConditions.Contains(_pregnancyItem))
            {
                MedicationConditions.Remove(_pregnancyItem);
            }
        }
        else if (!MedicationConditions.Contains(_pregnancyItem))
        {
            MedicationConditions.Add(_pregnancyItem);
        }
    }

    /// <summary>STAGE-01: the identity-block sample-type summary follows the visit's tests.</summary>
    private void RefreshSampleTypeSummary() => OnPropertyChanged(nameof(SampleTypeSummary));

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
        // SD-18: this ONE insertion point covers all six existing call sites of this method
        // (:567, :840, :880, :1134, :1171, :1208) — add, remove and clear-all all stay in
        // sync without touching six places.
        RefreshCanClearAllVisitTests();

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
                AddSelectedTest(new SelectedTestItem
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

    /// <summary>
    /// Adds a test row and keeps the identity-block sample-type summary in step
    /// with the row's sample flags.
    /// </summary>
    private void AddSelectedTest(SelectedTestItem item)
    {
        item.PropertyChanged += (_, _) => RefreshSampleTypeSummary();
        SelectedTests.Add(item);
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

        AddSelectedTest(new SelectedTestItem
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
        RecentUltrasoundImaging = false;
        CardNumber = null;
        RegistrationDateText = FormatDateTime(DateTime.UtcNow);
        PickupDateText = string.Empty;
        PrintResultPasswordText = null;
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
        IReadOnlyList<int> MedicalConditionIds,
        DateTime RegistrationDateUtc,
        DateTime? PickupDateUtc,
        string? CardNumber);

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

        if (!TryParseDateTime(RegistrationDateText, out var registrationDate))
        {
            ErrorMessage = "تاريخ الدخول غير صالح. استخدم الصيغة yyyy-MM-dd HH:mm.";
            form = null!;
            return false;
        }

        DateTime? pickupDate = null;
        if (!string.IsNullOrWhiteSpace(PickupDateText))
        {
            if (!TryParseDateTime(PickupDateText, out var parsedPickup))
            {
                ErrorMessage = "تاريخ الاستلام غير صالح. استخدم الصيغة yyyy-MM-dd HH:mm.";
                form = null!;
                return false;
            }

            pickupDate = parsedPickup;
        }

        string? cardNumber = null;
        if (!string.IsNullOrWhiteSpace(CardNumber))
        {
            cardNumber = CardNumber.Trim();

            if (cardNumber.Length > Patient.MaxCardNumberLength)
            {
                ErrorMessage = "رقم البطاقة يجب ألا يتجاوز 30 حرفًا.";
                form = null!;
                return false;
            }
        }

        form = new ParsedForm(
            treatingDoctorId,
            referralEntityId,
            fastingHours,
            phones,
            Conditions.Where(c => c.IsSelected).Select(c => c.MedicalConditionTypeId).ToList(),
            registrationDate,
            pickupDate,
            cardNumber);
        return true;
    }

    private static bool TryParseDateTime(string? text, out DateTime value)
    {
        value = default;

        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        return DateTime.TryParseExact(
            text.Trim(),
            DateTimeFormats,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out value);
    }

    private static string FormatDateTime(DateTime value) =>
        value.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);

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
            form.RegistrationDateUtc,
            AccountType,
            IsVip,
            string.IsNullOrWhiteSpace(LabId) ? null : LabId.Trim(),
            string.IsNullOrWhiteSpace(Title) ? null : Title.Trim(),
            string.IsNullOrWhiteSpace(NationalId) ? null : NationalId.Trim(),
            string.IsNullOrWhiteSpace(Address) ? null : Address.Trim(),
            form.TreatingDoctorId,
            form.ReferralEntityId,
            // STAGE-01 (baseline 1.2.2.1.2): the receipt date defaults to the
            // registration moment when the user has not set one.
            form.PickupDateUtc ?? form.RegistrationDateUtc,
            IsFastingIndicated,
            form.FastingHours,
            RecentContrastImaging,
            string.IsNullOrWhiteSpace(Notes) ? null : Notes.Trim(),
            form.PhoneNumbers,
            form.MedicalConditionIds,
            tests,
            form.CardNumber,
            RecentUltrasoundImaging), cancellationToken);

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
            form.PhoneNumbers,
            form.CardNumber,
            RecentUltrasoundImaging,
            form.PickupDateUtc,
            form.RegistrationDateUtc), cancellationToken);

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
        // W-02 S14 (WP-29): one atomic command instead of N separate saves.
        var result = await _mediator.Send(new ApplyConditionDeltasCommand(patientId, wanted), cancellationToken);
        if (!result.IsSuccess)
        {
            ErrorMessage = _presenter.Present(result.Error!);
            return false;
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

        var removedIds = persisted.Values
            .Where(l => !currentIds.Contains(l.PatientTestId))
            .Select(l => l.PatientTestId)
            .ToList();
        var added = SelectedTests.Where(t => t.IsNew).ToList();
        var flagUpdates = SelectedTests
            .Where(t => !t.IsNew && persisted.ContainsKey(t.PatientTestId))
            .Where(t =>
            {
                persistedSamples.TryGetValue(t.PatientTestId, out var flags);
                return flags is null
                    || flags.IsUrine != t.IsUrine
                    || flags.IsStool != t.IsStool
                    || flags.IsBlood != t.IsBlood
                    || flags.IsSemen != t.IsSemen
                    || flags.IsCsf != t.IsCsf
                    || flags.IsTakenOutsideLab != t.IsTakenOutsideLab;
            })
            .Select(t => new VisitFlagUpdate(
                t.PatientTestId, t.IsUrine, t.IsStool, t.IsBlood, t.IsSemen, t.IsCsf, t.IsTakenOutsideLab))
            .ToList();

        // W-02 S14 (WP-29): one atomic command instead of N separate saves.
        var applied = await _mediator.Send(new ApplyVisitDeltasCommand(
            patientId, removedIds,
            added.Select(t => new AddTestInput(t.TestId, t.IsUrine, t.IsStool, t.IsBlood, t.IsSemen, t.IsCsf, t.IsTakenOutsideLab)).ToList(),
            flagUpdates), cancellationToken);
        if (!applied.IsSuccess)
        {
            ErrorMessage = _presenter.Present(applied.Error!);
            return false;
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

    /// <summary>
    /// S-03 Slice 4 (U-05): totals-row entry — parameterless <c>NavigateTo&lt;T&gt;</c>
    /// then <c>LoadAsync(patientId)</c> on the new VM (PatientsHub → editor idiom).
    /// </summary>
    private async Task OpenAccountAsync(CancellationToken cancellationToken)
    {
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;
        if (!IsEditMode || !_patientId.HasValue)
        {
            ErrorMessage = "احفظ بيانات المريض أولًا قبل عرض الحساب.";
            return;
        }

        _navigation.NavigateTo<PatientAccountViewModel>();
        if (_navigation.CurrentViewModel is PatientAccountViewModel account)
        {
            await account.LoadAsync(_patientId.Value, cancellationToken);
        }
    }

    /// <summary>
    /// GAP-10 / R1 Round 3 A1: the patient list opens in add mode as well as edit mode,
    /// so the unsaved-patient guard is deliberately absent here (it stays on
    /// <see cref="LabResultsAsync"/>). No unsaved-data confirmation exists elsewhere in the
    /// editor's navigation actions, so none is invented.
    /// </summary>
    private Task PatientWindowAsync(CancellationToken cancellationToken)
    {
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;
        _navigation.NavigateTo<PatientSearchViewModel>();
        return Task.CompletedTask;
    }

    private async Task LabResultsAsync(CancellationToken cancellationToken)
    {
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;
        if (!IsEditMode || !_patientId.HasValue)
        {
            ErrorMessage = "احفظ بيانات المريض أولًا قبل عرض النتائج.";
            return;
        }

        _navigation.NavigateTo<PatientResultSheetViewModel>();
        if (_navigation.CurrentViewModel is PatientResultSheetViewModel sheet)
        {
            await sheet.LoadAsync(_patientId.Value, cancellationToken);
        }
    }

    /// <summary>
    /// 1.2.2.6.10: the case follow-up screen for the loaded patient. Same guard and the same
    /// navigate-then-load idiom as the account and lab-results actions, so an unsaved patient
    /// gets the editor's standard "save first" message rather than a half-loaded history.
    /// </summary>
    private async Task FollowUpAsync(CancellationToken cancellationToken)
    {
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;
        if (!IsEditMode || !_patientId.HasValue)
        {
            ErrorMessage = "احفظ بيانات المريض أولًا قبل عرض المتابعة.";
            return;
        }

        _navigation.NavigateTo<TopLab.Presentation.ViewModels.Audit.AuditViewModel>();
        if (_navigation.CurrentViewModel is TopLab.Presentation.ViewModels.Audit.AuditViewModel audit)
        {
            await audit.LoadWithPatientIdAsync(_patientId.Value, cancellationToken);
        }
    }

    private async Task ToggleFollowUpFlagAsync(CancellationToken cancellationToken)
    {
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;

        if (!IsEditMode || !_patientId.HasValue)
        {
            ErrorMessage = "احفظ بيانات المريض أولًا قبل تحديد المتابعة.";
            return;
        }

        var request = new SetVisitFollowUpFlagCommand(_patientId.Value, !IsFollowUpRequired);
        var result = await _mediator.Send(request, cancellationToken);
        if (result.IsSuccess)
        {
            IsFollowUpRequired = result.Value;
            StatusMessage = "تم تحديث حالة المتابعة.";
        }
        else
        {
            ErrorMessage = result.Error?.Message ?? "تعذّر تحديث حالة المتابعة.";
        }
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

    private async Task AddTestGroupAsync(CancellationToken cancellationToken)
    {
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;
        if (!IsEditMode || !_patientId.HasValue)
        {
            ErrorMessage = "احفظ بيانات المريض أولًا قبل إضافة مجموعة تحاليل.";
            return;
        }

        if (SelectedTestGroup is null)
        {
            ErrorMessage = "اختر مجموعة تحاليل أولًا.";
            return;
        }

        IsBusy = true;
        try
        {
            var result = await _mediator.Send(new AddTestGroupToVisitCommand(_patientId.Value, SelectedTestGroup.Id), cancellationToken);
            if (!result.IsSuccess)
            {
                ErrorMessage = _presenter.Present(result.Error!);
                return;
            }

            await LoadVisitTestsAsync(cancellationToken);
            await RefreshBillingAsync(cancellationToken);
            StatusMessage = "تمت إضافة مجموعة التحاليل.";
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

        // R-A04-5 / SD-17: a stale command (button was enabled when this visit was the
        // first registration, then the state changed) is refused here with the SAME Arabic
        // literal the Application handler uses, rather than reaching the mediator.
        // The handler remains the authority — this is a courtesy short-circuit, not the gate.
        if (!CanClearAllVisitTests)
        {
            ErrorMessage = ClearAllTestsCommandHandler.FirstRegistrationOnlyMessage;
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

    /// <summary>
    /// R1 Round 3 B1/B2: the record-payment button. Parsing and the percentage conversion run
    /// through the shared <see cref="AccountEntryParser"/> / <see cref="AccountEntryCalculator"/>
    /// so this screen and the patient-account screen behave identically. A leading <c>+</c>
    /// routes to the existing extra-charge mechanism (1.3.1.4.1 / OD-11); the existing separate
    /// extra-charge action is kept.
    /// </summary>
    private async Task RecordPaymentAsync(CancellationToken cancellationToken)
    {
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;
        if (!IsEditMode || !_patientId.HasValue)
        {
            ErrorMessage = "احفظ بيانات المريض أولًا قبل تسجيل مدفوعات.";
            return;
        }

        var amount = AccountEntryParser.ParsePaidAmount(PaymentAmountText);
        if (!amount.IsValid)
        {
            ErrorMessage = amount.ErrorMessage!;
            return;
        }

        var discount = AccountEntryParser.ParseDiscount(PaymentDiscountText);
        if (!discount.IsValid)
        {
            ErrorMessage = discount.ErrorMessage!;
            return;
        }

        var percentage = AccountEntryParser.ParsePercentage(PaymentPercentageText);
        if (!percentage.IsValid)
        {
            ErrorMessage = percentage.ErrorMessage!;
            return;
        }

        if (amount.Kind == AccountEntryKind.Surcharge && discount.Value > 0m)
        {
            ErrorMessage = AccountEntryParser.SurchargeWithDiscountMessage;
            return;
        }

        IsBusy = true;
        try
        {
            var result = amount.Kind == AccountEntryKind.Surcharge
                ? await _mediator.Send(new RecordExtraChargeCommand(_patientId.Value, amount.Value), cancellationToken)
                : await _mediator.Send(
                    new RecordPaymentCommand(_patientId.Value, amount.Value, discount.Value > 0m ? discount.Value : null),
                    cancellationToken);
            if (!result.IsSuccess)
            {
                ErrorMessage = _presenter.Present(result.Error!);
                return;
            }

            _accountEntry.MarkPersisted();
            PaymentAmountText = string.Empty;
            PaymentDiscountText = null;
            PaymentPercentageText = null;
            await RefreshBillingAsync(cancellationToken);
            StatusMessage = amount.Kind == AccountEntryKind.Surcharge ? "تم تسجيل المبلغ الإضافي." : "تم تسجيل المدفوع.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// 1.3.1.4.2: one <c>Enter</c> press in the amount / discount / percentage fields.
    /// The first press only displays the resulting figures and never reaches the mediator;
    /// the second press with unchanged inputs routes to the same record-payment path the
    /// button uses, then resets.
    /// </summary>
    private async Task AccountEntryEnterAsync(CancellationToken cancellationToken)
    {
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;
        if (!IsEditMode || !_patientId.HasValue)
        {
            ErrorMessage = "احفظ بيانات المريض أولًا قبل تسجيل مدفوعات.";
            return;
        }

        // 1.3.1.3: a percentage is converted against the patient's TOTAL account amount.
        if (!string.IsNullOrWhiteSpace(PaymentPercentageText))
        {
            var conversionError = _accountEntry.ApplyPercentage(TotalCharged);
            if (conversionError is not null)
            {
                ErrorMessage = conversionError;
                return;
            }

            // Round 4 fix: push the converted amount into the field through the backing
            // field, not through the PaymentDiscountText setter. That setter exists to clear
            // the percentage on a USER edit, and routing the conversion through it would
            // clear the percentage (and, by the session's rule, the amount it just produced).
            _paymentDiscountText = _accountEntry.DiscountText;
            _paymentPercentageText = null;
            OnPropertyChanged(nameof(PaymentDiscountText));
            OnPropertyChanged(nameof(PaymentPercentageText));
        }

        var step = _accountEntry.PressEnter(TotalCharged, TotalPaid);
        if (!step.IsValid)
        {
            ErrorMessage = step.ErrorMessage!;
            return;
        }

        if (!step.ShouldPersist)
        {
            // First press: figures only, nothing recorded.
            StatusMessage = step.StatusMessage!;
            return;
        }

        await RecordPaymentAsync(cancellationToken);
    }

    /// <summary>1.3.2.5 «استرداد»: records a refund of the entered amount (never overwrites).</summary>
    private async Task RefundAsync(CancellationToken cancellationToken)
    {
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;
        if (!IsEditMode || !_patientId.HasValue)
        {
            ErrorMessage = "احفظ بيانات المريض أولًا قبل تسجيل استرداد.";
            return;
        }

        var parsed = AccountEntryParser.ParseRefundAmount(RefundAmountText);
        if (!parsed.IsValid)
        {
            ErrorMessage = parsed.ErrorMessage!;
            return;
        }

        IsBusy = true;
        try
        {
            var result = await _mediator.Send(new RecordRefundCommand(_patientId.Value, parsed.Value), cancellationToken);
            if (!result.IsSuccess)
            {
                ErrorMessage = _presenter.Present(result.Error!);
                return;
            }

            RefundAmountText = string.Empty;
            await RefreshBillingAsync(cancellationToken);
            StatusMessage = "تم تسجيل الاسترداد.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>1.3.2.7 «تصفير الحساب»: voids every operation after a confirmation.</summary>
    private async Task ClearAccountAsync(CancellationToken cancellationToken)
    {
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;
        if (!IsEditMode || !_patientId.HasValue)
        {
            ErrorMessage = "احفظ بيانات المريض أولًا قبل تصفير الحساب.";
            return;
        }

        var confirm = await _dialogs.ShowConfirmationAsync(
            "تصفير الحساب",
            "سيتم إلغاء جميع عمليات حساب المريض (تبقى ظاهرة بعلامة ملغاة). هل تريد المتابعة؟");
        if (!confirm)
        {
            return;
        }

        IsBusy = true;
        try
        {
            var result = await _mediator.Send(new ClearPatientAccountCommand(_patientId.Value), cancellationToken);
            if (!result.IsSuccess)
            {
                ErrorMessage = _presenter.Present(result.Error!);
                return;
            }

            await RefreshBillingAsync(cancellationToken);
            StatusMessage = result.Value > 0
                ? $"تم تصفير الحساب ({result.Value} عملية)."
                : "لا توجد عمليات لتصفيرها.";
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

    /// <summary>GAP-10: bind the existing envelope-print feature on the patient editor action band.</summary>
    private async Task PrintEnvelopeAsync(CancellationToken cancellationToken)
    {
        if (!IsEditMode || !_patientId.HasValue)
        {
            ErrorMessage = "احفظ بيانات المريض أولًا قبل الطباعة.";
            return;
        }

        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;
        IsBusy = true;
        try
        {
            var result = await _mediator.Send(new PrintEnvelopeCommand(_patientId.Value), cancellationToken);
            if (result.IsSuccess)
            {
                StatusMessage = "تمت طباعة المغلّف.";
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
                    "laborder" => await _mediator.Send(new PrintLabOrderCommand(_patientId.Value), cancellationToken),
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
