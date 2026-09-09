using FluentValidation;

namespace TopLab.Application.Features.AnalyteProfiles.Commands.CreateProfile;

public sealed class CreateProfileCommandValidator : AbstractValidator<CreateProfileCommand>
{
    public CreateProfileCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("اسم البروفايل مطلوب.")
            .MaximumLength(120);
        RuleFor(x => x.SpecializedTestId).GreaterThan(0).WithMessage("معرّف التحليل المتخصص غير صالح.");
        RuleFor(x => x.FixedPrice).GreaterThanOrEqualTo(0).WithMessage("السعر الثابت لا يمكن أن يكون سالباً.");

        RuleFor(x => x.AnalyteIds)
            .NotNull().WithMessage("قائمة المواد التحليلية مطلوبة.")
            .Must(ids => ids is null || ids.Distinct().Count() == ids.Count).WithMessage("لا يمكن تكرار المادة التحليلية.");
    }
}