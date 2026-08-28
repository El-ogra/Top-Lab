using TopLab.Domain.Common;

namespace TopLab.Domain.Common.Ids;

/// <summary>Strongly-typed identifier for TestId.</summary>
public sealed class TestId : StronglyTypedId<int>
{
    private TestId(int value) : base(value)
    {
    }

    public static TestId Create(int value) => new(value);
}
