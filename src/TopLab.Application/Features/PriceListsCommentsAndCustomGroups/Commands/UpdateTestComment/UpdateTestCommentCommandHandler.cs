using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Common;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.UpdateTestComment;

public sealed class UpdateTestCommentCommandHandler : IRequestHandler<UpdateTestCommentCommand, Result>
{
    private readonly IApplicationDbContext _db;

    public UpdateTestCommentCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result> Handle(UpdateTestCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = _db.Set<TestComment>().FirstOrDefault(c => c.Id.Value == request.Id);
        if (comment is null)
        {
            return Result.Failure(Error.NotFound("التعليق غير موجود."));
        }

        try
        {
            comment.Update(request.CommentText);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure(Error.Validation(DomainFailureTranslator.Translate(ex)));
        }

        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
