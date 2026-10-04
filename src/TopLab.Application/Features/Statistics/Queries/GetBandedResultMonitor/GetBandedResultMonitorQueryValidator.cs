using FluentValidation;

namespace TopLab.Application.Features.Statistics.Queries.GetBandedResultMonitor;

public sealed class GetBandedResultMonitorQueryValidator : AbstractValidator<GetBandedResultMonitorQuery>
{
    public GetBandedResultMonitorQueryValidator()
    {
        RuleFor(x => x)
            .Must(q => !q.From.HasValue || !q.To.HasValue || q.From.Value <= q.To.Value)
            .WithMessage("بداية الفترة يجب ألا تتجاوز نهايتها.");

        RuleFor(x => x)
            .Must(q => q.MinValue <= q.MaxValue)
            .WithMessage("الحد الأدنى يجب ألا يتجاوز الحد الأقصى.");
    }
}