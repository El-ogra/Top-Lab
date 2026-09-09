using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.AnalyteProfiles.Common;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.AnalyteProfiles.Queries.GetAnalyteDefinitions;

public sealed class GetAnalyteDefinitionsQueryHandler
    : IRequestHandler<GetAnalyteDefinitionsQuery, Result<IReadOnlyList<AnalyteDefinitionDto>>>
{
    private readonly IApplicationDbContext _db;

    public GetAnalyteDefinitionsQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Result<IReadOnlyList<AnalyteDefinitionDto>>> Handle(
        GetAnalyteDefinitionsQuery request,
        CancellationToken cancellationToken)
    {
        var analytes = _db.Set<Analyte>().OrderBy(a => a.Name).ToList();
        var ranges = _db.Set<AnalyteReferenceRange>().ToDictionary(r => r.AnalyteId, r => r);
        var bands = _db.Set<AnalyteReferenceRangeBand>()
            .GroupBy(b => b.AnalyteReferenceRangeId)
            .ToDictionary(g => g.Key, g => g.OrderBy(b => b.AgeMin).ThenBy(b => b.Id.Value).ToList());

        var dtos = analytes.Select(a =>
        {
            IReadOnlyList<AnalyteBandDto> bandDtos = Array.Empty<AnalyteBandDto>();
            if (ranges.TryGetValue(a.Id, out var range) && bands.TryGetValue(range.Id, out var rangeBands))
            {
                bandDtos = rangeBands
                    .Select(b => new AnalyteBandDto(
                        b.Sex, b.AgeUnit, b.AgeMin, b.AgeMax, b.MinValue, b.MaxValue, b.LowComment, b.HighComment))
                    .ToList();
            }

            return new AnalyteDefinitionDto(a.Id.Value, a.Name, a.ReportName, a.IsActive, bandDtos);
        }).ToList();

        return Task.FromResult(Result<IReadOnlyList<AnalyteDefinitionDto>>.Success(dtos));
    }
}