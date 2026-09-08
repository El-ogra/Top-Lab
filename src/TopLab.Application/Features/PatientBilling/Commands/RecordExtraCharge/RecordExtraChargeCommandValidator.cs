using FluentValidation;

namespace TopLab.Application.Features.PatientBilling.Commands.RecordExtraCharge;

public sealed class RecordExtraChargeCommandValidator : AbstractValidator<RecordExtraChargeCommand>
{
    public RecordExtraChargeCommandValidator()
    {
        RuleFor(x => x.PatientId).GreaterThan(0).WithMessage("معرّف المريض غير صالح.");
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("المبلغ الإضافي يجب أن يكون أكبر من صفر.");
    }
}
