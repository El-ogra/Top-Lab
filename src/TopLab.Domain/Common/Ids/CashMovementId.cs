using TopLab.Domain.Common;

namespace TopLab.Domain.Common.Ids;

/// <summary>Strongly-typed identifier for CashMovementId.</summary>
public sealed class CashMovementId : StronglyTypedId<int>
{
    private CashMovementId(int value) : base(value)
    {
    }

    public static CashMovementId Create(int value) => new(value);
}
