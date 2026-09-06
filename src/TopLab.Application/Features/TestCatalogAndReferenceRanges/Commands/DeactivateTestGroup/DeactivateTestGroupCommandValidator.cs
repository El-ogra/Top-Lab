using FluentValidation;

namespace TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.DeactivateTestGroup;

public sealed class DeactivateTestGroupCommandValidator : AbstractValidator<DeactivateTestGroupCommand>
{
    public DeactivateTestGroupCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("معرف المجموعة غير صالح.");
    }
}