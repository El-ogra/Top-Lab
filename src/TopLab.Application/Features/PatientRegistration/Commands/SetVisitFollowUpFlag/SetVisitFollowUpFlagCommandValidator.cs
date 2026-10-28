using FluentValidation;

namespace TopLab.Application.Features.PatientRegistration.Commands.SetVisitFollowUpFlag;

public sealed class SetVisitFollowUpFlagCommandValidator : AbstractValidator<SetVisitFollowUpFlagCommand>
{
    public SetVisitFollowUpFlagCommandValidator()
    {
        RuleFor(x => x.PatientId)
            .GreaterThan(0).WithMessage("المريض معرّف غير صحيح.");

        RuleFor(x => x.IsFollowUpRequired)
            .NotNull()
            .WithMessage("حالة المتابعة مطلوبة.");
    }
}
