using System.Collections.ObjectModel;
using MediatR;
using TopLab.Application.Features.ExternalEntities.Queries.SearchExternalEntities;
using TopLab.Application.Features.PatientSearch.Common;
using TopLab.Application.Features.PatientSearch.Queries.GetPatientByLabId;
using TopLab.Application.Features.PatientSearch.Queries.SearchPatientsGlobal;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Queries.SearchTestCatalog;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.Common.Navigation;

namespace TopLab.Presentation.ViewModels.Patients;

/// <summary>
/// S-05 Slice 0: Patient search screen (M08) — global search + lab-id fetch.
/// Gateway: «بحث عن مريض» in PatientsHubViewModel.
/// Open-patient affordance ships disabled until Slice 1 (no-half-wired-state).
/// </summary>
public sealed class PatientSearchViewModel : ViewModelBase
{
    private readonly ISender _mediator;
    private readonly ResultErrorPresenter _presenter;
    private readonly INavigationService _navigation;

    private string _searchText = string.Empty;
    private string _labIdText = string.Empty;
    private int _page = 1;
    private int _pageSize = 50;
    private ObservableCollection<PatientSearchHitDto> _items = new();
    private int _totalCount;
    private bool _isBusy;
    private string _errorMessage = string.Empty;
    private PatientSearchHitDto? _selectedItem;

    // --- P-01 F1–F5 filter state. Six separate properties (SD-6). ---
    private ExternalEntityFilterItem? _selectedTreatingDoctor;
    private ExternalEntityFilterItem? _selectedReferralEntity;
    private TestFilterItem? _selectedTest;
    private Sex? _selectedSex;
    private AgeUnit _selectedAgeUnit = AgeUnit.Year;
    private int? _ageFrom;
    private int? _ageTo;
    private DateTime? _fromDate;
    private DateTime? _toDate;

    private ObservableCollection<ExternalEntityFilterItem> _treatingDoctorOptions = new();
    private ObservableCollection<ExternalEntityFilterItem> _referralEntityOptions = new();
    private ObservableCollection<TestFilterItem> _testOptions = new();

    public PatientSearchViewModel(ISender mediator, ResultErrorPresenter presenter, INavigationService navigation)
    {
        _mediator = mediator;
        _presenter = presenter;
        _navigation = navigation;

        SearchCommand = new AsyncRelayCommand(async (_, ct) => await SearchAsync(ct));
        FetchByLabIdCommand = new AsyncRelayCommand(async (_, ct) => await FetchByLabIdAsync(ct));
        NextPageCommand = new AsyncRelayCommand(async (_, ct) => { Page++; await SearchAsync(ct); });
        PreviousPageCommand = new AsyncRelayCommand(async (_, ct) => { if (Page > 1) { Page--; await SearchAsync(ct); } });
        OpenPatientCommand = new RelayCommand(param => OpenPatient(param as PatientSearchHitDto));
        BackCommand = new RelayCommand(_ => _navigation.NavigateTo<PatientsHubViewModel>());
        // WP-15 / SD-2 / C-7: no Patient.BranchNumber — honest notice only.
        BranchFilterNoticeCommand = new RelayCommand(_ =>
        {
            ErrorMessage = "البحث بفرع غير متاح دون مبيعات موزّعة على الفروع.";
        });

        // P-01: clearing filters must not fire nine separate reloads; the individual
        // setters are suspended for the duration and one reload runs at the end.
        ClearFiltersCommand = new RelayCommand(_ =>
        {
            _suspendFilterReload = true;
            try
            {
                SelectedTreatingDoctor = null;
                SelectedReferralEntity = null;
                SelectedTest = null;
                SelectedSex = null;
                AgeFrom = null;
                AgeTo = null;
                FromDate = null;
                ToDate = null;
                SelectedAgeUnit = AgeUnit.Year;
            }
            finally
            {
                _suspendFilterReload = false;
            }

            OnFilterChanged();
        });
    }

    private bool _suspendFilterReload;

    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    public string LabIdText
    {
        get => _labIdText;
        set => SetProperty(ref _labIdText, value);
    }

