using FluentValidation;

namespace TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.UpdateTestGroup;

public sealed class UpdateTestGroupCommandValidator : AbstractValidator<UpdateTestGroupCommand>
{
    public UpdateTestGroupCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("اسم المجموعة مطلوب.")
            .MaximumLength(150).WithMessage("اسم المجموعة يجب ألا يتجاوز 150 حرفًا.");
    }
}