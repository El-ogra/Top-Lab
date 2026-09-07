using FluentValidation;

namespace TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.SetCustomGroupItemPrice;

public sealed class SetCustomGroupItemPriceCommandValidator : AbstractValidator<SetCustomGroupItemPriceCommand>
{
    public SetCustomGroupItemPriceCommandValidator()
    {
        RuleFor(x => x.CustomGroupId)
            .GreaterThan(0).WithMessage("معرف المجموعة غير صالح.");

        RuleFor(x => x.TestId)
            .GreaterThan(0).WithMessage("معرف التحليل غير صالح.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("السعر يجب أن يكون صفرًا أو أكثر.");
    }
}
