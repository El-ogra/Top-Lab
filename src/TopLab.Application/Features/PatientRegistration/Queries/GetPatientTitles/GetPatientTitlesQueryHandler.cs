using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientRegistration.Common;
using TopLab.Domain.Patients;

namespace TopLab.Application.Features.PatientRegistration.Queries.GetPatientTitles;

public sealed class GetPatientTitlesQueryHandler
    : IRequestHandler<GetPatientTitlesQuery, Result<IReadOnlyList<PatientTitleDto>>>
{
    private readonly IApplicationDbContext _db;

    public GetPatientTitlesQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Result<IReadOnlyList<PatientTitleDto>>> Handle(
        GetPatientTitlesQuery request, CancellationToken cancellationToken)
    {
        var items = _db.Set<PatientTitle>()
            .OrderBy(t => t.Id.Value)
            .Select(t => new PatientTitleDto(t.Id.Value, t.TitleText, t.IsDefault))
            .ToList();

        return Task.FromResult(Result<IReadOnlyList<PatientTitleDto>>.Success(items));
    }
}