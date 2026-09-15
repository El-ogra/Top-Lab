using FluentValidation;

namespace TopLab.Application.Features.SentOutSamples.Commands.RecordSentOutPayment;

public sealed class RecordSentOutPaymentCommandValidator : AbstractValidator<RecordSentOutPaymentCommand>
{
    public RecordSentOutPaymentCommandValidator()
    {
        RuleFor(x => x.SentOutSampleId)
            .GreaterThan(0).WithMessage("العينة المُرسَلة غير موجودة.");

        RuleFor(x => x.AmountPaid)
            .GreaterThan(0).WithMessage("مبلغ الدفع يجب أن يكون أكبر من صفر.");
    }
}
