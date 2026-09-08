using FluentValidation;

namespace TopLab.Application.Features.PatientBilling.Commands.RecordCorrection;

public sealed class RecordCorrectionCommandValidator : AbstractValidator<RecordCorrectionCommand>
{
    public RecordCorrectionCommandValidator()
    {
        RuleFor(x => x.PatientId).GreaterThan(0).WithMessage("معرّف المريض غير صالح.");
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("مبلغ القيد التصحيحي يجب أن يكون أكبر من صفر.");
    }
}
