using FluentValidation;

namespace TopLab.Application.Features.ReportProduction.Commands.PrintBlankReport;

public sealed class PrintBlankReportCommandValidator : AbstractValidator<PrintBlankReportCommand>
{
    public PrintBlankReportCommandValidator()
    {
        RuleFor(x => x.PatientId).GreaterThan(0).WithMessage("معرّف المريض غير صالح.");
    }
}