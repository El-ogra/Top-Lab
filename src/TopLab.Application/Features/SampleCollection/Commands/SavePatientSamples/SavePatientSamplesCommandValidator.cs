using FluentValidation;

namespace TopLab.Application.Features.SampleCollection.Commands.SavePatientSamples;

public sealed class SavePatientSamplesCommandValidator : AbstractValidator<SavePatientSamplesCommand>
{
    public SavePatientSamplesCommandValidator()
    {
        RuleFor(x => x.PatientId).GreaterThan(0).WithMessage("معرف غير صالح.");
    }
}