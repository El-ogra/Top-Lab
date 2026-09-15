using FluentValidation;

namespace TopLab.Application.Features.Statistics.Queries.GetSentOutStatistics;

public sealed class GetSentOutStatisticsQueryValidator : AbstractValidator<GetSentOutStatisticsQuery>
{
    public GetSentOutStatisticsQueryValidator()
    {
        RuleFor(x => x)
            .Must(q => !q.From.HasValue || !q.To.HasValue || q.From.Value <= q.To.Value)
            .WithMessage("بداية الفترة يجب ألا تتجاوز نهايتها.");
    }
}
