using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Common;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.CreateTestComment;

public sealed class CreateTestCommentCommandHandler : IRequestHandler<CreateTestCommentCommand, Result<int>>
{
    private readonly IApplicationDbContext _db;

    public CreateTestCommentCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result<int>> Handle(CreateTestCommentCommand request, CancellationToken cancellationToken)
    {
        if (!_db.Set<Test>().Any(t => t.Id.Value == request.TestId))
        {
            return Result<int>.Failure(Error.NotFound("التحليل غير موجود"));
        }

        TestComment comment;
        try
        {
            comment = TestComment.Create(TestCommentId.Create(0), TestId.Create(request.TestId), request.CommentText);
        }
        catch (ArgumentException ex)
        {
            return Result<int>.Failure(Error.Validation(DomainFailureTranslator.Translate(ex)));
        }

        _db.Add(comment);
        await _db.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(comment.Id.Value);
    }
}
