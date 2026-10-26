using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientBilling.Common;
using TopLab.Domain.Billing;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;

namespace TopLab.Application.Features.PatientBilling.Commands.RecordRefund;

/// <summary>
/// Records a refund (canonical reference 1.3.2.5, «استرداد»). The operation is a new
/// <c>OperationType.Refund</c> row carrying <c>RefundAmount</c> with <c>Amount = 0</c>
/// (the Domain's rule), so nothing stored is overwritten (OD-12 option b).
/// <para>
/// The refund may not exceed the patient's net total actually paid — that is
/// <c>PatientAccountCalculator.TotalPaid</c> after earlier refunds and voided operations —
/// so repeated refunds can never drive the account below zero.
/// </para>
/// </summary>
public sealed class RecordRefundCommandHandler : IRequestHandler<RecordRefundCommand, Result<int>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;

    public RecordRefundCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IDateTimeProvider clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Result<int>> Handle(RecordRefundCommand request, CancellationToken cancellationToken)
    {
        var patient = _db.Set<Patient>().FirstOrDefault(p => p.Id.Value == request.PatientId);
        if (patient is null || patient.IsDeleted)
        {
            return Result<int>.Failure(Error.NotFound("المريض غير موجود."));
        }

        var operations = _db.Set<PaymentOperation>()
            .Where(o => o.PatientId.Value == request.PatientId)
            .ToList();

        // R1 Round 5 Part B: the ceiling is the net CASH the patient actually paid — discounts
        // are excluded (a discount was never money), as are extra charges and earlier refunds.
        var maxRefundable = AccountEntryCalculator.MaxRefundable(operations);
        if (request.RefundAmount > maxRefundable)
        {
            return Result<int>.Failure(Error.Validation(
                $"مبلغ الاسترداد يتجاوز إجمالي المدفوع النقدي الصافي ({maxRefundable})."));
        }

        PaymentOperation refund;
        try
        {
            refund = PaymentOperation.Create(
                PaymentOperationId.Create(0),
                PatientId.Create(request.PatientId),
                amount: 0m,
                receivedByUserId: _currentUser.UserId,
                operationAtUtc: _clock.UtcNow,
                discountAmount: null,
                isExtraCharge: false,
                operationType: OperationType.Refund,
                refundAmount: request.RefundAmount);
        }
        catch (ArgumentException ex)
        {
            return Result<int>.Failure(Error.Validation(DomainFailureTranslator.Translate(ex)));
        }

        _db.Add(refund);
        await _db.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(refund.Id.Value);
    }
}
