using FluentValidation;

namespace TopLab.Application.Features.ResultsEntry.Commands.RefreshResultReferenceRange;

public sealed class RefreshResultReferenceRangeCommandValidator : AbstractValidator<RefreshResultReferenceRangeCommand>
{
    public RefreshResultReferenceRangeCommandValidator()
    {
        RuleFor(x => x.PatientTestId).GreaterThan(0).WithMessage("معرّف التحليل غير صالح.");
    }
}
