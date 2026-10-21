using FluentValidation;

namespace TopLab.Application.Features.PatientRegistration.Commands.RenameMedicalConditionType;

public sealed class RenameMedicalConditionTypeCommandValidator : AbstractValidator<RenameMedicalConditionTypeCommand>
{
    public RenameMedicalConditionTypeCommandValidator()
    {
        RuleFor(x => x.MedicalConditionTypeId)
            .GreaterThan(0).WithMessage("نوع الحالة الصحية غير صالح.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("اسم نوع الحالة الصحية مطلوب.")
            .MaximumLength(100).WithMessage("اسم نوع الحالة الصحية يجب ألا يتجاوز 100 حرف.");
    }
}
