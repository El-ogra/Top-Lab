using System.Collections.ObjectModel;
using System.ComponentModel;
using MediatR;
using TopLab.Application.Features.AuditAndTraceability.Common;
using TopLab.Application.Features.AuditAndTraceability.Queries.GetAuditActivity;
using TopLab.Application.Features.AuditAndTraceability.Queries.GetPatientAudit;
using TopLab.Application.Features.AuditAndTraceability.Queries.GetPatientTestAudit;
using TopLab.Application.Features.UsersAndPermissions.Queries.GetUsers;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.Common.Navigation;

namespace TopLab.Presentation.ViewModels.Audit;

public sealed class AuditViewModel : ViewModelBase
{
    private readonly ISender _mediator;
    private readonly ResultErrorPresenter _presenter;
    private readonly INavigationService _navigation;

    private int _selectedTab; // 0=P, 1=T, 2=activity
    private int _patientIdInput;
    private int _patientTestIdInput;
    private PatientAuditDto? _patientAudit;
    private PatientTestAuditDto? _testAudit;
    private bool _isBusy;
    private string _errorMessage = string.Empty;

    // Activity tab state
    private DateOnly? _activityFrom;
    private DateOnly? _activityTo;
    private ObservableCollection<AuditActivityRow> _activityRows = new();

