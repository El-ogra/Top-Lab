using FluentValidation;

namespace TopLab.Application.Features.SampleCollection.Commands.MarkAllSamplesDrawnForPatient;

public sealed class MarkAllSamplesDrawnForPatientCommandValidator : AbstractValidator<MarkAllSamplesDrawnForPatientCommand>
{
    public MarkAllSamplesDrawnForPatientCommandValidator()
    {
        RuleFor(x => x.PatientId).GreaterThan(0).WithMessage("معرّف المريض غير صالح.");
    }
}
