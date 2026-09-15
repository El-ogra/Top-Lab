using FluentValidation;

namespace TopLab.Application.Features.SentOutSamples.Commands.SendSampleOut;

public sealed class SendSampleOutCommandValidator : AbstractValidator<SendSampleOutCommand>
{
    public SendSampleOutCommandValidator()
    {
        RuleFor(x => x.PatientTestId)
            .GreaterThan(0).WithMessage("التحليل غير موجود");

        RuleFor(x => x.ExternalLabEntityId)
            .GreaterThan(0).WithMessage("الجهة الخارجية غير موجودة.");

        RuleFor(x => x.CostPrice)
            .GreaterThanOrEqualTo(0).When(x => x.CostPrice.HasValue).WithMessage("السعر يجب ألا يكون سالبًا.");

        RuleFor(x => x.PatientPrice)
            .GreaterThanOrEqualTo(0).When(x => x.PatientPrice.HasValue).WithMessage("السعر يجب ألا يكون سالبًا.");
    }
}
