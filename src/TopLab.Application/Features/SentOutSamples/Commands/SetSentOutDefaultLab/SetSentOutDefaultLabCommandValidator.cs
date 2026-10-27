using FluentValidation;

namespace TopLab.Application.Features.SentOutSamples.Commands.SetSentOutDefaultLab;

public sealed class SetSentOutDefaultLabCommandValidator : AbstractValidator<SetSentOutDefaultLabCommand>
{
    public SetSentOutDefaultLabCommandValidator()
    {
        RuleFor(x => x.ExternalLabEntityId).Must(v => v is null || v > 0).WithMessage("الجهة غير صالحة.");
    }
}