using TopLab.Domain.Common;

namespace TopLab.Domain.Common.Ids;

/// <summary>Strongly-typed identifier for TestGroupId.</summary>
public sealed class TestGroupId : StronglyTypedId<int>
{
    private TestGroupId(int value) : base(value)
    {
    }

    public static TestGroupId Create(int value) => new(value);
}
