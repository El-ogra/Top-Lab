using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.Attendance.Commands.CheckIn;
using TopLab.Application.Features.Attendance.Commands.CheckOut;
using TopLab.Application.Features.Attendance.Commands.EndBreak;
using TopLab.Application.Features.Attendance.Commands.StartBreak;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.Dialogs;
using TopLab.Presentation.Common.ErrorPresentation;
using TopLab.Presentation.Common.Navigation;
using TopLab.Presentation.ViewModels.Shell;

namespace TopLab.Presentation.ViewModels.Attendance;

/// <summary>
/// S-06 Slice 0: Attendance self-service screen «حضوري» (M18).
/// Command → response pattern: no backend status query exists for the current user.
/// All four commands are parameterless; user comes from the session (SD-18-2).
/// No shell wiring in this slice — entry point is the recorded open decision (Slice 1).
/// </summary>
public sealed class MyAttendanceViewModel : ViewModelBase
{
    private readonly ISender _mediator;
    private readonly ICurrentUserService _currentUser;
    private readonly ResultErrorPresenter _presenter;
    private readonly IDialogService _dialogs;
    private readonly INavigationService _navigation;

    private string _currentUserName = string.Empty;
    private string _currentTimeText = string.Empty;
    private string _statusMessage = string.Empty;
    private string _errorMessage = string.Empty;
    private bool _isBusy;
    private bool _hasCheckedIn;
    private bool _isOnBreak;
    private bool _hasCheckedOut;

    public MyAttendanceViewModel(
        ISender mediator,
        ICurrentUserService currentUser,
        ResultErrorPresenter presenter,
        IDialogService dialogs,
        INavigationService navigation)
    {
        _mediator = mediator;
        _currentUser = currentUser;
        _presenter = presenter;
        _dialogs = dialogs;
        _navigation = navigation;

        CheckInCommand = new AsyncRelayCommand(async (_, ct) => await ExecuteAsync(new CheckInCommand(), ct));
        StartBreakCommand = new AsyncRelayCommand(async (_, ct) => await ExecuteAsync(new StartBreakCommand(), ct));
        EndBreakCommand = new AsyncRelayCommand(async (_, ct) => await ExecuteAsync(new EndBreakCommand(), ct));
        CheckOutCommand = new AsyncRelayCommand(async (_, ct) => await CheckOutAsync(ct));
        BackCommand = new RelayCommand(_ => _navigation.NavigateTo<HomeViewModel>());
    }

    public string CurrentUserName
    {
        get => _currentUserName;
        private set => SetProperty(ref _currentUserName, value);
    }

    public string CurrentTimeText
    {
        get => _currentTimeText;
        private set => SetProperty(ref _currentTimeText, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
            {
                OnPropertyChanged(nameof(CanCheckIn));
                OnPropertyChanged(nameof(CanStartBreak));
                OnPropertyChanged(nameof(CanEndBreak));
                OnPropertyChanged(nameof(CanCheckOut));
            }
        }
    }

    // Command → response pattern: button availability derived from last known session state.
    public bool _hasCheckedInState
    {
        get => _hasCheckedIn;
        private set
        {
            if (SetProperty(ref _hasCheckedIn, value))
            {
                OnPropertyChanged(nameof(CanCheckIn));
                OnPropertyChanged(nameof(CanStartBreak));
                OnPropertyChanged(nameof(CanEndBreak));
                OnPropertyChanged(nameof(CanCheckOut));
            }
        }
    }

    public bool IsOnBreak
    {
        get => _isOnBreak;
        private set
        {
            if (SetProperty(ref _isOnBreak, value))
            {
                OnPropertyChanged(nameof(CanStartBreak));
                OnPropertyChanged(nameof(CanEndBreak));
            }
        }
    }

    public bool HasCheckedOut
    {
        get => _hasCheckedOut;
        private set
        {
            if (SetProperty(ref _hasCheckedOut, value))
            {
                OnPropertyChanged(nameof(CanCheckIn));
                OnPropertyChanged(nameof(CanStartBreak));
                OnPropertyChanged(nameof(CanEndBreak));
                OnPropertyChanged(nameof(CanCheckOut));
            }
        }
    }

