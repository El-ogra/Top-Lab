using TopLab.Domain.Common;
using Xunit;

namespace TopLab.Domain.Tests.Common;

public sealed class Money : ValueObject
{
    private readonly decimal _amount;

    public Money(decimal amount)
    {
        _amount = amount;
    }

    protected override System.Collections.Generic.IEnumerable<object> GetEqualityComponents()
    {
        yield return _amount;
    }
}

public class ValueObjectTests
{
    [Fact]
    public void GivenTwoValueObjectsWithSameComponents_WhenCompared_ThenTheyAreEqual()
    {
        Assert.Equal(new Money(10.5m), new Money(10.5m));
        Assert.True(new Money(10.5m) == new Money(10.5m));
    }

    [Fact]
    public void GivenTwoValueObjectsWithDifferentComponents_WhenCompared_ThenTheyAreNotEqual()
    {
        Assert.NotEqual(new Money(10.5m), new Money(20m));
        Assert.True(new Money(10.5m) != new Money(20m));
    }
}
