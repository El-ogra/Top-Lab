using FluentValidation;

namespace TopLab.Application.Features.ResultsEntry.Commands.UnreviewResult;

public sealed class UnreviewResultCommandValidator : AbstractValidator<UnreviewResultCommand>
{
    public UnreviewResultCommandValidator()
    {
        RuleFor(x => x.PatientTestId).GreaterThan(0).WithMessage("معرّف التحليل غير صالح.");
    }
}