    public int Page
    {
        get => _page;
        set => SetProperty(ref _page, value);
    }

    public int PageSize
    {
        get => _pageSize;
        set => SetProperty(ref _pageSize, value);
    }

    public PatientSearchHitDto? SelectedItem
    {
        get => _selectedItem;
        set => SetProperty(ref _selectedItem, value);
    }

    public ObservableCollection<PatientSearchHitDto> Items
    {
        get => _items;
        private set
        {
            if (SetProperty(ref _items, value))
            {
                OnPropertyChanged(nameof(HasResults));
                OnPropertyChanged(nameof(ShowEmpty));
            }
        }
    }

    public int TotalCount
    {
        get => _totalCount;
        private set => SetProperty(ref _totalCount, value);
    }

    public bool HasResults => Items.Count > 0;
    public bool ShowEmpty => Items.Count == 0 && !IsBusy && string.IsNullOrEmpty(ErrorMessage);

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

    public AsyncRelayCommand SearchCommand { get; }
    public AsyncRelayCommand FetchByLabIdCommand { get; }
    public AsyncRelayCommand NextPageCommand { get; }
    public AsyncRelayCommand PreviousPageCommand { get; }
    public RelayCommand OpenPatientCommand { get; }
    public RelayCommand BackCommand { get; }

    public RelayCommand BranchFilterNoticeCommand { get; }

    /// <summary>
    /// P-01 §5 step 6 — resets every filter to its inert state. Clearing a filter must widen
    /// the result set back to the unfiltered-by-that-filter state, never to an unfiltered table:
    /// <see cref="SearchAsync"/> still applies paging, and the soft-delete exclusion is untouched.
    /// </summary>
    public RelayCommand ClearFiltersCommand { get; }

    // =====================================================================
    // P-01 filter bindings. All six are PUBLIC (C-7) and all are separate:
    // treating doctor and referral entity are distinct concepts and never merge (SD-6).
    // =====================================================================

    /// <summary>F1a — treating doctor (an individual physician).</summary>
    public ExternalEntityFilterItem? SelectedTreatingDoctor
    {
        get => _selectedTreatingDoctor;
        set
        {
            if (SetProperty(ref _selectedTreatingDoctor, value))
            {
                OnFilterChanged();
            }
        }
    }

    /// <summary>F1b — referral entity (an institution). Separate property, separate lookup, separate control (SD-6).</summary>
    public ExternalEntityFilterItem? SelectedReferralEntity
    {
        get => _selectedReferralEntity;
        set
        {
            if (SetProperty(ref _selectedReferralEntity, value))
            {
                OnFilterChanged();
            }
        }
    }

    /// <summary>F2 — search by test.</summary>
    public TestFilterItem? SelectedTest
    {
        get => _selectedTest;
        set
        {
            if (SetProperty(ref _selectedTest, value))
            {
                OnFilterChanged();
            }
        }
    }

    /// <summary>F3 — search by gender. Null means "no gender filter".</summary>
    public Sex? SelectedSex
    {
        get => _selectedSex;
        set
        {
            if (SetProperty(ref _selectedSex, value))
            {
                OnFilterChanged();
            }
        }
    }

    /// <summary>F4 — the unit both the stored age and the band are expressed in. No conversion is performed.</summary>
    public AgeUnit SelectedAgeUnit
    {
        get => _selectedAgeUnit;
        set
        {
            if (SetProperty(ref _selectedAgeUnit, value))
            {
                OnFilterChanged();
            }
        }
    }

    /// <summary>F4 — inclusive lower age bound, in <see cref="SelectedAgeUnit"/>.</summary>
    public int? AgeFrom
    {
        get => _ageFrom;
        set
        {
            if (SetProperty(ref _ageFrom, value))
            {
                OnFilterChanged();
            }
        }
    }

    /// <summary>F4 — inclusive upper age bound, in <see cref="SelectedAgeUnit"/>.</summary>
    public int? AgeTo
    {
        get => _ageTo;
        set
        {
            if (SetProperty(ref _ageTo, value))
            {
                OnFilterChanged();
            }
        }
    }

