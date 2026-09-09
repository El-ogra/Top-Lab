using TopLab.Domain.Common;

namespace TopLab.Domain.Common.Ids;

/// <summary>Strongly-typed identifier for ProfileResultAmendmentId.</summary>
public sealed class ProfileResultAmendmentId : StronglyTypedId<int>
{
    private ProfileResultAmendmentId(int value) : base(value)
    {
    }

    public static ProfileResultAmendmentId Create(int value) => new(value);
}