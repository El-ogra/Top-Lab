using FluentValidation;

namespace TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.RemoveCustomGroupItem;

public sealed class RemoveCustomGroupItemCommandValidator : AbstractValidator<RemoveCustomGroupItemCommand>
{
    public RemoveCustomGroupItemCommandValidator()
    {
        RuleFor(x => x.CustomGroupId)
            .GreaterThan(0).WithMessage("معرف المجموعة غير صالح.");

        RuleFor(x => x.TestId)
            .GreaterThan(0).WithMessage("معرف التحليل غير صالح.");
    }
}
