using FluentValidation;

namespace TopLab.Application.Features.PatientRegistration.Commands.AddMedicalCondition;

public sealed class AddMedicalConditionCommandValidator : AbstractValidator<AddMedicalConditionCommand>
{
    public AddMedicalConditionCommandValidator()
    {
        RuleFor(x => x.PatientId).GreaterThan(0).WithMessage("معرّف المريض غير صالح.");
        RuleFor(x => x.MedicalConditionTypeId).GreaterThan(0).WithMessage("معرّف نوع الحالة الصحية غير صالح.");
    }
}