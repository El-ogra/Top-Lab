using FluentValidation;

namespace TopLab.Application.Features.ReportProduction.Commands.AutoInsertHistory;

public sealed class AutoInsertHistoryCommandValidator : AbstractValidator<AutoInsertHistoryCommand>
{
    public AutoInsertHistoryCommandValidator()
    {
        RuleFor(x => x.PatientTestId).GreaterThan(0);
    }
}