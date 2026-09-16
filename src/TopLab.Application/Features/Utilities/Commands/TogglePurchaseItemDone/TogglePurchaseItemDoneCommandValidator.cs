using FluentValidation;

namespace TopLab.Application.Features.Utilities.Commands.TogglePurchaseItemDone;

public sealed class TogglePurchaseItemDoneCommandValidator : AbstractValidator<TogglePurchaseItemDoneCommand>
{
    public TogglePurchaseItemDoneCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
