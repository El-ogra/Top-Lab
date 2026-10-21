using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientRegistration.Common;
using TopLab.Domain.Patients;

namespace TopLab.Application.Features.PatientRegistration.Queries.GetMedicalConditionTypes;

public sealed class GetMedicalConditionTypesQueryHandler
    : IRequestHandler<GetMedicalConditionTypesQuery, Result<IReadOnlyList<MedicalConditionTypeDto>>>
{
    private readonly IApplicationDbContext _db;

    public GetMedicalConditionTypesQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Result<IReadOnlyList<MedicalConditionTypeDto>>> Handle(
        GetMedicalConditionTypesQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Set<MedicalConditionType>().AsQueryable();

        if (request.Category.HasValue)
        {
            query = query.Where(mct => mct.Category == request.Category.Value);
        }

        // STAGE-01: the catalogue is ordered by the baseline's question order
        // (medication toggles, pregnancy toggle, condition toggles); rows added
        // through the maintenance screen follow the seeded ones. The catalogue
        // is a small master table, so it is ordered in memory.
        var items = query
            .ToList()
            .OrderBy(mct => MedicalConditionCatalogue.BaselineRank(mct.Id.Value))
            .ThenBy(mct => mct.Category)
            .ThenBy(mct => mct.Name)
            .Select(mct => new MedicalConditionTypeDto(mct.Id.Value, mct.Name, mct.Category))
            .ToList();

        return Task.FromResult(Result<IReadOnlyList<MedicalConditionTypeDto>>.Success(items));
    }
}