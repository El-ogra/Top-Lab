using FluentValidation;

namespace TopLab.Application.Features.UsersAndPermissions.Commands.ChangeOwnPassword;

public sealed class ChangeOwnPasswordCommandValidator : AbstractValidator<ChangeOwnPasswordCommand>
{
    public ChangeOwnPasswordCommandValidator()
    {
        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("كلمة المرور الجديدة مطلوبة (6 أحرف على الأقل)")
            .MinimumLength(6).WithMessage("كلمة المرور الجديدة مطلوبة (6 أحرف على الأقل)");

        RuleFor(x => x.ConfirmNewPassword)
            .Equal(x => x.NewPassword).WithMessage("غير متطابقتان");

        RuleFor(x => x)
            .Must(x => x.NewPassword != x.CurrentPassword).WithMessage("الجديدة يجب أن تختلف عن الحالية");
    }
}
