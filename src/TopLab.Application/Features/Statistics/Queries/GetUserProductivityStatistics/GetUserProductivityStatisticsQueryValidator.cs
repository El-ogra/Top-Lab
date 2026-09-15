using FluentValidation;

namespace TopLab.Application.Features.Statistics.Queries.GetUserProductivityStatistics;

public sealed class GetUserProductivityStatisticsQueryValidator : AbstractValidator<GetUserProductivityStatisticsQuery>
{
    public GetUserProductivityStatisticsQueryValidator()
    {
        RuleFor(x => x)
            .Must(q => !q.From.HasValue || !q.To.HasValue || q.From.Value <= q.To.Value)
            .WithMessage("بداية الفترة يجب ألا تتجاوز نهايتها.");
    }
}
