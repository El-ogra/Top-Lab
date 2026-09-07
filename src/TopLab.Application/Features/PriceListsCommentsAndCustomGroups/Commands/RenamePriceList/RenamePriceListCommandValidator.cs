using FluentValidation;

namespace TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.RenamePriceList;

public sealed class RenamePriceListCommandValidator : AbstractValidator<RenamePriceListCommand>
{
    public RenamePriceListCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("معرف قائمة الأسعار غير صالح.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("اسم قائمة الأسعار مطلوب.")
            .MaximumLength(150).WithMessage("اسم قائمة الأسعار يجب ألا يتجاوز 150 حرفًا.");
    }
}
