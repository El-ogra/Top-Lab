using FluentValidation;

namespace TopLab.Application.Features.ProfileResults.Commands.SaveProfileResults;

public sealed class SaveProfileResultsCommandValidator : AbstractValidator<SaveProfileResultsCommand>
{
    public SaveProfileResultsCommandValidator()
    {
        RuleFor(x => x.PatientTestId).GreaterThan(0).WithMessage("معرّف الزيارة غير صالح.");

        RuleFor(x => x.Items)
            .NotNull().WithMessage("قائمة النتائج مطلوبة.")
            .Must(items => items is not null && items.Count > 0).WithMessage("يجب إدخال نتيجة مادة واحدة على الأقل.")
            .Must(items => items is null || items.Select(i => i.AnalyteId).Distinct().Count() == items.Count).WithMessage("لا يمكن تكرار المادة التحليلية.");

        RuleForEach(x => x.Items)
            .ChildRules(item =>
            {
                item.RuleFor(i => i.ResultValue).NotEmpty().WithMessage("قيمة النتيجة مطلوبة.");
            });
    }
}