using FluentValidation;

namespace TopLab.Application.Features.PatientRegistration.Commands.PrintBarcode;

public sealed class PrintBarcodeCommandValidator : AbstractValidator<PrintBarcodeCommand>
{
    public PrintBarcodeCommandValidator()
    {
        RuleFor(x => x.PatientId)
            .GreaterThan(0).WithMessage("معرّف المريض غير صالح.");
    }
}
