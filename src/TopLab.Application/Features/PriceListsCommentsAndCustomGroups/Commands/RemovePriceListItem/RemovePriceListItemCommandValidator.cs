using FluentValidation;

namespace TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.RemovePriceListItem;

public sealed class RemovePriceListItemCommandValidator : AbstractValidator<RemovePriceListItemCommand>
{
    public RemovePriceListItemCommandValidator()
    {
        RuleFor(x => x.PriceListId)
            .GreaterThan(0).WithMessage("معرف قائمة الأسعار غير صالح.");

        RuleFor(x => x.TestId)
            .GreaterThan(0).WithMessage("معرف التحليل غير صالح.");
    }
}
