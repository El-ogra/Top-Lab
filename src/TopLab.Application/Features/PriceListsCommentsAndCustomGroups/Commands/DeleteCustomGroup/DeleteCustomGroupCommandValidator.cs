using FluentValidation;

namespace TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.DeleteCustomGroup;

public sealed class DeleteCustomGroupCommandValidator : AbstractValidator<DeleteCustomGroupCommand>
{
    public DeleteCustomGroupCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("معرف المجموعة غير صالح.");
    }
}
