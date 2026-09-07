using FluentValidation;

namespace TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.RenameCustomGroup;

public sealed class RenameCustomGroupCommandValidator : AbstractValidator<RenameCustomGroupCommand>
{
    public RenameCustomGroupCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("معرف المجموعة غير صالح.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("اسم المجموعة مطلوب.")
            .MaximumLength(150).WithMessage("اسم المجموعة يجب ألا يتجاوز 150 حرفًا.");
    }
}
