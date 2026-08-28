using TopLab.Domain.Common;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;

namespace TopLab.Domain.Accounting;

public sealed class CashMovement : AuditableEntity<CashMovementId>
{
    public MovementType MovementType { get; private set; }

    public decimal Amount { get; private set; }

    public ExternalEntityId? RelatedExternalEntityId { get; private set; }

    public int PerformedByUserId { get; private set; }

    public DateTime OccurredAtUtc { get; private set; }

    public string? Notes { get; private set; }

    private CashMovement()
    {
    }

    private CashMovement(CashMovementId id, MovementType movementType, decimal amount, ExternalEntityId? relatedExternalEntityId, int performedByUserId, DateTime occurredAtUtc, string? notes)
        : base(id)
    {
        MovementType = movementType;
        Amount = amount;
        RelatedExternalEntityId = relatedExternalEntityId;
        PerformedByUserId = performedByUserId;
        OccurredAtUtc = occurredAtUtc;
        Notes = notes;
    }

    public static CashMovement Create(CashMovementId id, MovementType movementType, decimal amount, int performedByUserId, DateTime occurredAtUtc, ExternalEntityId? relatedExternalEntityId = null, string? notes = null)
    {
        return new CashMovement(id, movementType, amount, relatedExternalEntityId, performedByUserId, occurredAtUtc, notes);
    }
}
