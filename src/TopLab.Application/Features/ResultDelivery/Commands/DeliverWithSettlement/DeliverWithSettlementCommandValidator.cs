using FluentValidation;

namespace TopLab.Application.Features.ResultDelivery.Commands.DeliverWithSettlement;

public sealed class DeliverWithSettlementCommandValidator : AbstractValidator<DeliverWithSettlementCommand>
{
    public DeliverWithSettlementCommandValidator()
    {
        RuleFor(x => x.PatientId).GreaterThan(0).WithMessage("معرّف المريض غير صالح.");

        RuleFor(x => x.PatientTestIds)
            .NotEmpty().WithMessage("حدد نتيجة واحدة على الأقل للتسليم.");

        When(x => x.SettleAmount.HasValue, () =>
        {
            RuleFor(x => x.SettleAmount!.Value)
                .GreaterThan(0).WithMessage("مبلغ التسوية غير صالح.");
        });

        RuleFor(x => x)
            .Must(q => !(q.SettleInFull && q.SettleAmount.HasValue && q.SettleAmount.Value > 0))
            .WithMessage("مبلغ التسوية غير صالح.");
    }
}
