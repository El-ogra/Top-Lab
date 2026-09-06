using FluentValidation;

namespace TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.DeleteReferenceRange;

public sealed class DeleteReferenceRangeCommandValidator : AbstractValidator<DeleteReferenceRangeCommand>
{
    public DeleteReferenceRangeCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("معرف النطاق المرجعي غير صالح.");
    }
}