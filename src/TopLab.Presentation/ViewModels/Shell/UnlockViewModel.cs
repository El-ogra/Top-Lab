using MediatR;
using TopLab.Application.Features.UsersAndPermissions.Commands.SignIn;
using TopLab.Presentation.Common;

namespace TopLab.Presentation.ViewModels.Shell;

/// <summary>
/// ViewModel for the workstation unlock window (S-02 Slice 0, P1 §1.4.5).
/// The user name is captured BEFORE locking (locking clears the session)
/// and shown read-only; unlocking reuses the existing <see cref="SignInCommand"/>.
/// </summary>
public sealed class UnlockViewModel : ViewModelBase
{
    private readonly ISender _mediator;

    private string _userName = string.Empty;
    private string _password = string.Empty;
    private string _errorMessage = string.Empty;
    private bool _isBusy;
    private bool _isUnlocked;

    public UnlockViewModel(ISender mediator)
    {
        _mediator = mediator;
    }

    public void Initialize(string userName)
    {
        UserName = userName;
    }

    public string UserName
    {
        get => _userName;
        private set => SetProperty(ref _userName, value);
    }

    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
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

    public bool IsUnlocked
    {
        get => _isUnlocked;
        private set => SetProperty(ref _isUnlocked, value);
    }

    public async Task<bool> UnlockAsync()
    {
        ErrorMessage = string.Empty;

        IsBusy = true;
        try
        {
            var result = await _mediator.Send(new SignInCommand(UserName, Password));

            if (result.IsSuccess)
            {
                IsUnlocked = true;
                return true;
            }

            ErrorMessage = "كلمة المرور غير صحيحة";
            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
