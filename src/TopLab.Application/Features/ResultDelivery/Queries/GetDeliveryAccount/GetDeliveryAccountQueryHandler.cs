using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientBilling.Queries.GetPatientAccount;
using TopLab.Application.Features.ResultDelivery.Common;
using TopLab.Domain.Patients;

namespace TopLab.Application.Features.ResultDelivery.Queries.GetDeliveryAccount;

public sealed class GetDeliveryAccountQueryHandler
    : IRequestHandler<GetDeliveryAccountQuery, Result<DeliveryAccountDto>>
{
    private readonly IApplicationDbContext _db;

    public GetDeliveryAccountQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Result<DeliveryAccountDto>> Handle(
        GetDeliveryAccountQuery request, CancellationToken cancellationToken)
    {
        var patient = _db.Set<Patient>().FirstOrDefault(p => p.Id.Value == request.PatientId);
        if (patient is null || patient.IsDeleted)
        {
            return Task.FromResult(Result<DeliveryAccountDto>.Failure(
                Error.NotFound("المريض غير موجود.")));
        }

        // Delegation, never recomputation: the totals below come from M-03's reader.
        // Only the two remaining amounts are derived here, from the sign of Balance.
        var account = PatientBillingReader.ReadAccount(_db, patient);

        return Task.FromResult(Result<DeliveryAccountDto>.Success(new DeliveryAccountDto(
            account.PatientId,
            account.TotalCharged,
            account.TotalPaid,
            account.Balance,
            account.Balance > 0 ? account.Balance : 0m,
            account.Balance < 0 ? -account.Balance : 0m)));
    }
}
