using FluentValidation;

namespace TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.RenameWorkGroupLog;

public sealed class RenameWorkGroupLogCommandValidator : AbstractValidator<RenameWorkGroupLogCommand>
{
    public RenameWorkGroupLogCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("اسم مجموعة العمل مطلوب.")
            .MaximumLength(150).WithMessage("اسم مجموعة العمل يجب ألا يتجاوز 150 حرفًا.");
    }
}