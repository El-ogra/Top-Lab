using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientBilling.Common;
using TopLab.Domain.Billing;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;
using TopLab.Domain.Users;

namespace TopLab.Application.Features.PatientBilling.Queries.GetPatientAccount;

public sealed class GetPatientAccountQueryHandler
    : IRequestHandler<GetPatientAccountQuery, Result<PatientAccountDto>>
{
    private readonly IApplicationDbContext _db;

    public GetPatientAccountQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Result<PatientAccountDto>> Handle(
        GetPatientAccountQuery request, CancellationToken cancellationToken)
    {
        var patient = _db.Set<Patient>().FirstOrDefault(p => p.Id.Value == request.PatientId);
        if (patient is null || patient.IsDeleted)
        {
            return Task.FromResult(Result<PatientAccountDto>.Failure(Error.NotFound("المريض غير موجود.")));
        }

        var account = PatientBillingReader.ReadAccount(_db, patient);

        return Task.FromResult(Result<PatientAccountDto>.Success(account));
    }
}
