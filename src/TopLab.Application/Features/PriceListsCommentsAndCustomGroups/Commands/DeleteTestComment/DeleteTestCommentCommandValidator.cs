using FluentValidation;

namespace TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.DeleteTestComment;

public sealed class DeleteTestCommentCommandValidator : AbstractValidator<DeleteTestCommentCommand>
{
    public DeleteTestCommentCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("معرف التعليق غير صالح.");
    }
}
