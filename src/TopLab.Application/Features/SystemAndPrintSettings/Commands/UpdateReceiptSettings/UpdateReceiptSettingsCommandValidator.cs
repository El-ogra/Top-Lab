using FluentValidation;
using TopLab.Domain.Common.Enums;

namespace TopLab.Application.Features.SystemAndPrintSettings.Commands.UpdateReceiptSettings;

public sealed class UpdateReceiptSettingsCommandValidator : AbstractValidator<UpdateReceiptSettingsCommand>
{
    public UpdateReceiptSettingsCommandValidator()
    {
        RuleFor(x => x.TopMarginCm)
            .InclusiveBetween(0, 30).WithMessage("الهامش العلوي يجب أن يكون بين 0 و 30 سم.");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("العملة مطلوبة.")
            .MaximumLength(10).WithMessage("العملة يجب ألا تتجاوز 10 أحرف.");
    }
}