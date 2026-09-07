using FluentValidation;

namespace TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.CreateTestComment;

public sealed class CreateTestCommentCommandValidator : AbstractValidator<CreateTestCommentCommand>
{
    public CreateTestCommentCommandValidator()
    {
        RuleFor(x => x.TestId)
            .GreaterThan(0).WithMessage("معرف التحليل غير صالح.");

        RuleFor(x => x.CommentText)
            .NotEmpty().WithMessage("نص التعليق مطلوب.")
            .MaximumLength(1000).WithMessage("نص التعليق يجب ألا يتجاوز 1000 حرف.");
    }
}
