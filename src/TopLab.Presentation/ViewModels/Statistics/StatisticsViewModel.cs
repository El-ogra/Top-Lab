using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using MediatR;
using TopLab.Application.Features.ExternalEntities.Queries.SearchExternalEntities;
using TopLab.Application.Features.Statistics.Common;
using TopLab.Application.Features.Statistics.Queries.GetBandedResultMonitor;
using TopLab.Application.Features.Statistics.Queries.GetPatientCountStatistics;
using TopLab.Application.Features.Statistics.Queries.GetSentOutStatistics;
using TopLab.Application.Features.Statistics.Queries.GetTestCountStatistics;
using TopLab.Application.Features.Statistics.Queries.GetUserProductivityStatistics;
using TopLab.Application.Features.SystemAndPrintSettings.Common;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Common;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Queries.GetTestGroups;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Queries.SearchTestCatalog;
using TopLab.Application.Features.UsersAndPermissions.Common;
using TopLab.Application.Features.UsersAndPermissions.Queries.GetUsers;
using TopLab.Domain.Common.Enums;
using TopLab.Presentation.Common;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Presentation.Common.Dialogs;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.Common.Navigation;

namespace TopLab.Presentation.ViewModels.Statistics;

/// <summary>
/// S-06 Slice 2: Statistics dashboard (M19) — four sections, tables-only per D5 closed.
/// D5: no charting library, no new package reference — DataGrid/number cards only.
/// </summary>
public sealed class StatisticsViewModel : ViewModelBase
{
    private readonly ISender _mediator;
    private readonly ResultErrorPresenter _presenter;
    private readonly INavigationService _navigation;
    private readonly IDialogService _dialogs;
    private readonly IBandedResultMonitorPdfWriter _monitorPdfWriter;
    private readonly ILabPrintTextStore _labPrintTextStore;

    // Shared period
    private DateOnly? _from;
    private DateOnly? _to;

    // Section 1: Patient counts
    private bool _bySex = true;
    private bool _byReferralEntity;
    private bool _byAccountType;
    private bool _groupByMonth;
    private bool _groupByDayOfMonth;
    private bool _includeMoneyRow;
    private PatientCountStatisticsDto? _patientStats;

    // Section 2: Test counts
    private int? _testGroupId;
    private TestCountStatisticsDto? _testStats;
    private ObservableCollection<TestGroupFilterItem> _testGroupItems = new();

    // Section 3: Sent-out statistics
    private int? _externalLabEntityId;
    private SentOutStatisticsDto? _sentOutStats;
    private ObservableCollection<LabFilterItem> _labItems = new();

    // Section 4: User productivity
    private int? _userId;
    private UserProductivityStatisticsDto? _productivityStats;
    private ObservableCollection<UserSummaryDto> _userItems = new();

    // Section 5 (R-F05): banded result monitor
    private TestSummaryDto? _monitorTest;
    private string _monitorMinInput = string.Empty;
    private string _monitorMaxInput = string.Empty;
    private BandedResultMonitorDto? _bandStats;
    private ObservableCollection<TestSummaryDto> _monitorTestItems = new();

    // OD-4b / BR-F05-17: dot is the ONLY accepted decimal separator. A comma is rejected
    // explicitly, because both NumberStyles.Any and NumberStyles.Number accept ',' as a
    // thousands separator and would silently read "3,5" as 35 (finding F-1).
    private const string CommaRejectedMessage = "استخدم النقطة (.) للفاصلة العشرية، والفاصلة (,) غير مقبولة.";

    private int _selectedSection; // 0=patients, 1=tests, 2=sentOut, 3=productivity, 4=monitor
    private bool _isBusy;
    private string _errorMessage = string.Empty;
    private string _statusMessage = string.Empty;

