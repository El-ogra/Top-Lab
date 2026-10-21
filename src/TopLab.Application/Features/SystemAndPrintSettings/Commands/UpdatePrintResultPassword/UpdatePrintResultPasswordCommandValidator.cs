using FluentValidation;

namespace TopLab.Application.Features.SystemAndPrintSettings.Commands.UpdatePrintResultPassword;

public sealed class UpdatePrintResultPasswordCommandValidator : AbstractValidator<UpdatePrintResultPasswordCommand>
{
    public UpdatePrintResultPasswordCommandValidator()
    {
        RuleFor(x => x.Password)
            .MaximumLength(200).WithMessage("كلمة مرور الطباعة طويلة جدًا.")
            .When(x => x.Enabled);
    }
}
