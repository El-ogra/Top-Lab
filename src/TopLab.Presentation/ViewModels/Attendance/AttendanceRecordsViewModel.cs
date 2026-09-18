using System.Collections.ObjectModel;
using MediatR;
using TopLab.Application.Features.Attendance.Common;
using TopLab.Application.Features.Attendance.Queries.GetAttendanceRecords;
using TopLab.Application.Features.UsersAndPermissions.Common;
using TopLab.Application.Features.UsersAndPermissions.Queries.GetUsers;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.Common.Navigation;

namespace TopLab.Presentation.ViewModels.Attendance;

/// <summary>
/// S-06 Slice 1: Attendance admin records screen (M18).
/// Gate: IsAbsolutePermission (handler-level, not a catalogue permission).
/// Entry point: «المستخدمون» shell path → UserManagement → attendance buttons (owner decision).
/// </summary>
public sealed class AttendanceRecordsViewModel : ViewModelBase
{
    private readonly ISender _mediator;
    private readonly ResultErrorPresenter _presenter;
    private readonly INavigationService _navigation;

    private int? _selectedUserId;
    private DateOnly? _from;
    private DateOnly? _to;
    private int _page = 1;
    private int _pageSize = 50;
    private ObservableCollection<AttendanceRecordDto> _records = new();
    private ObservableCollection<UserSummaryDto> _userFilterItems = new();
    private bool _isBusy;
    private string _errorMessage = string.Empty;

    public AttendanceRecordsViewModel(ISender mediator, ResultErrorPresenter presenter, INavigationService navigation)
    {
        _mediator = mediator;
        _presenter = presenter;
        _navigation = navigation;

        LoadCommand = new AsyncRelayCommand(async (_, ct) => await LoadAsync(ct));
        NextPageCommand = new AsyncRelayCommand(async (_, ct) => { Page++; await LoadAsync(ct); });
        PreviousPageCommand = new AsyncRelayCommand(async (_, ct) => { if (Page > 1) { Page--; await LoadAsync(ct); } });
        BackCommand = new RelayCommand(_ => _navigation.NavigateTo<Users.UserManagementViewModel>());
    }

    public int? SelectedUserId
    {
        get => _selectedUserId;
        set => SetProperty(ref _selectedUserId, value);
    }

    public DateOnly? From { get => _from; set => SetProperty(ref _from, value); }
    public DateOnly? To { get => _to; set => SetProperty(ref _to, value); }
    public int Page { get => _page; set => SetProperty(ref _page, value); }
    public int PageSize { get => _pageSize; set => SetProperty(ref _pageSize, value); }

    public ObservableCollection<UserSummaryDto> UserFilterItems
    {
        get => _userFilterItems;
        private set => SetProperty(ref _userFilterItems, value);
    }

    public ObservableCollection<AttendanceRecordDto> Records
    {
        get => _records;
        private set
        {
            if (SetProperty(ref _records, value))
            {
                OnPropertyChanged(nameof(HasRecords));
                OnPropertyChanged(nameof(ShowEmpty));
            }
        }
    }

    public bool HasRecords => Records.Count > 0;
    public bool ShowEmpty => Records.Count == 0 && !IsBusy && string.IsNullOrEmpty(ErrorMessage);

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

    public AsyncRelayCommand LoadCommand { get; }
    public AsyncRelayCommand NextPageCommand { get; }
    public AsyncRelayCommand PreviousPageCommand { get; }
    public RelayCommand BackCommand { get; }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            // Load user filter items
            if (UserFilterItems.Count == 0)
            {
                var usersResult = await _mediator.Send(new GetUsersQuery(), cancellationToken);
                if (usersResult.IsSuccess && usersResult.Value is not null)
                {
                    UserFilterItems = new ObservableCollection<UserSummaryDto>(usersResult.Value);
                }
            }

            var result = await _mediator.Send(
                new GetAttendanceRecordsQuery(SelectedUserId, From, To, Page, PageSize), cancellationToken);

            if (result.IsSuccess && result.Value is not null)
            {
                Records = new ObservableCollection<AttendanceRecordDto>(result.Value);
            }
            else if (result.Error is not null)
            {
                ErrorMessage = _presenter.Present(result.Error);
                Records = new ObservableCollection<AttendanceRecordDto>();
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
}