    public StatisticsViewModel(
        ISender mediator,
        ResultErrorPresenter presenter,
        INavigationService navigation,
        IDialogService dialogs,
        IBandedResultMonitorPdfWriter monitorPdfWriter,
        ILabPrintTextStore labPrintTextStore)
    {
        _mediator = mediator;
        _presenter = presenter;
        _navigation = navigation;
        _dialogs = dialogs;
        _monitorPdfWriter = monitorPdfWriter;
        _labPrintTextStore = labPrintTextStore;

        LoadPatientsCommand = new AsyncRelayCommand(async (_, ct) => await LoadPatientsAsync(ct));
        LoadTestsCommand = new AsyncRelayCommand(async (_, ct) => await LoadTestsAsync(ct));
        LoadSentOutCommand = new AsyncRelayCommand(async (_, ct) => await LoadSentOutAsync(ct));
        LoadProductivityCommand = new AsyncRelayCommand(async (_, ct) => await LoadProductivityAsync(ct));
        LoadBandCommand = new AsyncRelayCommand(async (_, ct) => await LoadBandAsync(ct));
        PrintBandCommand = new AsyncRelayCommand(async (_, ct) => await PrintBandAsync(ct));
        BackCommand = new RelayCommand(_ => _navigation.NavigateTo<Shell.HomeViewModel>());
    }

    public DateOnly? From { get => _from; set => SetProperty(ref _from, value); }
    public DateOnly? To { get => _to; set => SetProperty(ref _to, value); }

    public int SelectedSection
    {
        get => _selectedSection;
        set
        {
            if (SetProperty(ref _selectedSection, value))
            {
                OnPropertyChanged(nameof(IsPatientsSection));
                OnPropertyChanged(nameof(IsTestsSection));
                OnPropertyChanged(nameof(IsSentOutSection));
                OnPropertyChanged(nameof(IsProductivitySection));
                OnPropertyChanged(nameof(IsMonitorSection));
            }
        }
    }

    public bool IsPatientsSection => SelectedSection == 0;
    public bool IsTestsSection => SelectedSection == 1;
    public bool IsSentOutSection => SelectedSection == 2;
    public bool IsProductivitySection => SelectedSection == 3;
    public bool IsMonitorSection => SelectedSection == 4;

    // Section 5 properties (R-F05 banded result monitor)
    public ObservableCollection<TestSummaryDto> MonitorTestItems
    {
        get => _monitorTestItems;
        private set => SetProperty(ref _monitorTestItems, value);
    }

    public TestSummaryDto? MonitorTest
    {
        get => _monitorTest;
        set => SetProperty(ref _monitorTest, value);
    }

    /// <summary>Free-text min. Parsed per BR-F05-17: dot only, comma rejected.</summary>
    public string MonitorMinInput
    {
        get => _monitorMinInput;
        set => SetProperty(ref _monitorMinInput, value);
    }

    /// <summary>Free-text max. Parsed per BR-F05-17: dot only, comma rejected.</summary>
    public string MonitorMaxInput
    {
        get => _monitorMaxInput;
        set => SetProperty(ref _monitorMaxInput, value);
    }

    public BandedResultMonitorDto? BandStats
    {
        get => _bandStats;
        private set
        {
            if (SetProperty(ref _bandStats, value))
            {
                OnPropertyChanged(nameof(HasBandStats));
                OnPropertyChanged(nameof(ShowBandEmpty));
            }
        }
    }

    public bool HasBandStats => _bandStats is not null && _bandStats.TotalCount > 0;
    public bool ShowBandEmpty => _bandStats is not null && _bandStats.TotalCount == 0 && !IsBusy;

    // Section 1 properties
    public bool BySex { get => _bySex; set => SetProperty(ref _bySex, value); }
    public bool ByReferralEntity { get => _byReferralEntity; set => SetProperty(ref _byReferralEntity, value); }
    public bool ByAccountType { get => _byAccountType; set => SetProperty(ref _byAccountType, value); }
    public bool GroupByMonth { get => _groupByMonth; set => SetProperty(ref _groupByMonth, value); }
    public bool GroupByDayOfMonth { get => _groupByDayOfMonth; set => SetProperty(ref _groupByDayOfMonth, value); }
    public bool IncludeMoneyRow { get => _includeMoneyRow; set => SetProperty(ref _includeMoneyRow, value); }

