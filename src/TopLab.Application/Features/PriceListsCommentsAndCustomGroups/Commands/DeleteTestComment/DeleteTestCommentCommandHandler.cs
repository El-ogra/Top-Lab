using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.DeleteTestComment;

public sealed class DeleteTestCommentCommandHandler : IRequestHandler<DeleteTestCommentCommand, Result>
{
    private readonly IApplicationDbContext _db;

    public DeleteTestCommentCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result> Handle(DeleteTestCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = _db.Set<TestComment>().FirstOrDefault(c => c.Id.Value == request.Id);
        if (comment is null)
        {
            return Result.Failure(Error.NotFound("التعليق غير موجود."));
        }

        _db.Remove(comment);
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
