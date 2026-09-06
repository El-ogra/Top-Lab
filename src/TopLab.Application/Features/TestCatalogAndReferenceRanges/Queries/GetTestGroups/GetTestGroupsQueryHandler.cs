using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Common;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.TestCatalogAndReferenceRanges.Queries.GetTestGroups;

public sealed class GetTestGroupsQueryHandler : IRequestHandler<GetTestGroupsQuery, Result<IReadOnlyList<TestGroupDto>>>
{
    private readonly IApplicationDbContext _db;

    public GetTestGroupsQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Result<IReadOnlyList<TestGroupDto>>> Handle(GetTestGroupsQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Set<TestGroup>().AsQueryable();

        if (!request.IncludeInactive)
        {
            query = query.Where(g => g.IsActive);
        }

        var dtos = query
            .OrderBy(g => g.Name)
            .Select(g => new TestGroupDto(g.Id.Value, g.Name, g.IsActive))
            .ToList();

        return Task.FromResult(Result<IReadOnlyList<TestGroupDto>>.Success(dtos));
    }
}