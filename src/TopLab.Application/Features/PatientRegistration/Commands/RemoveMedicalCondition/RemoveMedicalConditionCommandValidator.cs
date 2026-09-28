using FluentValidation;

namespace TopLab.Application.Features.PatientRegistration.Commands.RemoveMedicalCondition;

public sealed class RemoveMedicalConditionCommandValidator : AbstractValidator<RemoveMedicalConditionCommand>
{
    public RemoveMedicalConditionCommandValidator()
    {
        RuleFor(x => x.PatientId).GreaterThan(0).WithMessage("معرف المريض غير صالح.");
        RuleFor(x => x.MedicalConditionTypeId).GreaterThan(0).WithMessage("معرف الحالة المرضية غير صالح.");
    }
}
