using FluentValidation;

namespace TopLab.Application.Features.ReportProduction.Commands.BuildCombinedReport;

public sealed class BuildCombinedReportCommandValidator : AbstractValidator<BuildCombinedReportCommand>
{
    public BuildCombinedReportCommandValidator()
    {
        RuleFor(x => x.PatientId).GreaterThan(0);
        RuleFor(x => x.OrderedPatientTestIds).NotEmpty().WithMessage("قائمة التحاليل مطلوبة.");
    }
}