using FluentValidation;
using TopLab.Domain.Common.Enums;

namespace TopLab.Application.Features.SystemAndPrintSettings.Commands.UpdateReportSettings;

public sealed class UpdateReportSettingsCommandValidator : AbstractValidator<UpdateReportSettingsCommand>
{
    public UpdateReportSettingsCommandValidator()
    {
        RuleFor(x => x.PageMarginLeftCm)
            .InclusiveBetween(0, 30).WithMessage("الهامش الأيسر يجب أن يكون بين 0 و 30 سم.");

        RuleFor(x => x.PageMarginBottomCm)
            .InclusiveBetween(0, 30).WithMessage("الهامش السفلي يجب أن يكون بين 0 و 30 سم.");

        RuleFor(x => x.ReportTopSpaceCm)
            .LessThanOrEqualTo(8m).WithMessage("الهامش العلوي للتقرير لا يمكن أن يتجاوز 8 سم")
            .GreaterThanOrEqualTo(0m).WithMessage("الهامش العلوي للتقرير يجب أن يكون أكبر من أو يساوي 0.");

        RuleFor(x => x.HeaderColor)
            .MaximumLength(9).WithMessage("لون الرأس لا يمكن أن يتجاوز 9 أحرف.")
            .When(x => !string.IsNullOrWhiteSpace(x.HeaderColor));

        RuleFor(x => x.FooterColor)
            .MaximumLength(9).WithMessage("لون التذييل لا يمكن أن يتجاوز 9 أحرف.")
            .When(x => !string.IsNullOrWhiteSpace(x.FooterColor));

        RuleFor(x => x.HeaderImagePath)
            .MaximumLength(300).WithMessage("مسار صورة الرأس لا يمكن أن يتجاوز 300 حرف.")
            .When(x => !string.IsNullOrWhiteSpace(x.HeaderImagePath));

        RuleFor(x => x.FooterImagePath)
            .MaximumLength(300).WithMessage("مسار صورة التذييل لا يمكن أن يتجاوز 300 حرف.")
            .When(x => !string.IsNullOrWhiteSpace(x.FooterImagePath));

        RuleFor(x => x.NormalRangeMarker)
            .MaximumLength(10).WithMessage("علامة المدى الطبيعي لا يمكن أن تتجاوز 10 أحرف.")
            .When(x => !string.IsNullOrWhiteSpace(x.NormalRangeMarker));

        RuleFor(x => x.AbnormalResultFontColor)
            .MaximumLength(9).WithMessage("لون خط النتيجة غير الطبيعية لا يمكن أن يتجاوز 9 أحرف.")
            .When(x => !string.IsNullOrWhiteSpace(x.AbnormalResultFontColor));

        RuleFor(x => x.AbnormalResultBackgroundColor)
            .MaximumLength(9).WithMessage("لون خلفية النتيجة غير الطبيعية لا يمكن أن يتجاوز 9 أحرف.")
            .When(x => !string.IsNullOrWhiteSpace(x.AbnormalResultBackgroundColor));
    }
}