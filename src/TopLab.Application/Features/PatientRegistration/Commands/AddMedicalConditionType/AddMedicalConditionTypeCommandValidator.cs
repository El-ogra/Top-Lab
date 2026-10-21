using FluentValidation;
using TopLab.Domain.Common.Enums;

namespace TopLab.Application.Features.PatientRegistration.Commands.AddMedicalConditionType;

public sealed class AddMedicalConditionTypeCommandValidator : AbstractValidator<AddMedicalConditionTypeCommand>
{
    public AddMedicalConditionTypeCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("اسم نوع الحالة الصحية مطلوب.")
            .MaximumLength(100).WithMessage("اسم نوع الحالة الصحية يجب ألا يتجاوز 100 حرف.");

        RuleFor(x => x.Category)
            .IsInEnum().WithMessage("تصنيف نوع الحالة الصحية غير صالح.");
    }
}
