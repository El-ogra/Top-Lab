using FluentValidation;

namespace TopLab.Application.Features.ProfileResults.Commands.MarkProfilePrinted;

public sealed class MarkProfilePrintedCommandValidator : AbstractValidator<MarkProfilePrintedCommand>
{
    public MarkProfilePrintedCommandValidator()
    {
        RuleFor(x => x.PatientTestId).GreaterThan(0).WithMessage("معرف تحليل المريض غير صالح.");
    }
}
