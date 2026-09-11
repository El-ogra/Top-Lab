using FluentValidation;
using TopLab.Application.Features.WorkSheets.Common;

namespace TopLab.Application.Features.WorkSheets.Queries.GetWorkSheetByTestGroup;

public sealed class GetWorkSheetByTestGroupQueryValidator
    : AbstractValidator<GetWorkSheetByTestGroupQuery>
{
    public GetWorkSheetByTestGroupQueryValidator()
    {
        When(x => x.TestGroupId.HasValue, () =>
        {
            RuleFor(x => x.TestGroupId!.Value)
                .GreaterThan(0).WithMessage("معرف مجموعة التحاليل غير صالح.");
        });

        RuleFor(x => x)
            .Must(q => WorkSheetPeriod.IsValid(q.From, q.To))
            .WithMessage("بداية الفترة يجب ألا تتجاوز نهايتها.");
    }
}
