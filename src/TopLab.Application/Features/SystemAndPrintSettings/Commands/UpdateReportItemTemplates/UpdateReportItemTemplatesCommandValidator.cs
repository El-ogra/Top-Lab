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
        });
    }
}
