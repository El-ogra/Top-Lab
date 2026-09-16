using FluentValidation;

namespace TopLab.Application.Features.Utilities.Commands.RemovePurchaseItem;

public sealed class RemovePurchaseItemCommandValidator : AbstractValidator<RemovePurchaseItemCommand>
{
    public RemovePurchaseItemCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
