using FluentValidation;

namespace TopLab.Application.Features.ResultsEntry.Commands.EnterResult;

public sealed class EnterResultCommandValidator : AbstractValidator<EnterResultCommand>
{
    public EnterResultCommandValidator()
    {
        RuleFor(x => x.PatientTestId).GreaterThan(0).WithMessage("معرّف التحليل غير صالح.");

        RuleFor(x => x.ResultValue)
            .Must(v => !string.IsNullOrWhiteSpace(v))
            .WithMessage("الرجاء إدخال قيمة النتيجة قبل الحفظ");

        When(x => x.ResultFlag.HasValue, () =>
        {
            RuleFor(x => x.ResultFlag!.Value)
                .InclusiveBetween(0, 2).WithMessage("قيمة العلم غير صالحة.");
        });
    }
}
