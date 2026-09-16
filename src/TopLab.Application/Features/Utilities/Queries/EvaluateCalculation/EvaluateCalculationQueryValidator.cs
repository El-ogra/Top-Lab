using FluentValidation;

namespace TopLab.Application.Features.Utilities.Queries.EvaluateCalculation;

public sealed class EvaluateCalculationQueryValidator : AbstractValidator<EvaluateCalculationQuery>
{
    public EvaluateCalculationQueryValidator()
    {
        RuleFor(x => x.Expression)
            .NotEmpty()
            .WithMessage("التعبير الحسابي غير صالح.");
    }
}
