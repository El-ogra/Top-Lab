using TopLab.Domain.Common;

namespace TopLab.Domain.Common.Ids;

/// <summary>Strongly-typed identifier for ProfileResultItemId.</summary>
public sealed class ProfileResultItemId : StronglyTypedId<int>
{
    private ProfileResultItemId(int value) : base(value)
    {
    }

    public static ProfileResultItemId Create(int value) => new(value);
}
