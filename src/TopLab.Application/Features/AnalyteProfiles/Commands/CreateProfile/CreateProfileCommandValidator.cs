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

        // GAP-017: optional, so null/blank is always valid; the existing colour convention
        // caps a colour string at 9 characters.
        RuleFor(x => x.NormalFont)
            .MaximumLength(50).WithMessage("لون خط النتيجة الطبيعية للبروفايل لا يمكن أن يتجاوز 50 حرف.")
            .When(x => !string.IsNullOrWhiteSpace(x.NormalFont));

        RuleFor(x => x.NormalColor)
            .MaximumLength(9).WithMessage("لون خلفية النتيجة الطبيعية للبروفايل لا يمكن أن يتجاوز 9 أحرف.")
            .When(x => !string.IsNullOrWhiteSpace(x.NormalColor));

        RuleFor(x => x.AbnormalFont)
            .MaximumLength(50).WithMessage("لون خط النتيجة غير الطبيعية للبروفايل لا يمكن أن يتجاوز 50 حرف.")
            .When(x => !string.IsNullOrWhiteSpace(x.AbnormalFont));

        RuleFor(x => x.AbnormalBackgroundColor)
            .MaximumLength(9).WithMessage("لون خلفية النتيجة غير الطبيعية للبروفايل لا يمكن أن يتجاوز 9 أحرف.")
            .When(x => !string.IsNullOrWhiteSpace(x.AbnormalBackgroundColor));
    }
}