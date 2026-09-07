using FluentValidation;

namespace TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.UpdateTestComment;

public sealed class UpdateTestCommentCommandValidator : AbstractValidator<UpdateTestCommentCommand>
{
    public UpdateTestCommentCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("معرف التعليق غير صالح.");

        RuleFor(x => x.CommentText)
            .NotEmpty().WithMessage("نص التعليق مطلوب.")
            .MaximumLength(1000).WithMessage("نص التعليق يجب ألا يتجاوز 1000 حرف.");
    }
}
