using TopLab.Domain.Common;

namespace TopLab.Domain.Common.Ids;

/// <summary>Strongly-typed identifier for ProfileId.</summary>
public sealed class ProfileId : StronglyTypedId<int>
{
    private ProfileId(int value) : base(value)
    {
    }

    public static ProfileId Create(int value) => new(value);
}