using FluentValidation;

namespace TopLab.Application.Features.ResultsEntry.Commands.BulkPrint;

public sealed class ExecuteBulkPrintCommandValidator : AbstractValidator<ExecuteBulkPrintCommand>
{
    public ExecuteBulkPrintCommandValidator()
    {
        RuleFor(x => x.Decisions)
            .NotEmpty().WithMessage("قائمة القرارات مطلوبة.");

        RuleForEach(x => x.Decisions).ChildRules(d =>
        {
            d.RuleFor(v => v.PatientId).GreaterThan(0).WithMessage("معرّف المريض غير صالح.");
        });
    }
}
