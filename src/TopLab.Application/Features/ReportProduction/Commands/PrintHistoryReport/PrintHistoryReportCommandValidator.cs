using FluentValidation;

namespace TopLab.Application.Features.ReportProduction.Commands.PrintHistoryReport;

public sealed class PrintHistoryReportCommandValidator : AbstractValidator<PrintHistoryReportCommand>
{
    public PrintHistoryReportCommandValidator()
    {
        RuleFor(x => x.PatientId).GreaterThan(0).WithMessage("معرّف المريض غير صالح.");
    }
}