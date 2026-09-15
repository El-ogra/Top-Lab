using FluentValidation;

namespace TopLab.Application.Features.ReportProduction.Commands.InsertHistoryResult;

public sealed class InsertHistoryResultCommandValidator : AbstractValidator<InsertHistoryResultCommand>
{
    public InsertHistoryResultCommandValidator()
    {
        RuleFor(x => x.PatientTestId).GreaterThan(0);
        RuleFor(x => x.SourcePatientTestId).GreaterThan(0);
    }
}