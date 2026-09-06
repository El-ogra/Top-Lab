using FluentValidation;

namespace TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.SaveWorkGroupLogItems;

public sealed class SaveWorkGroupLogItemsCommandValidator : AbstractValidator<SaveWorkGroupLogItemsCommand>
{
    public SaveWorkGroupLogItemsCommandValidator()
    {
        RuleFor(x => x.TestIds)
            .NotEmpty().WithMessage("يجب تحديد تحليل واحد على الأقل.");

        RuleFor(x => x.TestIds)
            .Must(ids => ids.Distinct().Count() == ids.Count)
            .WithMessage("لا يمكن تكرار نفس التحليل في مجموعة العمل.");
    }
}