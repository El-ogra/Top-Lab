using FluentValidation;

namespace TopLab.Application.Features.PatientRegistration.Commands.ApplyVisitDeltas;

public sealed class ApplyVisitDeltasCommandValidator : AbstractValidator<ApplyVisitDeltasCommand>
{
    public ApplyVisitDeltasCommandValidator()
    {
        RuleFor(x => x.PatientId).GreaterThan(0).WithMessage("معرّف المريض غير صالح.");
        RuleForEach(x => x.RemovedPatientTestIds).GreaterThan(0).WithMessage("معرّف التحليل غير صالح.");
        RuleForEach(x => x.AddedTests).Must(t => t.TestId > 0).WithMessage("معرّف التحليل غير صالح.");
        RuleForEach(x => x.FlagUpdates).Must(f => f.PatientTestId > 0).WithMessage("معرّف التحليل غير صالح.");
    }
}
