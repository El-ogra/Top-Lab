using FluentValidation;

namespace TopLab.Application.Features.PatientBilling.Commands.PrintReceipt;

public sealed class PrintReceiptCommandValidator : AbstractValidator<PrintReceiptCommand>
{
    public PrintReceiptCommandValidator()
    {
        RuleFor(x => x.PatientId)
            .GreaterThan(0).WithMessage("معرّف المريض غير صالح.");
    }
}
