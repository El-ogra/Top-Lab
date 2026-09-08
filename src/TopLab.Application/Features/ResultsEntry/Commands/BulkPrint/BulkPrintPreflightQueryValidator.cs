using FluentValidation;

namespace TopLab.Application.Features.ResultsEntry.Commands.BulkPrint;

public sealed class BulkPrintPreflightQueryValidator : AbstractValidator<BulkPrintPreflightQuery>
{
    public BulkPrintPreflightQueryValidator()
    {
        RuleFor(x => x.PatientIds)
            .NotEmpty().WithMessage("قائمة المرضى مطلوبة.");

        RuleForEach(x => x.PatientIds)
            .GreaterThan(0).WithMessage("معرّف المريض غير صالح.");
    }
}
