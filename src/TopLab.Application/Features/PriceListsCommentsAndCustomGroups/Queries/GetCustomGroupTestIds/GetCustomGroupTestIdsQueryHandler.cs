using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Queries.GetCustomGroupTestIds;

/// <summary>
/// Returns the distinct test ids that appear in at least one custom group.
/// Read-only projection over the existing <c>CustomGroupItem</c> rows; no write, no schema change.
/// </summary>
public sealed class GetCustomGroupTestIdsQueryHandler
    : IRequestHandler<GetCustomGroupTestIdsQuery, Result<IReadOnlyList<int>>>
{
    private readonly IApplicationDbContext _db;

    public GetCustomGroupTestIdsQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Result<IReadOnlyList<int>>> Handle(
        GetCustomGroupTestIdsQuery request, CancellationToken cancellationToken)
    {
        // Distinct: a test in several groups must appear once, because the catalogue shows one
        // row per test.
        var testIds = _db.Set<CustomGroupItem>()
            .Select(i => i.TestId.Value)
            .Distinct()
            .OrderBy(id => id)
            .ToList();

        return Task.FromResult(Result<IReadOnlyList<int>>.Success(testIds));
    }
}
