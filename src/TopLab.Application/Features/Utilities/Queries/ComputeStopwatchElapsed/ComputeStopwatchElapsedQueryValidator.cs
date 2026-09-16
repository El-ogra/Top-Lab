using FluentValidation;

namespace TopLab.Application.Features.Utilities.Queries.ComputeStopwatchElapsed;

public sealed class ComputeStopwatchElapsedQueryValidator : AbstractValidator<ComputeStopwatchElapsedQuery>
{
    public ComputeStopwatchElapsedQueryValidator()
    {
        RuleFor(x => x.StartUtc)
            .NotEqual(default(DateTime))
            .WithMessage("وقت النهاية يسبق وقت البداية.");
    }
}
