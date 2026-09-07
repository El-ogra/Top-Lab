using FluentValidation;

namespace TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.CreatePriceList;

public sealed class CreatePriceListCommandValidator : AbstractValidator<CreatePriceListCommand>
{
    public CreatePriceListCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("اسم قائمة الأسعار مطلوب.")
            .MaximumLength(150).WithMessage("اسم قائمة الأسعار يجب ألا يتجاوز 150 حرفًا.");
    }
}
