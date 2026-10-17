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

        RuleFor(x => x.HeaderFontFamily)
            .MaximumLength(100).WithMessage("خط رأس الإيصال لا يمكن أن يتجاوز 100 حرف.")
            .When(x => !string.IsNullOrWhiteSpace(x.HeaderFontFamily));

        RuleFor(x => x.HeaderFontSizePt)
            .InclusiveBetween(4, 72).WithMessage("حجم خط رأس الإيصال يجب أن يكون بين 4 و 72 نقطة.")
            .When(x => x.HeaderFontSizePt.HasValue);

        RuleFor(x => x.HeaderFontColor)
            .MaximumLength(9).WithMessage("لون خط رأس الإيصال لا يمكن أن يتجاوز 9 أحرف.")
            .When(x => !string.IsNullOrWhiteSpace(x.HeaderFontColor));

        RuleFor(x => x.FooterFontFamily)
            .MaximumLength(100).WithMessage("خط تذييل الإيصال لا يمكن أن يتجاوز 100 حرف.")
            .When(x => !string.IsNullOrWhiteSpace(x.FooterFontFamily));

        RuleFor(x => x.FooterFontSizePt)
            .InclusiveBetween(4, 72).WithMessage("حجم خط تذييل الإيصال يجب أن يكون بين 4 و 72 نقطة.")
            .When(x => x.FooterFontSizePt.HasValue);

        RuleFor(x => x.FooterFontColor)
            .MaximumLength(9).WithMessage("لون خط تذييل الإيصال لا يمكن أن يتجاوز 9 أحرف.")
            .When(x => !string.IsNullOrWhiteSpace(x.FooterFontColor));

        RuleFor(x => x.LogoImagePath)
            .MaximumLength(300).WithMessage("مسار شعار المعمل لا يمكن أن يتجاوز 300 حرف.")
            .When(x => !string.IsNullOrWhiteSpace(x.LogoImagePath));
    }
}
