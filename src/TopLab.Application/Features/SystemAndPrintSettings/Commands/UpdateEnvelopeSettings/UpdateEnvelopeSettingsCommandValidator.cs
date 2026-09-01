using FluentValidation;
using TopLab.Domain.Common.Enums;

namespace TopLab.Application.Features.SystemAndPrintSettings.Commands.UpdateEnvelopeSettings;

public sealed class UpdateEnvelopeSettingsCommandValidator : AbstractValidator<UpdateEnvelopeSettingsCommand>
{
    private static readonly string[] KnownItems = ["Name", "Code", "ReferralEntity", "Date"];

    public UpdateEnvelopeSettingsCommandValidator()
    {
        RuleFor(x => x.TopMarginCm)
            .InclusiveBetween(0, 30).WithMessage("الهامش العلوي يجب أن يكون بين 0 و 30 سم.");

        RuleFor(x => x.Positions)
            .NotNull().WithMessage("مواضع العناصر مطلوبة.");

        RuleFor(x => x)
            .Must(HasExactlyKnownNames).WithMessage("مواضع عناصر المظروف يجب أن تطابق العناصر المعروفة الأربعة.")
            .When(x => x.Positions != null);

        RuleForEach(x => x.Positions)
            .Must(p => p.LeftOffsetCm >= 0m && p.LeftOffsetCm <= 30m)
            .WithMessage("الإزاحة الأفقية يجب أن تكون بين 0 و 30 سم.")
            .Must(p => p.TopOffsetCm >= 0m && p.TopOffsetCm <= 30m)
            .WithMessage("الإزاحة الرأسية يجب أن تكون بين 0 و 30 سم.");
    }

    private static bool HasExactlyKnownNames(UpdateEnvelopeSettingsCommand cmd)
    {
        if (cmd.Positions is null)
        {
            return false;
        }

        var names = cmd.Positions.Select(p => p.ItemName).ToList();
        if (names.Count != KnownItems.Length)
        {
            return false;
        }

        return KnownItems.All(names.Contains) && names.Distinct().Count() == KnownItems.Length;
    }
}