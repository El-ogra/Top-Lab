using FluentValidation;

namespace TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.CreateCustomGroup;

public sealed class CreateCustomGroupCommandValidator : AbstractValidator<CreateCustomGroupCommand>
{
    public CreateCustomGroupCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("اسم المجموعة مطلوب.")
            .MaximumLength(150).WithMessage("اسم المجموعة يجب ألا يتجاوز 150 حرفًا.");
    }
}
