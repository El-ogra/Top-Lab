using TopLab.Domain.Common;

namespace TopLab.Domain.Common.Ids;

/// <summary>Strongly-typed identifier for CustomGroupId.</summary>
public sealed class CustomGroupId : StronglyTypedId<int>
{
    private CustomGroupId(int value) : base(value)
    {
    }

    public static CustomGroupId Create(int value) => new(value);
}
