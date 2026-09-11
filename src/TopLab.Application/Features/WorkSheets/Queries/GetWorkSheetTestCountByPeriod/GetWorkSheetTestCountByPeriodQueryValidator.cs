using FluentValidation;
using TopLab.Application.Features.WorkSheets.Common;

namespace TopLab.Application.Features.WorkSheets.Queries.GetWorkSheetTestCountByPeriod;

public sealed class GetWorkSheetTestCountByPeriodQueryValidator
    : AbstractValidator<GetWorkSheetTestCountByPeriodQuery>
{
    public GetWorkSheetTestCountByPeriodQueryValidator()
    {
        RuleFor(x => x)
            .Must(q => WorkSheetPeriod.IsValid(q.From, q.To))
            .WithMessage("بداية الفترة يجب ألا تتجاوز نهايتها.");
    }
}
