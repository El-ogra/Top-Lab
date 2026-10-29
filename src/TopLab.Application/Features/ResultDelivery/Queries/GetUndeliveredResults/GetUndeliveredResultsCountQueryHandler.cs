using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ResultDelivery.Common;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;

namespace TopLab.Application.Features.ResultDelivery.Queries.GetUndeliveredResults;

public sealed class GetUndeliveredResultsCountQueryHandler
    : IRequestHandler<GetUndeliveredResultsCountQuery, Result<int>>
{
    private readonly IApplicationDbContext _db;

    public GetUndeliveredResultsCountQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Result<int>> Handle(GetUndeliveredResultsCountQuery request, CancellationToken cancellationToken)
    {
        var (from, to) = ResultDeliveryPeriod.Resolve(request.From, request.To);

        var patientIds = _db.Set<Patient>()
            .Where(p => !p.IsDeleted
                && DateOnly.FromDateTime(p.RegistrationDateUtc) >= from
                && DateOnly.FromDateTime(p.RegistrationDateUtc) <= to)
            .Select(p => p.Id.Value)
            .ToHashSet();

        var count = _db.Set<PatientTest>()
            .Count(pt => patientIds.Contains(pt.PatientId.Value) && !pt.IsDelivered);

        return Task.FromResult(Result<int>.Success(count));
    }
}