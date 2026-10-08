using FluentValidation;

namespace TopLab.Application.Features.PatientEnvelope.Commands.PrintEnvelope;

public sealed class PrintEnvelopeCommandValidator : AbstractValidator<PrintEnvelopeCommand>
{
    public PrintEnvelopeCommandValidator()
    {
        RuleFor(x => x.PatientId)
            .GreaterThan(0).WithMessage("معرّف المريض غير صالح.");
    }
}