    public PatientCountStatisticsDto? PatientStats
    {
        get => _patientStats;
        private set
        {
            if (SetProperty(ref _patientStats, value))
            {
                OnPropertyChanged(nameof(HasPatientStats));
                OnPropertyChanged(nameof(ShowPatientsEmpty));
                OnPropertyChanged(nameof(MoneyRowText));
                OnPropertyChanged(nameof(HasMoneyRow));
            }
        }
    }

    public bool HasPatientStats => _patientStats is not null && _patientStats.TotalCount > 0;
    public bool ShowPatientsEmpty => _patientStats is not null && _patientStats.TotalCount == 0 && !IsBusy;

    /// <summary>
    /// BR-F01-12: one line under the grids, formatted with InvariantCulture so the digits
    /// and the decimal point do not depend on the operator's machine locale. Null while
    /// <c>IncludeMoneyRow</c> is off (BR-F01-11), which is what keeps the row hidden.
    /// </summary>
    public string? MoneyRowText => _patientStats?.Money is null
        ? null
        : string.Format(
            CultureInfo.InvariantCulture,
            "المدفوعات: {0}",
            _patientStats.Money.AmountsPaid);

    public bool HasMoneyRow => MoneyRowText is not null;

    // Section 2 properties
    public int? TestGroupId { get => _testGroupId; set => SetProperty(ref _testGroupId, value); }

    public TestCountStatisticsDto? TestStats
    {
        get => _testStats;
        private set
        {
            if (SetProperty(ref _testStats, value))
            {
                OnPropertyChanged(nameof(HasTestStats));
                OnPropertyChanged(nameof(ShowTestsEmpty));
            }
        }
    }

    public bool HasTestStats => _testStats is not null && _testStats.TotalOrders > 0;
    public bool ShowTestsEmpty => _testStats is not null && _testStats.TotalOrders == 0 && !IsBusy;

    public ObservableCollection<TestGroupFilterItem> TestGroupItems
    {
        get => _testGroupItems;
        private set => SetProperty(ref _testGroupItems, value);
    }

    // Section 3 properties
    public int? ExternalLabEntityId { get => _externalLabEntityId; set => SetProperty(ref _externalLabEntityId, value); }

    public SentOutStatisticsDto? SentOutStats
    {
        get => _sentOutStats;
        private set
        {
            if (SetProperty(ref _sentOutStats, value))
            {
                OnPropertyChanged(nameof(HasSentOutStats));
                OnPropertyChanged(nameof(ShowSentOutEmpty));
            }
        }
    }

    public bool HasSentOutStats => _sentOutStats is not null && _sentOutStats.TotalSent > 0;
    public bool ShowSentOutEmpty => _sentOutStats is not null && _sentOutStats.TotalSent == 0 && !IsBusy;

    public ObservableCollection<LabFilterItem> LabItems
    {
        get => _labItems;
        private set => SetProperty(ref _labItems, value);
    }

    // Section 4 properties
    public int? UserId { get => _userId; set => SetProperty(ref _userId, value); }

    public UserProductivityStatisticsDto? ProductivityStats
    {
        get => _productivityStats;
        private set
        {
            if (SetProperty(ref _productivityStats, value))
            {
                OnPropertyChanged(nameof(HasProductivityStats));
                OnPropertyChanged(nameof(ShowProductivityEmpty));
            }
        }
    }

    public bool HasProductivityStats => _productivityStats is not null && _productivityStats.Users.Count > 0;
    public bool ShowProductivityEmpty => _productivityStats is not null && _productivityStats.Users.Count == 0 && !IsBusy;

