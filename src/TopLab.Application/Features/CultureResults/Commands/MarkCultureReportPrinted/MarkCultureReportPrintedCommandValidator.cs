using FluentValidation;

namespace TopLab.Application.Features.CultureResults.Commands.MarkCultureReportPrinted;

public sealed class MarkCultureReportPrintedCommandValidator : AbstractValidator<MarkCultureReportPrintedCommand>
{
    public MarkCultureReportPrintedCommandValidator()
    {
        RuleFor(x => x.PatientTestId).GreaterThan(0).WithMessage("معرف تحليل المريض غير صالح.");
    }
}
