using FluentValidation;

namespace TopLab.Application.Features.ProfileResults.Commands.AmendProfileResult;

public sealed class AmendProfileResultCommandValidator : AbstractValidator<AmendProfileResultCommand>
{
    public AmendProfileResultCommandValidator()
    {
        RuleFor(x => x.ProfileResultItemId).GreaterThan(0).WithMessage("معرّف نتيجة البروفايل غير صالح.");
        RuleFor(x => x.ResultValue).NotEmpty().WithMessage("قيمة النتيجة مطلوبة.");
        RuleFor(x => x.Reason).MaximumLength(500).WithMessage("ملاحظة التعديل طويلة جداً.");
    }
}