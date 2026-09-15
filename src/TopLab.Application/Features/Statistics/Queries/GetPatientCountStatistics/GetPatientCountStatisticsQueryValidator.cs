using FluentValidation;

namespace TopLab.Application.Features.Statistics.Queries.GetPatientCountStatistics;

public sealed class GetPatientCountStatisticsQueryValidator : AbstractValidator<GetPatientCountStatisticsQuery>
{
    public GetPatientCountStatisticsQueryValidator()
    {
        RuleFor(x => x)
            .Must(q => !q.From.HasValue || !q.To.HasValue || q.From.Value <= q.To.Value)
            .WithMessage("بداية الفترة يجب ألا تتجاوز نهايتها.");
    }
}
