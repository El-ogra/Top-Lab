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

    /// <summary>
    /// Convenience read-only flag for tests and callers. Getter-only by design;
    /// EF Core ignores getter-only properties by convention, so this adds no column.
    /// </summary>
    public bool IsEffectivelyZero => !IsVoided && Amount == 0m && (DiscountAmount ?? 0m) == 0m;

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
        if (amount < 0)
            throw new ArgumentException("Amount must be >= 0.", nameof(amount));
        if (discountAmount < 0)
            throw new ArgumentException("Discount must be >= 0.", nameof(discountAmount));
        if (discountAmount > amount)
            throw new ArgumentException("Discount cannot exceed the operation amount.", nameof(discountAmount));
        if (isExtraCharge && discountAmount is > 0)
            throw new ArgumentException("An extra charge cannot carry a discount.", nameof(discountAmount));
        if (operationType == OperationType.FullSettlement && amount <= 0)
            throw new ArgumentException("Settlement amount must be > 0.", nameof(amount));
        return new PaymentOperation(id, patientId, amount, discountAmount, isExtraCharge, operationType, receivedByUserId, operationAtUtc);
    }

    public void Void()
    {
        IsVoided = true;
    }
}
