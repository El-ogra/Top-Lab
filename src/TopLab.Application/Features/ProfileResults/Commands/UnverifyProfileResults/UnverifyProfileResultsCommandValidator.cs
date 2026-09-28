using FluentValidation;

namespace TopLab.Application.Features.ProfileResults.Commands.UnverifyProfileResults;

public sealed class UnverifyProfileResultsCommandValidator : AbstractValidator<UnverifyProfileResultsCommand>
{
    public UnverifyProfileResultsCommandValidator()
    {
        RuleFor(x => x.PatientTestId).GreaterThan(0).WithMessage("معرف تحليل المريض غير صالح.");
    }
}
