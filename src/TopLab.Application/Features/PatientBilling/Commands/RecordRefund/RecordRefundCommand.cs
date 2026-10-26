using MediatR;
using TopLab.Application.Common.Results;

namespace TopLab.Application.Features.PatientBilling.Commands.RecordRefund;

/// <summary>
/// Records a refund against the patient's account (canonical reference 1.3.2.5).
/// <para>
/// Ungated exactly like <see cref="Commands.RecordPayment.RecordPaymentCommand"/>: the
/// reference establishes the action, not a new permission, and the round forbids adding
/// permission codes. The refund is a new operation of <c>OperationType.Refund</c> carrying
/// <c>RefundAmount</c> with <c>Amount = 0</c>, as the Domain enforces — a stored amount is
/// never overwritten (OD-12 option b).
/// </para>
/// </summary>
public sealed record RecordRefundCommand(
    int PatientId,
    decimal RefundAmount) : IRequest<Result<int>>;
