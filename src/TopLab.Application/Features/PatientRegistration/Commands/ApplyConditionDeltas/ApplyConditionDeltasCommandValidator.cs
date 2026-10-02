using FluentValidation;

namespace TopLab.Application.Features.PatientRegistration.Commands.ApplyConditionDeltas;

public sealed class ApplyConditionDeltasCommandValidator : AbstractValidator<ApplyConditionDeltasCommand>
{
    public ApplyConditionDeltasCommandValidator()
    {
        RuleFor(x => x.PatientId).GreaterThan(0).WithMessage("معرّف المريض غير صالح.");
        RuleForEach(x => x.WantedMedicalConditionTypeIds).GreaterThan(0).WithMessage("معرّف الحالة الصحية غير صالح.");
    }
}
