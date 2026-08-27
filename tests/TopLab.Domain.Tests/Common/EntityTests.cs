using TopLab.Domain.Common;
using Xunit;

namespace TopLab.Domain.Tests.Common;

public sealed class PatientId : StronglyTypedId<int>
{
    public PatientId(int value)
        : base(value)
    {
    }
}

public sealed class TestId : StronglyTypedId<int>
{
    public TestId(int value)
        : base(value)
    {
    }
}

public sealed class SampleEntity : Entity<PatientId>
{
    public SampleEntity(PatientId id)
        : base(id)
    {
    }
}

public class EntityTests
{
    [Fact]
    public void GivenTwoEntitiesWithSameId_WhenCompared_ThenTheyAreEqual()
    {
        var a = new SampleEntity(new PatientId(1));
        var b = new SampleEntity(new PatientId(1));

        Assert.Equal(a, b);
        Assert.True(a == b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void GivenTwoEntitiesWithDifferentIds_WhenCompared_ThenTheyAreNotEqual()
    {
        var a = new SampleEntity(new PatientId(1));
        var b = new SampleEntity(new PatientId(2));

        Assert.NotEqual(a, b);
        Assert.True(a != b);
    }

    [Fact]
    public void GivenEntityOfDifferentType_WhenCompared_ThenTheyAreNotEqual()
    {
        var a = new SampleEntity(new PatientId(1));
        var other = new OtherEntity(new PatientId(1));

        Assert.False(a.Equals(other));
    }

    private sealed class OtherEntity : Entity<PatientId>
    {
        public OtherEntity(PatientId id)
            : base(id)
        {
        }
    }
}
