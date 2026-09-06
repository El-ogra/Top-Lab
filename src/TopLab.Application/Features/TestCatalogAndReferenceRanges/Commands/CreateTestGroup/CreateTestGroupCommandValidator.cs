using FluentValidation;

namespace TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.CreateTestGroup;

public sealed class CreateTestGroupCommandValidator : AbstractValidator<CreateTestGroupCommand>
{
    public CreateTestGroupCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("اسم المجموعة مطلوب.")
            .MaximumLength(150).WithMessage("اسم المجموعة يجب ألا يتجاوز 150 حرفًا.");
    }
}