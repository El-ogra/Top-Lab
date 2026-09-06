using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Common;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.TestCatalogAndReferenceRanges.Queries.GetReferenceRanges;

public sealed class GetReferenceRangesQueryHandler : IRequestHandler<GetReferenceRangesQuery, Result<IReadOnlyList<ReferenceRangeDto>>>
{
    private readonly IApplicationDbContext _db;

    public GetReferenceRangesQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Result<IReadOnlyList<ReferenceRangeDto>>> Handle(GetReferenceRangesQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Set<ReferenceRange>().AsQueryable();

        if (request.TestId is not null)
        {
            var testIdValue = request.TestId.Value;
            query = query.Where(r => r.TestId.Value == testIdValue);
        }

        var dtos = query
            .OrderBy(r => r.TestId.Value)
            .ThenBy(r => r.AgeMin)
            .ThenBy(r => r.AgeMax)
            .Select(r => new ReferenceRangeDto(
                r.Id.Value,
                r.TestId.Value,
                r.Sex,
                r.AgeUnit,
                r.AgeMin,
                r.AgeMax,
                r.MinValue,
                r.MaxValue,
                r.LowComment,
                r.HighComment))
            .ToList();

        return Task.FromResult(Result<IReadOnlyList<ReferenceRangeDto>>.Success(dtos));
    }
}