using FluentValidation;

namespace TopLab.Application.Features.PatientBilling.Commands.PrintInvoice;

public sealed class PrintInvoiceCommandValidator : AbstractValidator<PrintInvoiceCommand>
{
    public PrintInvoiceCommandValidator()
    {
        RuleFor(x => x.PatientId)
            .GreaterThan(0).WithMessage("معرّف المريض غير صالح.");
    }
}
