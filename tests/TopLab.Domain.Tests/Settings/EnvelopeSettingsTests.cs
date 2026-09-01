using TopLab.Domain.Common.Enums;
using TopLab.Domain.Settings;
using Xunit;

namespace TopLab.Domain.Tests.Settings;

public class EnvelopeSettingsTests
{
    [Fact]
    public void Update_Valid_UpdatesAll()
    {
        var e = EnvelopeSettings.CreateDefault();
        e.Update(2.0m, HeaderFooterMode.Words, true);
        Assert.Equal(2.0m, e.TopMarginCm);
        Assert.Equal(HeaderFooterMode.Words, e.HeaderFooterMode);
        Assert.True(e.SuppressCaptions);
    }

    [Fact]
    public void Update_NegativeMargin_Throws()
    {
        var e = EnvelopeSettings.CreateDefault();
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            e.Update(-0.1m, HeaderFooterMode.None, false));
    }

    [Fact]
    public void Update_OverThirtyMargin_Throws()
    {
        var e = EnvelopeSettings.CreateDefault();
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            e.Update(30.01m, HeaderFooterMode.None, false));
    }
}