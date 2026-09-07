using FluentValidation;

namespace TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.SetPriceListItemPrice;

public sealed class SetPriceListItemPriceCommandValidator : AbstractValidator<SetPriceListItemPriceCommand>
{
    public SetPriceListItemPriceCommandValidator()
    {
        RuleFor(x => x.PriceListId)
            .GreaterThan(0).WithMessage("معرف قائمة الأسعار غير صالح.");

        RuleFor(x => x.TestId)
            .GreaterThan(0).WithMessage("معرف التحليل غير صالح.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("السعر يجب أن يكون صفرًا أو أكثر.");
    }
}
