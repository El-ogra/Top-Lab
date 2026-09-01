using TopLab.Domain.Common.Enums;
using TopLab.Domain.Settings;
using Xunit;

namespace TopLab.Domain.Tests.Settings;

public class ReportSettingsTests
{
    [Fact]
    public void SetMargins_Valid_Updates()
    {
        var r = ReportSettings.CreateDefault();
        r.SetMargins(2.5m, 3.0m);
        Assert.Equal(2.5m, r.PageMarginLeftCm);
        Assert.Equal(3.0m, r.PageMarginBottomCm);
    }

    [Fact]
    public void SetMargins_NegativeLeft_Throws()
    {
        var r = ReportSettings.CreateDefault();
        Assert.Throws<ArgumentOutOfRangeException>(() => r.SetMargins(-0.1m, 1.0m));
    }

    [Fact]
    public void SetMargins_NegativeBottom_Throws()
    {
        var r = ReportSettings.CreateDefault();
        Assert.Throws<ArgumentOutOfRangeException>(() => r.SetMargins(1.0m, -0.1m));
    }

    [Fact]
    public void SetMargins_OverThirty_Throws()
    {
        var r = ReportSettings.CreateDefault();
        Assert.Throws<ArgumentOutOfRangeException>(() => r.SetMargins(30.01m, 1.0m));
        Assert.Throws<ArgumentOutOfRangeException>(() => r.SetMargins(1.0m, 30.01m));
    }

    [Fact]
    public void SetTopSpace_OverEight_Throws()
    {
        var r = ReportSettings.CreateDefault();
        Assert.Throws<ArgumentException>(() => r.SetTopSpace(8.01m));
    }

    [Fact]
    public void SetTopSpace_Eight_Accepts()
    {
        var r = ReportSettings.CreateDefault();
        r.SetTopSpace(8.0m);
        Assert.Equal(8.0m, r.ReportTopSpaceCm);
    }

    [Fact]
    public void SetPaperSize_Updates()
    {
        var r = ReportSettings.CreateDefault();
        r.SetPaperSize(PaperSize.A5);
        Assert.Equal(PaperSize.A5, r.PaperSize);
    }

    [Fact]
    public void SetHeaderFooterMode_Updates()
    {
        var r = ReportSettings.CreateDefault();
        r.SetHeaderFooterMode(HeaderFooterMode.Words);
        Assert.Equal(HeaderFooterMode.Words, r.HeaderFooterMode);
    }

    [Fact]
    public void SetDoctorSignature_Updates()
    {
        var r = ReportSettings.CreateDefault();
        r.SetDoctorSignature(true);
        Assert.True(r.DoctorSignatureEnabled);
    }

    [Fact]
    public void SetHistoryOptions_Updates()
    {
        var r = ReportSettings.CreateDefault();
        r.SetHistoryOptions(HistorySortMode.ByPatientName, false);
        Assert.Equal(HistorySortMode.ByPatientName, r.HistorySortMode);
        Assert.False(r.HistoryAutoDisplayEnabled);
    }

    [Fact]
    public void NoColorMutator_ColorPropertiesUnchanged()
    {
        var r = ReportSettings.CreateDefault();
        Assert.Null(r.HeaderColor);
        Assert.Null(r.FooterColor);
    }
}