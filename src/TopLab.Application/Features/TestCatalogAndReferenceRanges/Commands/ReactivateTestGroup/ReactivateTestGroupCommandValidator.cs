using FluentValidation;

namespace TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.ReactivateTestGroup;

public sealed class ReactivateTestGroupCommandValidator : AbstractValidator<ReactivateTestGroupCommand>
{
    public ReactivateTestGroupCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("معرف المجموعة غير صالح.");
    }
}