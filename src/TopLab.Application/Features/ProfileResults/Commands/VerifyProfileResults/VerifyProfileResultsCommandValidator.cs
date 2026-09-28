using FluentValidation;

namespace TopLab.Application.Features.ProfileResults.Commands.VerifyProfileResults;

public sealed class VerifyProfileResultsCommandValidator : AbstractValidator<VerifyProfileResultsCommand>
{
    public VerifyProfileResultsCommandValidator()
    {
        RuleFor(x => x.PatientTestId).GreaterThan(0).WithMessage("معرف تحليل المريض غير صالح.");
    }
}
