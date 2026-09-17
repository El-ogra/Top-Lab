using MediatR;
using TopLab.Application.Features.UsersAndPermissions.Commands.ChangeOwnPassword;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.ErrorPresentation;

namespace TopLab.Presentation.ViewModels.Users;

/// <summary>
/// ViewModel for the D7 self-change-password dialog (S-02 Slice 1).
/// Opened by clicking the current-user name in the shell status bar.
/// </summary>
public sealed class ChangeOwnPasswordViewModel : ViewModelBase
{
    private readonly ISender _mediator;
    private readonly ResultErrorPresenter _presenter;

    private string _currentPassword = string.Empty;
    private string _newPassword = string.Empty;
    private string _confirmNewPassword = string.Empty;
    private string _errorMessage = string.Empty;
    private bool _isBusy;
    private bool _isChanged;

    public ChangeOwnPasswordViewModel(ISender mediator, ResultErrorPresenter presenter)
    {
        _mediator = mediator;
        _presenter = presenter;
    }

    public string CurrentPassword
    {
        get => _currentPassword;
        set => SetProperty(ref _currentPassword, value);
    }

    public string NewPassword
    {
        get => _newPassword;
        set => SetProperty(ref _newPassword, value);
    }

    public string ConfirmNewPassword
    {
        get => _confirmNewPassword;
        set => SetProperty(ref _confirmNewPassword, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set => SetProperty(ref _isBusy, value);
    }

    public bool IsChanged
    {
        get => _isChanged;
        private set => SetProperty(ref _isChanged, value);
    }

    public async Task<bool> ChangeAsync()
    {
        ErrorMessage = string.Empty;

        IsBusy = true;
        try
        {
            var result = await _mediator.Send(new ChangeOwnPasswordCommand(CurrentPassword, NewPassword, ConfirmNewPassword));

            if (result.IsSuccess)
            {
                IsChanged = true;
                return true;
            }

            ErrorMessage = result.Error is not null ? _presenter.Present(result.Error) : "كلمة المرور الحالية غير صحيحة";
            if (result.Errors.Count > 1)
            {
                ErrorMessage = string.Join(Environment.NewLine, _presenter.PresentAll(result.Errors));
            }

            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
