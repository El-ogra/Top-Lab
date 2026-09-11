using FluentValidation;
using TopLab.Application.Features.WorkSheets.Common;

namespace TopLab.Application.Features.WorkSheets.Queries.GetWorkSheetSummary;

public sealed class GetWorkSheetSummaryQueryValidator
    : AbstractValidator<GetWorkSheetSummaryQuery>
{
    public GetWorkSheetSummaryQueryValidator()
    {
        RuleFor(x => x)
            .Must(q => WorkSheetPeriod.IsValid(q.From, q.To))
            .WithMessage("بداية الفترة يجب ألا تتجاوز نهايتها.");
    }
}
