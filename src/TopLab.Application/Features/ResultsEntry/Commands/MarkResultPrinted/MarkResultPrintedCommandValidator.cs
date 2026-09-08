using FluentValidation;

namespace TopLab.Application.Features.ResultsEntry.Commands.MarkResultPrinted;

public sealed class MarkResultPrintedCommandValidator : AbstractValidator<MarkResultPrintedCommand>
{
    public MarkResultPrintedCommandValidator()
    {
        RuleFor(x => x.PatientTestId).GreaterThan(0).WithMessage("معرّف التحليل غير صالح.");
    }
}
