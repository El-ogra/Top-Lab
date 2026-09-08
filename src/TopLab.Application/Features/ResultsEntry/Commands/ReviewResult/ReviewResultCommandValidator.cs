using FluentValidation;

namespace TopLab.Application.Features.ResultsEntry.Commands.ReviewResult;

public sealed class ReviewResultCommandValidator : AbstractValidator<ReviewResultCommand>
{
    public ReviewResultCommandValidator()
    {
        RuleFor(x => x.PatientTestId).GreaterThan(0).WithMessage("معرّف التحليل غير صالح.");
    }
}
