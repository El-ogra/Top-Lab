using TopLab.Domain.Utilities;
using Xunit;

namespace TopLab.Domain.Tests.Utilities;

public class StopwatchCalculatorTests
{
    [Fact]
    public void Elapsed_ComputesDifference()
    {
        var start = new DateTime(2026, 3, 15, 10, 0, 0, DateTimeKind.Utc);
        var end = new DateTime(2026, 3, 15, 11, 30, 15, DateTimeKind.Utc);

        var elapsed = StopwatchCalculator.Elapsed(start, end);

        Assert.Equal(new TimeSpan(1, 30, 15), elapsed);
    }

    [Fact]
    public void EqualBounds_ReturnsZero()
    {
        var at = new DateTime(2026, 3, 15, 10, 0, 0, DateTimeKind.Utc);
        Assert.Equal(TimeSpan.Zero, StopwatchCalculator.Elapsed(at, at));
    }

    [Fact]
    public void InvertedBounds_Throw()
    {
        var start = new DateTime(2026, 3, 15, 10, 0, 0, DateTimeKind.Utc);
        var end = start.AddSeconds(-1);

        var ex = Assert.Throws<ArgumentException>(() => StopwatchCalculator.Elapsed(start, end));
        Assert.Equal("endUtc", ex.ParamName);
    }
}
