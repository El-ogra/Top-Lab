using FluentValidation;

namespace TopLab.Application.Features.ReportProduction.Commands.BuildBlankReport;

public sealed class BuildBlankReportCommandValidator : AbstractValidator<BuildBlankReportCommand>
{
    public BuildBlankReportCommandValidator()
    {
        RuleFor(x => x.PatientId).GreaterThan(0);
    }
}