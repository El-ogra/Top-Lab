using FluentValidation;

namespace TopLab.Application.Features.ReportProduction.Commands.PrintCombinedReport;

public sealed class PrintCombinedReportCommandValidator : AbstractValidator<PrintCombinedReportCommand>
{
    public PrintCombinedReportCommandValidator()
    {
        RuleFor(x => x.PatientId).GreaterThan(0).WithMessage("معرّف المريض غير صالح.");
        RuleFor(x => x.OrderedPatientTestIds).NotEmpty().WithMessage("قائمة التحاليل مطلوبة.");
    }
}