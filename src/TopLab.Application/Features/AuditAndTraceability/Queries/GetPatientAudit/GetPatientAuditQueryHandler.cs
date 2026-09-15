using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.AuditAndTraceability.Common;
using TopLab.Domain.Billing;
using TopLab.Domain.Patients;
using TopLab.Domain.Users;

namespace TopLab.Application.Features.AuditAndTraceability.Queries.GetPatientAudit;

/// <summary>
/// P view (SD-10-5/SD-10-7): read-only patient-record audit. Soft-deleted
/// patients remain auditable (no <c>IsDeleted</c> filter); voided payment
/// operations are included. User names resolve via a <c>Users</c> dictionary
/// with raw-id fallback for deleted users (SD-10-4).
/// </summary>
public sealed class GetPatientAuditQueryHandler
    : IRequestHandler<GetPatientAuditQuery, Result<PatientAuditDto>>
{
    private readonly IApplicationDbContext _db;

    public GetPatientAuditQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Result<PatientAuditDto>> Handle(
        GetPatientAuditQuery request, CancellationToken cancellationToken)
    {
        var patient = _db.Set<Patient>()
            .FirstOrDefault(p => p.Id.Value == request.PatientId);

        if (patient is null)
        {
            return Task.FromResult(Result<PatientAuditDto>.Failure(
                Error.NotFound("المريض غير موجود.")));
        }

        var userNames = _db.Set<User>().ToDictionary(u => u.Id.Value, u => u.UserName);

        string Resolve(int userId) =>
            userNames.TryGetValue(userId, out var name) ? name : userId.ToString();

        var receivers = _db.Set<PaymentOperation>()
            .Where(o => o.PatientId.Value == request.PatientId)
            .ToList()
            .GroupBy(o => o.ReceivedByUserId)
            .Select(g => new PaymentReceiverAuditDto(
                g.Key,
                Resolve(g.Key),
                g.Max(o => o.OperationAtUtc)))
            .OrderBy(r => r.OperationAtUtc)
            .ThenBy(r => r.UserId)
            .ToList();

        var dto = new PatientAuditDto(
            patient.Id.Value,
            patient.FullName,
            patient.CreatedByUserId,
            Resolve(patient.CreatedByUserId),
            patient.CreatedAtUtc,
            patient.ModificationCount,
            patient.LastModifiedByUserId,
            Resolve(patient.LastModifiedByUserId),
            patient.LastModifiedAtUtc,
            receivers);

        return Task.FromResult(Result<PatientAuditDto>.Success(dto));
    }
}