    public ObservableCollection<UserSummaryDto> UserItems
    {
        get => _userItems;
        private set => SetProperty(ref _userItems, value);
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

    public AsyncRelayCommand LoadPatientsCommand { get; }
    public AsyncRelayCommand LoadTestsCommand { get; }
    public AsyncRelayCommand LoadSentOutCommand { get; }
    public AsyncRelayCommand LoadProductivityCommand { get; }
    public AsyncRelayCommand LoadBandCommand { get; }
    public AsyncRelayCommand PrintBandCommand { get; }
    public RelayCommand BackCommand { get; }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        await LoadFilterItemsAsync(cancellationToken);
        await LoadPatientsAsync(cancellationToken);
    }

    private async Task LoadFilterItemsAsync(CancellationToken cancellationToken)
    {
        // Test groups for section 2
        if (TestGroupItems.Count == 0)
        {
            var groupsResult = await _mediator.Send(new GetTestGroupsQuery(false), cancellationToken);
            if (groupsResult.IsSuccess && groupsResult.Value is not null)
            {
                var items = new ObservableCollection<TestGroupFilterItem> { new(null, "الكل") };
                foreach (var g in groupsResult.Value)
                {
                    items.Add(new TestGroupFilterItem(g.Id, g.Name));
                }

                TestGroupItems = items;
            }
        }

        // External labs for section 3 (D10 pattern)
        if (LabItems.Count == 0)
        {
            var labsResult = await _mediator.Send(
                new SearchExternalEntitiesQuery(EntityType.PartnerLab, null, 1, 100), cancellationToken);
            if (labsResult.IsSuccess && labsResult.Value is not null)
            {
                var items = new ObservableCollection<LabFilterItem> { new(null, "الكل") };
                foreach (var lab in labsResult.Value)
                {
                    items.Add(new LabFilterItem(lab.Id, lab.Name));
                }

                LabItems = items;
            }
        }

        // OD-1: the monitor's test picker reuses the existing un-paged
        // SearchTestCatalogQuery. No new query, no new validator, no new authorization entry.
        if (MonitorTestItems.Count == 0)
        {
            var catalogResult = await _mediator.Send(
                new SearchTestCatalogQuery(null, null, IncludeInactive: false), cancellationToken);
            if (catalogResult.IsSuccess && catalogResult.Value is not null)
            {
                MonitorTestItems = new ObservableCollection<TestSummaryDto>(catalogResult.Value);
            }
        }

        // Users for section 4
        if (UserItems.Count == 0)
        {
            var usersResult = await _mediator.Send(new GetUsersQuery(), cancellationToken);
            if (usersResult.IsSuccess && usersResult.Value is not null)
            {
                UserItems = new ObservableCollection<UserSummaryDto>(usersResult.Value);
            }
        }
    }

