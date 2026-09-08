using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientBilling.Common;
using TopLab.Application.Features.PatientBilling.Queries.GetPatientAccount;
using TopLab.Domain.Patients;

namespace TopLab.Application.Features.PatientBilling.Queries.ListPatientPayments;

public sealed class ListPatientPaymentsQueryHandler
    : IRequestHandler<ListPatientPaymentsQuery, Result<IReadOnlyList<PaymentOperationDto>>>
{
    private readonly IApplicationDbContext _db;

    public ListPatientPaymentsQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Result<IReadOnlyList<PaymentOperationDto>>> Handle(
        ListPatientPaymentsQuery request, CancellationToken cancellationToken)
    {
        var patient = _db.Set<Patient>().FirstOrDefault(p => p.Id.Value == request.PatientId);
        if (patient is null || patient.IsDeleted)
        {
            return Task.FromResult(Result<IReadOnlyList<PaymentOperationDto>>.Failure(Error.NotFound("المريض غير موجود.")));
        }

        IReadOnlyList<PaymentOperationDto> page = PatientBillingReader
            .ReadOperations(_db, patient.Id.Value)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        return Task.FromResult(Result<IReadOnlyList<PaymentOperationDto>>.Success(page));
    }
}
