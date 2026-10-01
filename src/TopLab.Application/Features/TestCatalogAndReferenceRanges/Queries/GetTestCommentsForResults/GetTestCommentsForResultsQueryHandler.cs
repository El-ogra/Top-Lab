using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.TestCatalogAndReferenceRanges.Queries.GetTestCommentsForResults;

/// <summary>W-02 S8 (WP-13): single filtered query, grouped in memory.</summary>
public sealed class GetTestCommentsForResultsQueryHandler
    : IRequestHandler<GetTestCommentsForResultsQuery, Result<IReadOnlyList<TestCommentsForTestDto>>>
{
    private readonly IApplicationDbContext _db;

    public GetTestCommentsForResultsQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Result<IReadOnlyList<TestCommentsForTestDto>>> Handle(
        GetTestCommentsForResultsQuery request, CancellationToken cancellationToken)
    {
        if (request.TestIds.Count == 0)
        {
            return Task.FromResult(
                Result<IReadOnlyList<TestCommentsForTestDto>>.Success(
                    Array.Empty<TestCommentsForTestDto>()));
        }

        var rows = _db.Set<TestComment>()
            .Where(c => request.TestIds.Contains(c.TestId.Value))
            .OrderBy(c => c.Id.Value)
            .ToList();

        IReadOnlyList<TestCommentsForTestDto> result = request.TestIds
            .Distinct()
            .Select(id => new TestCommentsForTestDto(
                id,
                rows.Where(r => r.TestId.Value == id)
                    .Select(r => r.CommentText)
                    .ToList()))
            .ToList();

        return Task.FromResult(Result<IReadOnlyList<TestCommentsForTestDto>>.Success(result));
    }
}