    private async Task LoadPatientsAsync(CancellationToken cancellationToken)
    {
        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            var result = await _mediator.Send(
                new GetPatientCountStatisticsQuery(From, To, BySex, ByReferralEntity, ByAccountType, GroupByMonth, GroupByDayOfMonth, IncludeMoneyRow),
                cancellationToken);

            if (result.IsSuccess && result.Value is not null)
            {
                PatientStats = result.Value;
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

    private async Task LoadTestsAsync(CancellationToken cancellationToken)
    {
        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            var result = await _mediator.Send(
                new GetTestCountStatisticsQuery(From, To, TestGroupId), cancellationToken);

            if (result.IsSuccess && result.Value is not null)
            {
                TestStats = result.Value;
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

    private async Task LoadSentOutAsync(CancellationToken cancellationToken)
    {
        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            var result = await _mediator.Send(
                new GetSentOutStatisticsQuery(From, To, ExternalLabEntityId), cancellationToken);

            if (result.IsSuccess && result.Value is not null)
            {
                SentOutStats = result.Value;
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

    private async Task LoadProductivityAsync(CancellationToken cancellationToken)
    {
        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            var result = await _mediator.Send(
                new GetUserProductivityStatisticsQuery(From, To, UserId), cancellationToken);

            if (result.IsSuccess && result.Value is not null)
            {
                ProductivityStats = result.Value;
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
    /// BR-F05-17 / OD-4b: parses a min or max the operator typed.
    /// <para>
    /// A value containing a comma is REJECTED outright, before any parsing is attempted.
    /// This is deliberately stricter than both existing repository patterns
    /// (<c>ResultFlagComputer.TryParse</c> uses <c>NumberStyles.Any</c> and
    /// <c>PatientEditorViewModel</c> uses <c>NumberStyles.Number</c>) — both include
    /// <c>AllowThousands</c> and would silently read "3,5" as 35. Only
    /// <c>AllowDecimalPoint</c> is used here.
    /// </para>
    /// </summary>
    private static bool TryParseBandInput(string? input, out decimal value, out string? error)
    {
        value = 0m;
        error = null;

        var text = input?.Trim();
        if (string.IsNullOrEmpty(text))
        {
            error = "أدخل الحد الأدنى والحد الأقصى.";
            return false;
        }

        if (text.Contains(',', StringComparison.Ordinal))
        {
            error = CommaRejectedMessage;
            return false;
        }

        const NumberStyles styles = NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign;

        if (!decimal.TryParse(text, styles, CultureInfo.InvariantCulture, out value))
        {
            error = "أدخل قيمة رقمية صحيحة.";
            return false;
        }

        return true;
    }

    private async Task LoadBandAsync(CancellationToken cancellationToken)
    {
        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            if (MonitorTest is null)
            {
                ErrorMessage = "اختر التحليل.";
                return;
            }

            if (!TryParseBandInput(MonitorMinInput, out var min, out var minError))
            {
                ErrorMessage = minError!;
                return;
            }

            if (!TryParseBandInput(MonitorMaxInput, out var max, out var maxError))
            {
                ErrorMessage = maxError!;
                return;
            }

            if (min > max)
            {
                ErrorMessage = "الحد الأدنى يجب ألا يتجاوز الحد الأقصى.";
                return;
            }

            var result = await _mediator.Send(
                new GetBandedResultMonitorQuery(MonitorTest.Id, From, To, min, max),
                cancellationToken);

            if (result.IsSuccess && result.Value is not null)
            {
                BandStats = result.Value;
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

    private async Task PrintBandAsync(CancellationToken cancellationToken)
    {
        if (BandStats is null)
        {
            ErrorMessage = "اعرض النتائج أولًا قبل الطباعة.";
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            var path = await _dialogs.PickPdfSavePathAsync("نطاق-النتائج.pdf");
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            var labTextResult = await _labPrintTextStore.GetAsync(LabPrintTextScope.Report);
            if (!labTextResult.IsSuccess || labTextResult.Value is null)
            {
                ErrorMessage = labTextResult.Error is not null
                    ? _presenter.Present(labTextResult.Error)
                    : "تعذّر تحميل بيانات المعمل للطباعة.";
                return;
            }

            await _monitorPdfWriter.WritePdfAsync(path, BandStats, labTextResult.Value);
            StatusMessage = "تم إنشاء ملف نطاق النتائج.";
        }
        catch (IOException)
        {
            ErrorMessage = "الملف موجود مسبقًا؛ لم يتم الكتابة فوقه.";
        }
        catch (UnauthorizedAccessException)
        {
            ErrorMessage = "لا توجد صلاحية للكتابة في المسار المحدد.";
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            ErrorMessage = _presenter.Present(Error.Unexpected(ex.Message));
        }
        finally
        {
            IsBusy = false;
        }
    }
}

/// <summary>Test group filter ComboBox item.</summary>
public sealed record TestGroupFilterItem(int? Id, string Name);

/// <summary>Lab filter ComboBox item (D10 pattern).</summary>
public sealed record LabFilterItem(int? Id, string Name);
