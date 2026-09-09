using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.AnalyteProfiles.Common;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.AnalyteProfiles.Queries.GetProfileDefinitions;

public sealed class GetProfileDefinitionsQueryHandler
    : IRequestHandler<GetProfileDefinitionsQuery, Result<IReadOnlyList<ProfileDefinitionDto>>>
{
    private readonly IApplicationDbContext _db;

    public GetProfileDefinitionsQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Result<IReadOnlyList<ProfileDefinitionDto>>> Handle(
        GetProfileDefinitionsQuery request,
        CancellationToken cancellationToken)
    {
        var profiles = _db.Set<Profile>().OrderBy(p => p.Name).ToList();
        var tests = _db.Set<Test>()
            .Where(t => profiles.Any(p => p.TestId.Equals(t.Id)))
            .ToDictionary(t => t.Id, t => t);
        var links = _db.Set<ProfileAnalyte>()
            .GroupBy(l => l.ProfileId)
            .ToDictionary(g => g.Key, g => g.OrderBy(l => l.Id.Value).Select(l => l.AnalyteId.Value).ToList());

        var dtos = profiles.Select(p =>
        {
            tests.TryGetValue(p.TestId, out var test);
            return new ProfileDefinitionDto(
                p.Id.Value,
                p.Name,
                p.TestId.Value,
                test?.Name ?? string.Empty,
                p.FixedPrice,
                p.IsActive,
                links.TryGetValue(p.Id, out var ids) ? ids : new List<int>());
        }).ToList();

        return Task.FromResult(Result<IReadOnlyList<ProfileDefinitionDto>>.Success(dtos));
    }
}