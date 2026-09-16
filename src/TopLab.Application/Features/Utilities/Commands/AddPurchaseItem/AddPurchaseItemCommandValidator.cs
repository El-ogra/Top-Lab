using FluentValidation;

namespace TopLab.Application.Features.Utilities.Commands.AddPurchaseItem;

public sealed class AddPurchaseItemCommandValidator : AbstractValidator<AddPurchaseItemCommand>
{
    public AddPurchaseItemCommandValidator()
    {
        RuleFor(x => x.Text)
            .NotEmpty()
            .WithMessage("نص البند مطلوب.")
            .MaximumLength(200);
    }
}
