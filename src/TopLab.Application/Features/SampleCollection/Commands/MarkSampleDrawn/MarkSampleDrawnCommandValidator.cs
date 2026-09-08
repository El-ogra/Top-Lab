using FluentValidation;

namespace TopLab.Application.Features.SampleCollection.Commands.MarkSampleDrawn;

public sealed class MarkSampleDrawnCommandValidator : AbstractValidator<MarkSampleDrawnCommand>
{
    public MarkSampleDrawnCommandValidator()
    {
        RuleFor(x => x.PatientTestId).GreaterThan(0).WithMessage("معرّف تحليل المريض غير صالح.");
    }
}