    public AuditViewModel(ISender mediator, ResultErrorPresenter presenter, INavigationService navigation)
    {
        _mediator = mediator;
        _presenter = presenter;
        _navigation = navigation;

        _activityFrom = new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1);
        _activityTo = new DateOnly(DateTime.Today.Year, DateTime.Today.Month, DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month));

        LoadPatientAuditCommand = new AsyncRelayCommand(async (_, ct) => await LoadPatientAuditAsync(ct));
        LoadTestAuditCommand = new AsyncRelayCommand(async (_, ct) => await LoadTestAuditAsync(ct));
        LoadActivityCommand = new AsyncRelayCommand(async (_, ct) => await LoadActivityAsync(ct));
        ShowSelectedUsersCommand = new AsyncRelayCommand(async (_, ct) => await ShowSelectedUsersAsync(ct));
        ShowAllUsersCommand = new AsyncRelayCommand(async (_, ct) => await ShowAllUsersAsync(ct));
        BackCommand = new RelayCommand(_ => _navigation.NavigateTo<Shell.HomeViewModel>());

        AuditUsers = new ObservableCollection<AuditUserFilterItem>();
    }

    public int SelectedTab
    {
        get => _selectedTab;
        set
        {
            if (SetProperty(ref _selectedTab, value))
            {
                OnPropertyChanged(nameof(IsPatientTab));
                OnPropertyChanged(nameof(IsTestTab));
                OnPropertyChanged(nameof(IsActivityTab));
            }
        }
    }

    public bool IsPatientTab => SelectedTab == 0;
    public bool IsTestTab => SelectedTab == 1;
    public bool IsActivityTab => SelectedTab == 2;

    public int PatientIdInput { get => _patientIdInput; set => SetProperty(ref _patientIdInput, value); }
    public int PatientTestIdInput { get => _patientTestIdInput; set => SetProperty(ref _patientTestIdInput, value); }

    public PatientAuditDto? PatientAudit
    {
        get => _patientAudit;
        private set
        {
            if (SetProperty(ref _patientAudit, value))
            {
                OnPropertyChanged(nameof(HasPatientAudit));
                OnPropertyChanged(nameof(ShowPatientEmpty));
            }
        }
    }

    public bool HasPatientAudit => _patientAudit is not null;
    public bool ShowPatientEmpty => _patientAudit is null && !IsBusy && string.IsNullOrEmpty(ErrorMessage);

    public PatientTestAuditDto? TestAudit
    {
        get => _testAudit;
        private set
        {
            if (SetProperty(ref _testAudit, value))
            {
                OnPropertyChanged(nameof(HasTestAudit));
                OnPropertyChanged(nameof(ShowTestEmpty));
            }
        }
    }

    public bool HasTestAudit => _testAudit is not null;
    public bool ShowTestEmpty => _testAudit is null && !IsBusy && string.IsNullOrEmpty(ErrorMessage);

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

    // Activity
    public DateOnly? ActivityFrom { get => _activityFrom; set => SetProperty(ref _activityFrom, value); }
    public DateOnly? ActivityTo { get => _activityTo; set => SetProperty(ref _activityTo, value); }

    public ObservableCollection<AuditUserFilterItem> AuditUsers { get; private set; }

    public ObservableCollection<AuditActivityRow> ActivityRows
    {
        get => _activityRows;
        private set
        {
            if (SetProperty(ref _activityRows, value))
            {
                OnPropertyChanged(nameof(HasActivityRows));
                OnPropertyChanged(nameof(ShowActivityEmpty));
            }
        }
    }

    public bool HasActivityRows => _activityRows.Count > 0;
    public bool ShowActivityEmpty => _activityRows.Count == 0 && !IsBusy && string.IsNullOrEmpty(ErrorMessage);

    public AsyncRelayCommand LoadPatientAuditCommand { get; }
    public AsyncRelayCommand LoadTestAuditCommand { get; }
    public AsyncRelayCommand LoadActivityCommand { get; }
    public AsyncRelayCommand ShowSelectedUsersCommand { get; }
    public AsyncRelayCommand ShowAllUsersCommand { get; }
    public RelayCommand BackCommand { get; }

    public Task LoadAsync(CancellationToken cancellationToken = default)
    {
        return LoadUsersAsync(cancellationToken);
    }

    private async Task LoadUsersAsync(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetUsersQuery(), cancellationToken);
        if (result.IsSuccess && result.Value is not null)
        {
            AuditUsers = new ObservableCollection<AuditUserFilterItem>(
                result.Value.Select(u => new AuditUserFilterItem(u.Id, u.UserName)));
        }
    }

    public async Task LoadWithPatientIdAsync(int patientId, CancellationToken cancellationToken = default)
    {
        PatientIdInput = patientId;
        SelectedTab = 0;
        await LoadPatientAuditAsync(cancellationToken);
    }

    public async Task LoadWithPatientTestIdAsync(int patientTestId, CancellationToken cancellationToken = default)
    {
        PatientTestIdInput = patientTestId;
        SelectedTab = 1;
        await LoadTestAuditAsync(cancellationToken);
    }

    private async Task LoadPatientAuditAsync(CancellationToken cancellationToken)
    {
        if (PatientIdInput <= 0)
        {
            ErrorMessage = "معرف غير صالح.";
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        PatientAudit = null;

        try
        {
            var result = await _mediator.Send(new GetPatientAuditQuery(PatientIdInput), cancellationToken);
            if (result.IsSuccess && result.Value is not null)
            {
                PatientAudit = result.Value;
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

    private async Task LoadTestAuditAsync(CancellationToken cancellationToken)
    {
        if (PatientTestIdInput <= 0)
        {
            ErrorMessage = "معرف غير صالح.";
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        TestAudit = null;

        try
        {
            var result = await _mediator.Send(new GetPatientTestAuditQuery(PatientTestIdInput), cancellationToken);
            if (result.IsSuccess && result.Value is not null)
            {
                TestAudit = result.Value;
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

    private async Task LoadActivityAsync(CancellationToken cancellationToken)
    {
        // "عرض الكل" semantics when no user selected.
        await RunActivityQuery(null, cancellationToken);
    }

    private async Task ShowSelectedUsersAsync(CancellationToken cancellationToken)
    {
        var ids = AuditUsers.Where(u => u.IsSelected).Select(u => u.Id).ToList();
        // If none checked, behave like show-all (documented).
        await RunActivityQuery(ids.Count == 0 ? null : ids, cancellationToken);
    }

    private async Task ShowAllUsersAsync(CancellationToken cancellationToken)
    {
        foreach (var u in AuditUsers) u.IsSelected = false;
        await RunActivityQuery(null, cancellationToken);
    }

    private async Task RunActivityQuery(IReadOnlyList<int>? userIds, CancellationToken cancellationToken)
    {
        if (ActivityFrom is null || ActivityTo is null)
        {
            ErrorMessage = "الفترة غير صالحة.";
            return;
        }

        if (ActivityFrom.Value > ActivityTo.Value)
        {
            ErrorMessage = "الفترة غير صالحة.";
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        ActivityRows = new ObservableCollection<AuditActivityRow>();

        try
        {
            var result = await _mediator.Send(
                new GetAuditActivityQuery(ActivityFrom.Value, ActivityTo.Value, userIds), cancellationToken);
            if (result.IsSuccess && result.Value is not null)
            {
                ActivityRows = new ObservableCollection<AuditActivityRow>(
                    result.Value.Select(r => new AuditActivityRow(
                        r.UserId, r.UserName, r.ActivityCode, r.ActivityLabel, r.OccurredAtUtc, r.PatientId, r.PatientTestId, r.Detail)));
            }
            else if (result.Error is not null)
            {
                ErrorMessage = _presenter.Present(result.Error);
                ActivityRows = new ObservableCollection<AuditActivityRow>();
            }
        }
        finally
        {
            IsBusy = false;
            OnPropertyChanged(nameof(HasActivityRows));
            OnPropertyChanged(nameof(ShowActivityEmpty));
        }
    }
}

public sealed class AuditUserFilterItem : INotifyPropertyChanged
{
    public int Id { get; }
    public string Name { get; }

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected != value)
            {
                _isSelected = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
            }
        }
    }

    public AuditUserFilterItem(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}

public sealed record AuditActivityRow(
    int UserId,
    string UserName,
    string ActivityCode,
    string ActivityLabel,
    DateTime OccurredAtUtc,
    int? PatientId,
    int? PatientTestId,
    string? Detail);
