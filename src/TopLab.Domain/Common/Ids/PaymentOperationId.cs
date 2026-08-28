using TopLab.Domain.Common;

namespace TopLab.Domain.Common.Ids;

/// <summary>Strongly-typed identifier for PaymentOperationId.</summary>
public sealed class PaymentOperationId : StronglyTypedId<int>
{
    private PaymentOperationId(int value) : base(value)
    {
    }

    public static PaymentOperationId Create(int value) => new(value);
}
