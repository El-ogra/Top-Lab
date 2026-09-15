using FluentValidation;

namespace TopLab.Application.Features.Statistics.Queries.GetTestCountStatistics;

public sealed class GetTestCountStatisticsQueryValidator : AbstractValidator<GetTestCountStatisticsQuery>
{
    public GetTestCountStatisticsQueryValidator()
    {
        RuleFor(x => x)
            .Must(q => !q.From.HasValue || !q.To.HasValue || q.From.Value <= q.To.Value)
            .WithMessage("بداية الفترة يجب ألا تتجاوز نهايتها.");
    }
}
