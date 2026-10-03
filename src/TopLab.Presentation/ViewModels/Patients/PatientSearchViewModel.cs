using System.Collections.ObjectModel;
using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ExternalEntities.Queries.SearchExternalEntities;
using TopLab.Application.Features.PatientSearch.Common;
using TopLab.Application.Features.PatientSearch.Queries.GetPatientByLabId;
using TopLab.Application.Features.PatientSearch.Queries.SearchPatientsGlobal;
using TopLab.Application.Features.PatientRegistration.Commands.PrintBarcode;
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
    private string _statusMessage = string.Empty;
    private PatientSearchHitDto? _selectedItem;

    // --- P-01 F1–F5 filter state. Six separate properties (SD-6). ---
    private ExternalEntityFilterItem? _selectedTreatingDoctor;
    private ExternalEntityFilterItem? _selectedReferralEntity;
    private TestFilterItem? _selectedTest;
    private Sex? _selectedSex;
    private AgeUnit _selectedAgeUnit = AgeUnit.Year;
    private string? _ageFrom;
    private string? _ageTo;
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
        // P-02 A-12: enabled only when a patient is selected — no half-wired state.
        ReprintBarcodeCommand = new AsyncRelayCommand(
            async (_, ct) => await ReprintBarcodeAsync(ct),
            _ => SelectedItem is not null);
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
        set
        {
            if (SetProperty(ref _selectedItem, value))
            {
                // P-02 A-12: the reprint is only meaningful for a selected patient.
                ReprintBarcodeCommand.RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>
    /// P-02 A-12 (SD-6) — reprints the selected patient's card barcode, exactly as the
    /// reference describes for a lost card (REF3 p.16 §14): search for the patient and
    /// *"طباعة الباركود الخاص به مرة اخري وال يعطى رقم اخر جديد"* — print his barcode again
    /// and <b>do not issue a new number</b>.
    ///
    /// This dispatches the EXISTING <c>PrintBarcodeCommand</c>, which re-prints the patient's
    /// current identifier. No new command was created, no identifier is minted, and
    /// <c>GetNextLabId</c> is not called — the reference forbids that here.
    /// </summary>
    public AsyncRelayCommand ReprintBarcodeCommand { get; }

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

    /// <summary>Non-error feedback, e.g. "barcode sent to the printer". Added for P-02 A-12.</summary>
    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
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

    /// <summary>
    /// F4 — inclusive lower age bound, in <see cref="SelectedAgeUnit"/>.
    ///
    /// P-02 D-4: this is <c>string</c>-backed rather than <c>int?</c> because WPF cannot
    /// convert "" to <c>int?</c> when a TextBox is cleared, so the source silently kept its
    /// old value and the only way to unset it was «مسح الفلاتر». Empty or unparseable text
    /// now maps to <c>null</c>, which widens the result set back to the unfiltered state.
    /// </summary>
    public string? AgeFrom
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

    /// <summary>
    /// F4 — inclusive upper age bound, in <see cref="SelectedAgeUnit"/>.
    /// <c>string</c>-backed for the same reason as <see cref="AgeFrom"/> (P-02 D-4).
    /// </summary>
    public string? AgeTo
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

    /// <summary>
    /// Reloads the result set whenever any filter changes, and resets to page 1.
    ///
    /// P-02 D-3: the reload used to be discarded with `_ =`, so a database fault during a
    /// filter change was lost and a burst of changes could land out of order and show a
    /// stale result set. It is now funnelled through <see cref="RunFilterReloadAsync"/>,
    /// which sequences the calls and surfaces failures.
    /// </summary>
    private void OnFilterChanged()
    {
        if (_suspendFilterReload)
        {
            return;
        }

        _ = RunFilterReloadAsync();
    }

    /// <summary>
    /// P-02 D-3 — the single place a filter reload is started.
    ///
    /// <para>
    /// <b>Sequencing uses a generation counter together with a
    /// <see cref="CancellationTokenSource"/> — not the counter alone.</b> Both are kept
    /// because they do different jobs: the token stops a superseded query early where the
    /// provider honours it, and the generation makes correctness independent of that — a
    /// result is discarded when its generation is no longer the current one, even if that
    /// query already completed and ignored the token. Cancellation alone leaves that window
    /// open; the counter alone would waste work but still be correct.
    /// </para>
    ///
    /// <para>
    /// <see cref="OperationCanceledException"/> is treated as cancellation, not as an error,
    /// and any other failure is surfaced through <see cref="ErrorMessage"/> with the previous
    /// result list left intact, so a database fault is never silently discarded.
    /// </para>
    /// </summary>
    private async Task RunFilterReloadAsync()
    {
        var generation = Interlocked.Increment(ref _filterReloadGeneration);

        // Supersede any in-flight reload before starting this one.
        var previous = Interlocked.Exchange(ref _filterReloadCts, null);
        if (previous is not null)
        {
            try
            {
                previous.Cancel();
            }
            catch (ObjectDisposedException)
            {
                // The superseded source was already disposed by its own run.
            }
        }

        var cts = new CancellationTokenSource();
        Interlocked.Exchange(ref _filterReloadCts, cts);

        try
        {
            Page = 1;
            await SearchAsync(cts.Token, generation);
        }
        catch (OperationCanceledException)
        {
            // Superseded or cancelled — not an error, and a newer reload owns the list.
        }
        catch (Exception ex)
        {
            // D-3: a fault during a filter change must be visible, not discarded. The
            // previous list is deliberately left untouched so the user keeps the last good set.
            ErrorMessage = _presenter.Present(Error.Unexpected(ex.Message));
        }
        finally
        {
            // Clear the field only if it is still ours; a newer reload may have replaced it.
            Interlocked.CompareExchange(ref _filterReloadCts, null, cts);
            cts.Dispose();
        }
    }

    private CancellationTokenSource? _filterReloadCts;

    private long _filterReloadGeneration;

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

    /// <summary>
/// P-02 D-4: parses one age bound. Null, empty or whitespace means "no bound".
/// Unparseable text also means "no bound" rather than throwing, so a stray character
/// in the box widens the result set instead of breaking the screen.
/// </summary>
public static int? ParseAgeBound(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        return int.TryParse(text.Trim(), out var value) ? value : null;
    }

    private async Task SearchAsync(CancellationToken cancellationToken, long? generation = null)
    {
        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            var text = string.IsNullOrWhiteSpace(SearchText) ? null : SearchText.Trim();

            // P-02 D-4: the age band is string-backed and parsed here. Empty or unparseable
            // text means "no bound" (null), so clearing the box widens the result set
            // back instead of silently keeping the old value. The band itself is only
            // sent when at least one bound exists, so an empty box pair stays inert (SD-5).
            var ageFrom = ParseAgeBound(AgeFrom);
            var ageTo = ParseAgeBound(AgeTo);
            var age = ageFrom is null && ageTo is null
                ? null
                : new AgeValueBand(SelectedAgeUnit, ageFrom, ageTo);

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

            // P-02 D-3: a superseded reload must never write to the view. The generation
            // check is the second line of defence after cancellation: it also catches a
            // query that completed successfully while a newer filter change was issued.
            if (generation.HasValue && generation.Value != Volatile.Read(ref _filterReloadGeneration))
            {
                return;
            }

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

    /// <summary>
    /// P-02 A-12: reprints the selected patient's card barcode.
    ///
    /// Dispatches the EXISTING <c>PrintBarcodeCommand</c> — no new command, no handler
    /// change, no barcode-port widening. The command re-prints the patient's current
    /// identifier, so nothing is minted and <c>LabId</c> is untouched (SD-6).
    /// </summary>
    private async Task ReprintBarcodeAsync(CancellationToken cancellationToken)
    {
        var patientId = SelectedItem?.PatientId;
        if (patientId is null or <= 0)
        {
            return;
        }

        var result = await _mediator.Send(new PrintBarcodeCommand(patientId.Value), cancellationToken);

        if (!result.IsSuccess && result.Error is not null)
        {
            ErrorMessage = _presenter.Present(result.Error);
            return;
        }

        StatusMessage = "تم إرسال الباركود للطباعة.";
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
