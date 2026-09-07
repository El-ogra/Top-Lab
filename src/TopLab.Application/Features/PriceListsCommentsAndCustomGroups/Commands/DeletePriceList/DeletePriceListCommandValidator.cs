using FluentValidation;

namespace TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.DeletePriceList;

public sealed class DeletePriceListCommandValidator : AbstractValidator<DeletePriceListCommand>
{
    public DeletePriceListCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("معرف قائمة الأسعار غير صالح.");
    }
}
