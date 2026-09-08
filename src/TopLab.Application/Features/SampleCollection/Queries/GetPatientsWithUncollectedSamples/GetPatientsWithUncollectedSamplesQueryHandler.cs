using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.SampleCollection.Common;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;

namespace TopLab.Application.Features.SampleCollection.Queries.GetPatientsWithUncollectedSamples;

public sealed class GetPatientsWithUncollectedSamplesQueryHandler
    : IRequestHandler<GetPatientsWithUncollectedSamplesQuery, Result<IReadOnlyList<PatientWithUndrawnTestsDto>>>
{
    private readonly IApplicationDbContext _db;

    public GetPatientsWithUncollectedSamplesQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Result<IReadOnlyList<PatientWithUndrawnTestsDto>>> Handle(
        GetPatientsWithUncollectedSamplesQuery request, CancellationToken cancellationToken)
    {
        var day = request.Day ?? DateOnly.FromDateTime(DateTime.UtcNow);

        var undrawnCounts = _db.Set<PatientTest>()
            .Where(pt => !pt.IsSampleDrawn && !pt.IsTakenOutsideLab)
            .GroupBy(pt => pt.PatientId.Value)
            .ToDictionary(g => g.Key, g => g.Count());

        var rows = _db.Set<Patient>()
            .Where(p => !p.IsDeleted
                && DateOnly.FromDateTime(p.RegistrationDateUtc) == day
                && undrawnCounts.ContainsKey(p.Id.Value))
            .OrderBy(p => p.RegistrationDateUtc)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        IReadOnlyList<PatientWithUndrawnTestsDto> items = rows
            .Select(p => new PatientWithUndrawnTestsDto(
                p.Id.Value,
                p.LabId == null ? null : p.LabId.Value,
                p.FullName,
                p.RegistrationDateUtc,
                undrawnCounts[p.Id.Value]))
            .ToList();

        return Task.FromResult(Result<IReadOnlyList<PatientWithUndrawnTestsDto>>.Success(items));
    }
}
