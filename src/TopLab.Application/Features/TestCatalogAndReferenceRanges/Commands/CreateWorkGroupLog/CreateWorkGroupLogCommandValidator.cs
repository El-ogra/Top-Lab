using FluentValidation;

namespace TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.CreateWorkGroupLog;

public sealed class CreateWorkGroupLogCommandValidator : AbstractValidator<CreateWorkGroupLogCommand>
{
    public CreateWorkGroupLogCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("اسم مجموعة العمل مطلوب.")
            .MaximumLength(150).WithMessage("اسم مجموعة العمل يجب ألا يتجاوز 150 حرفًا.");
    }
}