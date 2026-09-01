using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.UsersAndPermissions.Commands.CreateUser;
using TopLab.Presentation.Common;
using TopLab.Presentation.Common.ErrorPresentation;

namespace TopLab.Presentation.ViewModels.Setup;

public sealed class FirstRunAdminViewModel : ViewModelBase
{
    private readonly ISender _mediator;
    private readonly ResultErrorPresenter _presenter;

    private string _userName = string.Empty;
    private string _password = string.Empty;
    private string _confirmPassword = string.Empty;
    private string _secondaryPassword = string.Empty;
    private string _confirmSecondaryPassword = string.Empty;
    private string _errorMessage = string.Empty;
    private bool _isBusy;

    public FirstRunAdminViewModel(ISender mediator, ResultErrorPresenter presenter)
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

    public string ConfirmPassword
    {
        get => _confirmPassword;
        set => SetProperty(ref _confirmPassword, value);
    }

    public string SecondaryPassword
    {
        get => _secondaryPassword;
        set => SetProperty(ref _secondaryPassword, value);
    }

    public string ConfirmSecondaryPassword
    {
        get => _confirmSecondaryPassword;
        set => SetProperty(ref _confirmSecondaryPassword, value);
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

    public async Task<bool> CreateAsync()
    {
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(UserName))
        {
            ErrorMessage = "اسم المستخدم مطلوب.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(Password) || Password.Length < 6)
        {
            ErrorMessage = "كلمة المرور يجب ألا تقل عن 6 أحرف.";
            return false;
        }

        if (Password != ConfirmPassword)
        {
            ErrorMessage = "تأكيد كلمة المرور غير متطابق.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(SecondaryPassword) || SecondaryPassword.Length < 6)
        {
            ErrorMessage = "كلمة المرور الثانوية يجب ألا تقل عن 6 أحرف.";
            return false;
        }

        if (SecondaryPassword != ConfirmSecondaryPassword)
        {
            ErrorMessage = "تأكيد كلمة المرور الثانوية غير متطابق.";
            return false;
        }

        IsBusy = true;
        try
        {
            var cmd = new CreateUserCommand(
                UserName.Trim(),
                Password,
                SecondaryPassword,
                IsAbsolutePermission: true,
                DiscountLimitPercent: 0,
                BlockPrintOnRemainingBalance: false,
                WorkStartTime: null,
                WorkEndTime: null,
                HasBreakPeriod: false,
                BreakDurationMinutes: null,
                PermissionCodes: Array.Empty<string>());

            var result = await _mediator.Send(cmd);

            if (result.IsSuccess)
            {
                return true;
            }

            ErrorMessage = result.Error is not null ? _presenter.Present(result.Error) : "فشل إنشاء المستخدم.";
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
