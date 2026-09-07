using FluentValidation;

namespace TopLab.Application.Features.PatientRegistration.Commands.UpdatePatientTestSampleFlags;

public sealed class UpdatePatientTestSampleFlagsCommandValidator : AbstractValidator<UpdatePatientTestSampleFlagsCommand>
{
    public UpdatePatientTestSampleFlagsCommandValidator()
    {
        RuleFor(x => x.PatientTestId).GreaterThan(0).WithMessage("معرّف تحليل المريض غير صالح.");
    }
}