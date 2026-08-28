using TopLab.Domain.Common;

namespace TopLab.Domain.Common.Ids;

/// <summary>Strongly-typed identifier for UserId.</summary>
public sealed class UserId : StronglyTypedId<int>
{
    private UserId(int value) : base(value)
    {
    }

    public static UserId Create(int value) => new(value);
}
