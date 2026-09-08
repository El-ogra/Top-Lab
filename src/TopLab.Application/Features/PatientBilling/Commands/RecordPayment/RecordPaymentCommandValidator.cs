using FluentValidation;

namespace TopLab.Application.Features.PatientBilling.Commands.RecordPayment;

public sealed class RecordPaymentCommandValidator : AbstractValidator<RecordPaymentCommand>
{
    public RecordPaymentCommandValidator()
    {
        RuleFor(x => x.PatientId).GreaterThan(0).WithMessage("معرّف المريض غير صالح.");
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("مبلغ الدفع يجب أن يكون أكبر من صفر.");
        RuleFor(x => x.DiscountAmount)
            .GreaterThan(0).WithMessage("الخصم يجب أن يكون أكبر من صفر ولا يتجاوز مبلغ الدفع.")
            .LessThanOrEqualTo(x => x.Amount).WithMessage("الخصم يجب أن يكون أكبر من صفر ولا يتجاوز مبلغ الدفع.")
            .When(x => x.DiscountAmount.HasValue);
    }
}