    /// <summary>F5 — inclusive first day of the registration period.</summary>
    public DateTime? FromDate
    {
        get => _fromDate;
        set
        {
            if (SetProperty(ref _fromDate, value))
            {
                OnFilterChanged();
            }
        }
    }

    /// <summary>F5 — inclusive last day of the registration period.</summary>
    public DateTime? ToDate
    {
        get => _toDate;
        set
        {
            if (SetProperty(ref _toDate, value))
            {
                OnFilterChanged();
            }
        }
    }

    /// <summary>Treating-doctor lookup options — <see cref="EntityType.TreatingDoctor"/> only.</summary>
    public ObservableCollection<ExternalEntityFilterItem> TreatingDoctorOptions
    {
        get => _treatingDoctorOptions;
        private set => SetProperty(ref _treatingDoctorOptions, value);
    }

    /// <summary>Referral-entity lookup options — <see cref="EntityType.ReferralOrContract"/> only.</summary>
    public ObservableCollection<ExternalEntityFilterItem> ReferralEntityOptions
    {
        get => _referralEntityOptions;
        private set => SetProperty(ref _referralEntityOptions, value);
    }

    /// <summary>Test lookup options (F2).</summary>
    public ObservableCollection<TestFilterItem> TestOptions
    {
        get => _testOptions;
        private set => SetProperty(ref _testOptions, value);
    }

    /// <summary>Gender choices for the F3 ComboBox; a null value means "all".</summary>
    public IReadOnlyList<SexFilterItem> SexOptions { get; } = new[]
    {
        new SexFilterItem(null, "الكل"),
        new SexFilterItem(Sex.Male, "ذكر"),
        new SexFilterItem(Sex.Female, "أنثى")
    };

    /// <summary>Age-unit choices for the F4 band.</summary>
    public IReadOnlyList<AgeUnitFilterItem> AgeUnitOptions { get; } = new[]
    {
        new AgeUnitFilterItem(AgeUnit.Day, "يوم"),
        new AgeUnitFilterItem(AgeUnit.Month, "شهر"),
        new AgeUnitFilterItem(AgeUnit.Year, "سنة")
    };

    /// <summary>Reloads the result set whenever any filter changes, and resets to page 1.</summary>
    private void OnFilterChanged()
    {
        if (_suspendFilterReload)
        {
            return;
        }

        _ = ReloadFromFirstPageAsync();
    }

