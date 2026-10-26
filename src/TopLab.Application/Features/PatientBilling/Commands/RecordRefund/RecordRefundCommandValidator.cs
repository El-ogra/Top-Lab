using FluentValidation;

namespace TopLab.Application.Features.PatientBilling.Commands.RecordRefund;

public sealed class RecordRefundCommandValidator : AbstractValidator<RecordRefundCommand>
{
    public RecordRefundCommandValidator()
    {
        RuleFor(x => x.PatientId).GreaterThan(0).WithMessage("معرّف المريض غير صالح.");
        RuleFor(x => x.RefundAmount).GreaterThan(0).WithMessage("مبلغ الاسترداد يجب أن يكون أكبر من صفر.");
    }
}
