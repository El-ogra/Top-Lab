using FluentValidation;

namespace TopLab.Application.Features.PatientEnvelope.Commands.PrintLabOrder;

public sealed class PrintLabOrderCommandValidator : AbstractValidator<PrintLabOrderCommand>
{
    public PrintLabOrderCommandValidator()
    {
        RuleFor(x => x.PatientId)
            .GreaterThan(0).WithMessage("معرّف المريض غير صالح.");
    }
}
