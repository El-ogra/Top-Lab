using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Domain.Billing;

namespace TopLab.Application.Features.PatientBilling.Commands.VoidPaymentOperation;

/// <summary>
/// Voids a payment operation (gated on CASH_DISBURSE_DEPOSIT). The row survives
/// and is excluded from all totals. Settled: no edit command ships — a
/// mis-recorded amount is corrected by voiding the row and re-recording
/// (ADR-0017 + Coding Standards §7.4); void-and-reissue is the only
/// correction flow.
/// </summary>
public sealed class VoidPaymentOperationCommandHandler : IRequestHandler<VoidPaymentOperationCommand, Result>
{
    private readonly IApplicationDbContext _db;

    public VoidPaymentOperationCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result> Handle(VoidPaymentOperationCommand request, CancellationToken cancellationToken)
    {
        var operation = _db.Set<PaymentOperation>().FirstOrDefault(o => o.Id.Value == request.PaymentOperationId);
        if (operation is null)
        {
            return Result.Failure(Error.NotFound("العملية غير موجودة."));
        }

        if (operation.IsVoided)
        {
            return Result.Failure(Error.Conflict("العملية ملغاة بالفعل."));
        }

        operation.Void();
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
