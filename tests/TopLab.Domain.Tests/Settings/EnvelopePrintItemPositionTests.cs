using TopLab.Domain.Settings;
using Xunit;

namespace TopLab.Domain.Tests.Settings;

public class EnvelopePrintItemPositionTests
{
    [Fact]
    public void Update_Valid_UpdatesAll()
    {
        var p = new EnvelopePrintItemPosition("Name", true, 1.0m, 1.0m);
        p.Update(false, 2.5m, 3.0m);
        Assert.False(p.IsEnabled);
        Assert.Equal(2.5m, p.LeftOffsetCm);
        Assert.Equal(3.0m, p.TopOffsetCm);
    }

    [Fact]
    public void Update_NegativeLeftOffset_Throws()
    {
        var p = new EnvelopePrintItemPosition("Name", true, 1.0m, 1.0m);
        Assert.Throws<ArgumentOutOfRangeException>(() => p.Update(true, -0.1m, 1.0m));
    }

    [Fact]
    public void Update_NegativeTopOffset_Throws()
    {
        var p = new EnvelopePrintItemPosition("Name", true, 1.0m, 1.0m);
        Assert.Throws<ArgumentOutOfRangeException>(() => p.Update(true, 1.0m, -0.1m));
    }

    [Fact]
    public void Update_OverThirtyOffset_Throws()
    {
        var p = new EnvelopePrintItemPosition("Name", true, 1.0m, 1.0m);
        Assert.Throws<ArgumentOutOfRangeException>(() => p.Update(true, 30.01m, 1.0m));
        Assert.Throws<ArgumentOutOfRangeException>(() => p.Update(true, 1.0m, 30.01m));
    }

    [Fact]
    public void ItemName_IsImmutable()
    {
        var p = new EnvelopePrintItemPosition("Code", true, 1.0m, 2.0m);
        Assert.NotNull(p.ItemName);
        Assert.Equal("Code", p.ItemName);
    }
}