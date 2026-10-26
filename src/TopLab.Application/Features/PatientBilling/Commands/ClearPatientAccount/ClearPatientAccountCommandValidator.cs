using FluentValidation;

namespace TopLab.Application.Features.PatientBilling.Commands.ClearPatientAccount;

public sealed class ClearPatientAccountCommandValidator : AbstractValidator<ClearPatientAccountCommand>
{
    public ClearPatientAccountCommandValidator()
    {
        RuleFor(x => x.PatientId).GreaterThan(0).WithMessage("معرّف المريض غير صالح.");
    }
}
