using FluentValidation;

namespace TopLab.Application.Features.SentOutSamples.Commands.SettleSentOutInFull;

public sealed class SettleSentOutInFullCommandValidator : AbstractValidator<SettleSentOutInFullCommand>
{
    public SettleSentOutInFullCommandValidator()
    {
        RuleFor(x => x.SentOutSampleId)
            .GreaterThan(0).WithMessage("العينة المُرسَلة غير موجودة.");
    }
}
