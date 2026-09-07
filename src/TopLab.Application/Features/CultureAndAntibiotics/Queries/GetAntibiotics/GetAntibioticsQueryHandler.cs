using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.CultureAndAntibiotics.Common;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.CultureAndAntibiotics.Queries.GetAntibiotics;

public sealed class GetAntibioticsQueryHandler
    : IRequestHandler<GetAntibioticsQuery, Result<IReadOnlyList<AntibioticDto>>>
{
    private readonly IApplicationDbContext _db;

    public GetAntibioticsQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Result<IReadOnlyList<AntibioticDto>>> Handle(
        GetAntibioticsQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Set<Antibiotic>().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim();
            query = query.Where(a => a.Name.Contains(term));
        }

        IReadOnlyList<AntibioticDto> items = query
            .OrderBy(a => a.Name)
            .Select(a => new AntibioticDto(
                a.Id.Value,
                a.Name,
                a.IsPregnancyFlagged,
                a.IsChildrenFlagged))
            .ToList();

        return Task.FromResult(Result<IReadOnlyList<AntibioticDto>>.Success(items));
    }
}