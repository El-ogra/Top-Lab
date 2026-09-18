using MediatR;
using TopLab.Application.Features.Attendance.Common;
using TopLab.Application.Features.Attendance.Queries.GetUserAttendanceSummary;
using TopLab.Application.Features.UsersAndPermissions.Common;
using TopLab.Application.Features.UsersAndPermissions.Queries.GetUsers;
using System.Collections.ObjectModel;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.Common.Navigation;

namespace TopLab.Presentation.ViewModels.Attendance;

/// <summary>
/// S-06 Slice 1: User attendance summary screen (M18).
/// Gate: IsAbsolutePermission (handler-level).
/// Entry point: «المستخدمون» shell path → UserManagement → attendance buttons.
/// </summary>
public sealed class UserAttendanceSummaryViewModel : ViewModelBase
{
    private readonly ISender _mediator;
    private readonly ResultErrorPresenter _presenter;
    private readonly INavigationService _navigation;

    private int _selectedUserId;
    private DateOnly? _from;
    private DateOnly? _to;
    private UserAttendanceSummaryDto? _summary;
    private ObservableCollection<UserSummaryDto> _userItems = new();
    private bool _isBusy;
    private string _errorMessage = string.Empty;

    public UserAttendanceSummaryViewModel(ISender mediator, ResultErrorPresenter presenter, INavigationService navigation)
    {
        _mediator = mediator;
        _presenter = presenter;
        _navigation = navigation;

        LoadCommand = new AsyncRelayCommand(async (_, ct) => await LoadAsync(ct));
        BackCommand = new RelayCommand(_ => _navigation.NavigateTo<Users.UserManagementViewModel>());
    }

    public int SelectedUserId
    {
        get => _selectedUserId;
        set => SetProperty(ref _selectedUserId, value);
    }

    public DateOnly? From { get => _from; set => SetProperty(ref _from, value); }
    public DateOnly? To { get => _to; set => SetProperty(ref _to, value); }

    public ObservableCollection<UserSummaryDto> UserItems
    {
        get => _userItems;
        private set => SetProperty(ref _userItems, value);
    }

    public UserAttendanceSummaryDto? Summary
    {
        get => _summary;
        private set
        {
            if (SetProperty(ref _summary, value))
            {
                OnPropertyChanged(nameof(HasSummary));
                OnPropertyChanged(nameof(ShowEmpty));
            }
        }
    }

    public bool HasSummary => _summary is not null;
    public bool ShowEmpty => _summary is null && !IsBusy && string.IsNullOrEmpty(ErrorMessage);

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
    public RelayCommand BackCommand { get; }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        Summary = null;

        try
        {
            // Load user list for the dropdown
            if (UserItems.Count == 0)
            {
                var usersResult = await _mediator.Send(new GetUsersQuery(), cancellationToken);
                if (usersResult.IsSuccess && usersResult.Value is not null)
                {
                    UserItems = new ObservableCollection<UserSummaryDto>(usersResult.Value);
                }
            }

            if (SelectedUserId <= 0)
            {
                ErrorMessage = "المستخدم غير موجود.";
                return;
            }

            var result = await _mediator.Send(
                new GetUserAttendanceSummaryQuery(SelectedUserId, From, To), cancellationToken);

            if (result.IsSuccess && result.Value is not null)
            {
                Summary = result.Value;
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
