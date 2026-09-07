using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Common;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Queries.GetTestComments;

public sealed class GetTestCommentsQueryHandler : IRequestHandler<GetTestCommentsQuery, Result<IReadOnlyList<TestCommentDto>>>
{
    private readonly IApplicationDbContext _db;

    public GetTestCommentsQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Result<IReadOnlyList<TestCommentDto>>> Handle(GetTestCommentsQuery request, CancellationToken cancellationToken)
    {
        if (request.TestId is not null)
        {
            if (!_db.Set<Test>().Any(t => t.Id.Value == request.TestId.Value))
            {
                return Task.FromResult(Result<IReadOnlyList<TestCommentDto>>.Failure(Error.NotFound("التحليل غير موجود")));
            }
        }

        var query = _db.Set<TestComment>().AsQueryable();

        if (request.TestId is not null)
        {
            var testIdValue = request.TestId.Value;
            query = query.Where(c => c.TestId.Value == testIdValue);
        }

        var comments = query.OrderBy(c => c.Id.Value).ToList();
        var testNamesById = _db.Set<Test>().ToDictionary(t => t.Id, t => t.Name);

        var dtos = comments
            .Select(c =>
            {
                testNamesById.TryGetValue(c.TestId, out var testName);
                return new TestCommentDto(c.Id.Value, c.TestId.Value, testName ?? string.Empty, c.CommentText);
            })
            .ToList();

        return Task.FromResult(Result<IReadOnlyList<TestCommentDto>>.Success(dtos));
    }
}
