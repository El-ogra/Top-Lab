using TopLab.Domain.Common;

namespace TopLab.Domain.Common.Ids;

/// <summary>Strongly-typed identifier for TestCommentId.</summary>
public sealed class TestCommentId : StronglyTypedId<int>
{
    private TestCommentId(int value) : base(value)
    {
    }

    public static TestCommentId Create(int value) => new(value);
}