    public bool CanCheckIn => !IsBusy && !HasCheckedOut && !_hasCheckedIn;
    public bool CanStartBreak => !IsBusy && _hasCheckedIn && !IsOnBreak && !HasCheckedOut;
    public bool CanEndBreak => !IsBusy && IsOnBreak && !HasCheckedOut;
    public bool CanCheckOut => !IsBusy && _hasCheckedIn && !HasCheckedOut;

    public AsyncRelayCommand CheckInCommand { get; }
    public AsyncRelayCommand StartBreakCommand { get; }
    public AsyncRelayCommand EndBreakCommand { get; }
    public AsyncRelayCommand CheckOutCommand { get; }
    public RelayCommand BackCommand { get; }

    public Task LoadAsync(CancellationToken cancellationToken = default)
    {
        CurrentUserName = _currentUser.UserName;
        CurrentTimeText = DateTime.Now.ToString("HH:mm:ss");
        StatusMessage = string.Empty;
        ErrorMessage = string.Empty;
        _hasCheckedIn = false;
        _isOnBreak = false;
        _hasCheckedOut = false;
        OnPropertyChanged(nameof(CanCheckIn));
        OnPropertyChanged(nameof(CanStartBreak));
        OnPropertyChanged(nameof(CanEndBreak));
        OnPropertyChanged(nameof(CanCheckOut));
        return Task.CompletedTask;
    }

    /// <summary>Update the live clock display (called by the view's DispatcherTimer).</summary>
    public void UpdateClock()
    {
        CurrentTimeText = DateTime.Now.ToString("HH:mm:ss");
    }

    private async Task CheckOutAsync(CancellationToken cancellationToken)
    {
        var confirmed = await _dialogs.ShowConfirmationAsync(
            "تأكيد تسجيل الانصراف",
            "هل تريد تسجيل الانصراف الآن؟");
        if (!confirmed)
        {
            return;
        }

        await ExecuteAsync(new CheckOutCommand(), cancellationToken);
    }

    private async Task ExecuteAsync<T>(IRequest<T> command, CancellationToken cancellationToken)
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;

        try
        {
            var result = await _mediator.Send(command, cancellationToken);

            if (command is CheckInCommand)
            {
                if (result is Result<int> checkInResult)
                {
                    if (checkInResult.IsSuccess)
                    {
                        _hasCheckedIn = true;
                        StatusMessage = "تم تسجيل الحضور بنجاح.";
                    }
                    else if (checkInResult.Error is not null)
                    {
                        ErrorMessage = _presenter.Present(checkInResult.Error);
                    }
                }
            }
            else if (command is StartBreakCommand)
            {
                if (result is Result startBreakResult)
                {
                    if (startBreakResult.IsSuccess)
                    {
                        _isOnBreak = true;
                        StatusMessage = "تم بدء الاستراحة.";
                    }
                    else if (startBreakResult.Error is not null)
                    {
                        ErrorMessage = _presenter.Present(startBreakResult.Error);
                    }
                }
            }
            else if (command is EndBreakCommand)
            {
                if (result is Result endBreakResult)
                {
                    if (endBreakResult.IsSuccess)
                    {
                        _isOnBreak = false;
                        StatusMessage = "تم إنهاء الاستراحة.";
                    }
                    else if (endBreakResult.Error is not null)
                    {
                        ErrorMessage = _presenter.Present(endBreakResult.Error);
                    }
                }
            }
            else if (command is CheckOutCommand)
            {
                if (result is Result checkOutResult)
                {
                    if (checkOutResult.IsSuccess)
                    {
                        _hasCheckedOut = true;
                        StatusMessage = "تم تسجيل الانصراف بنجاح.";
                    }
                    else if (checkOutResult.Error is not null)
                    {
                        ErrorMessage = _presenter.Present(checkOutResult.Error);
                    }
                }
            }

            OnPropertyChanged(nameof(CanCheckIn));
            OnPropertyChanged(nameof(CanStartBreak));
            OnPropertyChanged(nameof(CanEndBreak));
            OnPropertyChanged(nameof(CanCheckOut));
        }
        finally
        {
            IsBusy = false;
        }
    }
}
