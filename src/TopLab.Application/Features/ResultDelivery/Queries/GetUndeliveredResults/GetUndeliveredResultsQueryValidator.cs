using FluentValidation;
using TopLab.Application.Features.ResultDelivery.Common;

namespace TopLab.Application.Features.ResultDelivery.Queries.GetUndeliveredResults;

public sealed class GetUndeliveredResultsQueryValidator : AbstractValidator<GetUndeliveredResultsQuery>
{
    public GetUndeliveredResultsQueryValidator()
    {
        RuleFor(x => x)
            .Must(q => ResultDeliveryPeriod.IsValid(q.From, q.To))
            .WithMessage("بداية الفترة يجب ألا تتجاوز نهايتها.");

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("معاملات الترقيم غير صالحة.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 500).WithMessage("معاملات الترقيم غير صالحة.");
    }
}
