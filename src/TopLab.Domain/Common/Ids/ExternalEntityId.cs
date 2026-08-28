using TopLab.Domain.Common;

namespace TopLab.Domain.Common.Ids;

/// <summary>Strongly-typed identifier for ExternalEntityId.</summary>
public sealed class ExternalEntityId : StronglyTypedId<int>
{
    private ExternalEntityId(int value) : base(value)
    {
    }

    public static ExternalEntityId Create(int value) => new(value);
}