    private async Task ReloadFromFirstPageAsync()
    {
        Page = 1;
        await SearchAsync(CancellationToken.None);
    }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        await LoadFilterLookupsAsync(cancellationToken);
        await SearchAsync(cancellationToken);
    }

    /// <summary>
    /// Loads the three lookup lists. The doctor and referral-entity lists are populated from
    /// two SEPARATE queries differing only in <see cref="EntityType"/>, so neither list can
    /// ever contain the other concept's rows (SD-6).
    /// </summary>
    private async Task LoadFilterLookupsAsync(CancellationToken cancellationToken)
    {
        if (TreatingDoctorOptions.Count == 0)
        {
            var doctors = await _mediator.Send(
                new SearchExternalEntitiesQuery(EntityType.TreatingDoctor, null, 1, 200), cancellationToken);
            if (doctors.IsSuccess && doctors.Value is not null)
            {
                var items = new ObservableCollection<ExternalEntityFilterItem> { new(null, "الكل") };
                foreach (var doctor in doctors.Value)
                {
                    items.Add(new ExternalEntityFilterItem(doctor.Id, doctor.Name));
                }

                TreatingDoctorOptions = items;
            }
        }

        if (ReferralEntityOptions.Count == 0)
        {
            var referrals = await _mediator.Send(
                new SearchExternalEntitiesQuery(EntityType.ReferralOrContract, null, 1, 200), cancellationToken);
            if (referrals.IsSuccess && referrals.Value is not null)
            {
                var items = new ObservableCollection<ExternalEntityFilterItem> { new(null, "الكل") };
                foreach (var referral in referrals.Value)
                {
                    items.Add(new ExternalEntityFilterItem(referral.Id, referral.Name));
                }

                ReferralEntityOptions = items;
            }
        }

        if (TestOptions.Count == 0)
        {
            var catalog = await _mediator.Send(
                new SearchTestCatalogQuery(null, null, IncludeInactive: false), cancellationToken);
            if (catalog.IsSuccess && catalog.Value is not null)
            {
                var items = new ObservableCollection<TestFilterItem> { new(null, "الكل") };
                foreach (var test in catalog.Value)
                {
                    items.Add(new TestFilterItem(test.Id, test.Name));
                }

                TestOptions = items;
            }
        }
    }

    private async Task SearchAsync(CancellationToken cancellationToken)
    {
        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            var text = string.IsNullOrWhiteSpace(SearchText) ? null : SearchText.Trim();

            // The age band is only sent when at least one bound is supplied, so an empty
            // band is inert rather than a match-everything predicate (SD-5).
            var age = AgeFrom is null && AgeTo is null
                ? null
                : new AgeValueBand(SelectedAgeUnit, AgeFrom, AgeTo);

            // F5: DateTime? -> DateOnly?, inclusive on both ends.
            DateOnly? from = FromDate.HasValue
                ? DateOnly.FromDateTime(FromDate.Value)
                : null;
            DateOnly? to = ToDate.HasValue
                ? DateOnly.FromDateTime(ToDate.Value)
                : null;

            var result = await _mediator.Send(
                new SearchPatientsGlobalQuery(
                    text,
                    Page,
                    PageSize,
                    SelectedTreatingDoctor?.Id is int doctorId ? ExternalEntityId.Create(doctorId) : null,
                    SelectedReferralEntity?.Id is int referralId ? ExternalEntityId.Create(referralId) : null,
                    SelectedTest?.Id,
                    SelectedSex,
                    age,
                    from,
                    to),
                cancellationToken);

            if (result.IsSuccess && result.Value is not null)
            {
                Items = new ObservableCollection<PatientSearchHitDto>(result.Value);
                TotalCount = result.Value.Count;
            }
            else if (result.Error is not null)
            {
                ErrorMessage = _presenter.Present(result.Error);
                Items = new ObservableCollection<PatientSearchHitDto>();
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task FetchByLabIdAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(LabIdText))
        {
            ErrorMessage = "كود المعمل مطلوب.";
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            var result = await _mediator.Send(
                new GetPatientByLabIdQuery(LabIdText.Trim()), cancellationToken);

            if (result.IsSuccess && result.Value is not null)
            {
                // S-05 Slice 1: navigate to visit history with the pre-fetched DTO
                var history = result.Value;
                var patientId = history.Visits.Count > 0 ? history.Visits[0].PatientId : 0;
                _navigation.NavigateTo<PatientVisitHistoryViewModel>();
                if (_navigation.CurrentViewModel is PatientVisitHistoryViewModel vm && patientId > 0)
                {
                    await vm.LoadWithHistoryAsync(patientId, history, cancellationToken);
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

    private void OpenPatient(PatientSearchHitDto? hit)
    {
        if (hit is null || hit.PatientId <= 0)
        {
            return;
        }

        _navigation.NavigateTo<PatientVisitHistoryViewModel>();
        if (_navigation.CurrentViewModel is PatientVisitHistoryViewModel vm)
        {
            _ = vm.LoadAsync(hit.PatientId);
        }
    }
}

/// <summary>
/// P-01 F1 — one entry in a treating-doctor OR referral-entity lookup. A null
/// <see cref="Id"/> is the "all" entry and forwards a null id, i.e. no filter (SD-5).
/// </summary>
public sealed record ExternalEntityFilterItem(int? Id, string Name);

/// <summary>P-01 F2 — one entry in the test lookup; a null <see cref="Id"/> means "all".</summary>
public sealed record TestFilterItem(int? Id, string Name);

/// <summary>P-01 F3 — a gender choice; a null <see cref="Sex"/> means "all".</summary>
public sealed record SexFilterItem(Sex? Sex, string Label);

/// <summary>P-01 F4 — an age-unit choice. The band compares like with like, so the unit is a filter input, never a conversion factor.</summary>
public sealed record AgeUnitFilterItem(AgeUnit Unit, string Label);
