using FluentValidation;

namespace TopLab.Application.Features.ResultsEntry.Commands.MarkAllPatientResultsReviewed;

public sealed class MarkAllPatientResultsReviewedCommandValidator : AbstractValidator<MarkAllPatientResultsReviewedCommand>
{
    public MarkAllPatientResultsReviewedCommandValidator()
    {
        RuleFor(x => x.PatientId).GreaterThan(0).WithMessage("معرّف المريض غير صالح.");
    }
}
