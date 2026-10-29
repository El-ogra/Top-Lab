using TopLab.Domain.Common;

namespace TopLab.Domain.Common.Ids;

public sealed class ReceiptEmissionId : StronglyTypedId<int>
{
    private ReceiptEmissionId(int value) : base(value) { }
    public static ReceiptEmissionId Create(int value) => new(value);
}