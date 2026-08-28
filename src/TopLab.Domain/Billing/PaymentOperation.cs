using TopLab.Domain.Common;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;

namespace TopLab.Domain.Billing;

public sealed class PaymentOperation : AuditableEntity<PaymentOperationId>
{
    public PatientId PatientId { get; private set; } = default!;

    public decimal Amount { get; private set; }

    public decimal? DiscountAmount { get; private set; }

    public bool IsExtraCharge { get; private set; }

    public OperationType OperationType { get; private set; }

    public int ReceivedByUserId { get; private set; }

    public DateTime OperationAtUtc { get; private set; }

    public bool IsVoided { get; private set; }

    private PaymentOperation()
    {
    }

    private PaymentOperation(
        PaymentOperationId id,
        PatientId patientId,
        decimal amount,
        decimal? discountAmount,
        bool isExtraCharge,
        OperationType operationType,
        int receivedByUserId,
        DateTime operationAtUtc)
        : base(id)
    {
        PatientId = patientId;
        Amount = amount;
        DiscountAmount = discountAmount;
        IsExtraCharge = isExtraCharge;
        OperationType = operationType;
        ReceivedByUserId = receivedByUserId;
        OperationAtUtc = operationAtUtc;
    }

    public static PaymentOperation Create(
        PaymentOperationId id,
        PatientId patientId,
        decimal amount,
        int receivedByUserId,
        DateTime operationAtUtc,
        decimal? discountAmount = null,
        bool isExtraCharge = false,
        OperationType operationType = OperationType.Payment)
    {
        return new PaymentOperation(id, patientId, amount, discountAmount, isExtraCharge, operationType, receivedByUserId, operationAtUtc);
    }

    public void Void()
    {
        IsVoided = true;
    }
}
