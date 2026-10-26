using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Domain.Billing;
using TopLab.Domain.Patients;

namespace TopLab.Application.Features.PatientBilling.Commands.ClearPatientAccount;

/// <summary>
/// Clears the account (canonical reference 1.3.2.7, «تصفير الحساب») by voiding every
/// non-voided operation of the patient.
/// <para>
/// Settled pins honoured: no hard delete (rows and audit trail survive), only this patient's
/// operations are touched, already-voided rows are left alone, and the command is idempotent —
/// an account with nothing left to void returns success with a count of zero. The returned
/// value is the number of operations actually voided, which the screen reports.
/// </para>
/// </summary>
public sealed class ClearPatientAccountCommandHandler : IRequestHandler<ClearPatientAccountCommand, Result<int>>
{
    private readonly IApplicationDbContext _db;

    public ClearPatientAccountCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result<int>> Handle(ClearPatientAccountCommand request, CancellationToken cancellationToken)
    {
        var patient = _db.Set<Patient>().FirstOrDefault(p => p.Id.Value == request.PatientId);
        if (patient is null || patient.IsDeleted)
        {
            return Result<int>.Failure(Error.NotFound("المريض غير موجود."));
        }

        var operations = _db.Set<PaymentOperation>()
            .Where(o => o.PatientId.Value == request.PatientId && !o.IsVoided)
            .ToList();

        foreach (var operation in operations)
        {
            operation.Void();
        }

        if (operations.Count > 0)
        {
            await _db.SaveChangesAsync(cancellationToken);
        }

        return Result<int>.Success(operations.Count);
    }
}
