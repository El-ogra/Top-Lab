using System.Globalization;
using System.Text;
using FluentValidation;
using TopLab.Domain.Settings;

namespace TopLab.Application.Features.SystemAndPrintSettings.Commands.UpdateReportItemTemplates;

public sealed class UpdateReportItemTemplatesCommandValidator : AbstractValidator<UpdateReportItemTemplatesCommand>
{
    public UpdateReportItemTemplatesCommandValidator()
    {
        RuleFor(x => x.Items)
            .NotNull().WithMessage("عناصر التقرير مطلوبة.");

        RuleFor(x => x.Items)
            .Must(items => items.Select(i => i.ItemKey).Distinct(StringComparer.Ordinal).Count() == items.Count)
            .WithMessage("لا يمكن تكرار عنصر التقرير.")
            .When(x => x.Items is not null);

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ItemKey)
                .Must(key => ReportItemKeys.All.Contains(key, StringComparer.Ordinal))
                .WithMessage("عنصر التقرير غير معروف.");

            item.RuleFor(i => i.Label)
                .NotEmpty().WithMessage("التسمية مطلوبة.")
                .MaximumLength(ReportItemTemplate.MaxLabelLength)
                .WithMessage($"التسمية يجب ألا تتجاوز {ReportItemTemplate.MaxLabelLength} حرفاً.");

            item.RuleFor(i => i.ReferenceValueExpression)
                .MaximumLength(ReportItemTemplate.MaxReferenceValueExpressionLength)
                .WithMessage($"تعبير القيمة المرجعية يجب ألا يتجاوز {ReportItemTemplate.MaxReferenceValueExpressionLength} حرفاً.")
                .When(i => !string.IsNullOrEmpty(i.ReferenceValueExpression));

            item.RuleFor(i => i.ListMembership)
                .MaximumLength(ReportItemTemplate.MaxListMembershipLength)
                .WithMessage($"عضوية القائمة يجب ألا تتجاوز {ReportItemTemplate.MaxListMembershipLength} حرفاً.")
                .When(i => !string.IsNullOrEmpty(i.ListMembership));

            // GAP-015: per-item layout. Null means "keep the composer's existing
            // behaviour"; a value must be inside the documented range.
            item.RuleFor(i => i.LineSpacing)
                .InclusiveBetween(ReportItemTemplate.MinLineSpacing, ReportItemTemplate.MaxLineSpacing)
                .WithMessage($"تباعد الأسطر يجب أن يكون بين {ReportItemTemplate.MinLineSpacing} و {ReportItemTemplate.MaxLineSpacing}.")
                .When(i => i.LineSpacing is not null);

            item.RuleFor(i => i.LeftOffsetCm)
                .InclusiveBetween(ReportItemTemplate.MinLeftOffsetCm, ReportItemTemplate.MaxLeftOffsetCm)
                .WithMessage($"الإزاحة لليسار يجب أن تكون بين {ReportItemTemplate.MinLeftOffsetCm} و {ReportItemTemplate.MaxLeftOffsetCm} سم.")
                .When(i => i.LeftOffsetCm is not null);

            // GAP-015: storage allows 10 characters, the business rule is one
            // visible text element (an emoji or combining sequence may span more
            // than one UTF-16 code unit).
            item.RuleFor(i => i.MarkerSymbol)
                .Must(IsSingleVisibleMarker)
                .WithMessage("رمز العلامة يجب أن يكون حرفاً واحداً ظاهراً.")
                .When(i => i.MarkerSymbol is not null);
        });
    }

    private static bool IsSingleVisibleMarker(string? marker)
    {
        if (string.IsNullOrWhiteSpace(marker)
            || marker.Length > ReportItemTemplate.MaxMarkerSymbolLength)
        {
            return false;
        }

        if (new StringInfo(marker).LengthInTextElements != 1)
        {
            return false;
        }

        foreach (var rune in marker.EnumerateRunes())
        {
            if (Rune.IsControl(rune))
            {
                return false;
            }
        }

        return true;
    }
}
