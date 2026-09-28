using FluentValidation;

namespace TopLab.Application.Features.PatientRegistration.Commands.SoftDeletePatient;

public sealed class SoftDeletePatientCommandValidator : AbstractValidator<SoftDeletePatientCommand>
{
    public SoftDeletePatientCommandValidator()
    {
        RuleFor(x => x.PatientId).GreaterThan(0).WithMessage("معرف المريض غير صالح.");
    }
}
