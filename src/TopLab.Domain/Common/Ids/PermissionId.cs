using TopLab.Domain.Common;

namespace TopLab.Domain.Common.Ids;

/// <summary>Strongly-typed identifier for PermissionId.</summary>
public sealed class PermissionId : StronglyTypedId<int>
{
    private PermissionId(int value) : base(value)
    {
    }

    public static PermissionId Create(int value) => new(value);
}
