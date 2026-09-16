using MediatR;
using TopLab.Application.Features.UsersAndPermissions.Commands.SignIn;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.ErrorPresentation;

namespace TopLab.Presentation.ViewModels.Setup;

public sealed class LoginViewModel : ViewModelBase
{
    private readonly ISender _mediator;
    private readonly ResultErrorPresenter _presenter;

    private string _userName = string.Empty;
    private string _password = string.Empty;
    private string _errorMessage = string.Empty;
    private bool _isBusy;

    public LoginViewModel(ISender mediator, ResultErrorPresenter presenter)
    {
        _mediator = mediator;
        _presenter = presenter;
    }

    public string UserName
    {
        get => _userName;
        set => SetProperty(ref _userName, value);
    }

    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    public async Task<bool> SignInAsync()
    {
        ErrorMessage = string.Empty;

        IsBusy = true;
        try
        {
            var result = await _mediator.Send(new SignInCommand(UserName.Trim(), Password));

            if (result.IsSuccess)
            {
                return true;
            }

            ErrorMessage = result.Error is not null ? _presenter.Present(result.Error) : "فشل تسجيل الدخول.";
            if (result.Errors.Count > 1)
            {
                var all = _presenter.PresentAll(result.Errors);
                ErrorMessage = string.Join(Environment.NewLine, all);
            }

            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
