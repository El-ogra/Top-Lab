using TopLab.Domain.Common.Enums;
using TopLab.Domain.Settings;
using Xunit;

namespace TopLab.Domain.Tests.Settings;

public class ReceiptSettingsTests
{
    [Fact]
    public void Update_Valid_UpdatesAll()
    {
        var r = ReceiptSettings.CreateDefault();
        var pickup = new TimeOnly(14, 30);
        r.Update(2.5m, "$", pickup, true, TestDetailDisplayMode.ShowWithCode, true, HeaderFooterMode.Words);
        Assert.Equal(2.5m, r.TopMarginCm);
        Assert.Equal("$", r.Currency);
        Assert.Equal(pickup, r.PickupTimeDefault);
        Assert.True(r.PrintOnce);
        Assert.Equal(TestDetailDisplayMode.ShowWithCode, r.TestDetailDisplayMode);
        Assert.True(r.CashierPrinterEnabled);
        Assert.Equal(HeaderFooterMode.Words, r.HeaderFooterMode);
    }

    [Fact]
    public void Update_NegativeMargin_Throws()
    {
        var r = ReceiptSettings.CreateDefault();
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            r.Update(-0.1m, "L.E.", null, false, TestDetailDisplayMode.Show, false, HeaderFooterMode.None));
    }

    [Fact]
    public void Update_OverThirtyMargin_Throws()
    {
        var r = ReceiptSettings.CreateDefault();
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            r.Update(30.01m, "L.E.", null, false, TestDetailDisplayMode.Show, false, HeaderFooterMode.None));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_EmptyCurrency_Throws(string? currency)
    {
        var r = ReceiptSettings.CreateDefault();
        Assert.Throws<ArgumentException>(() =>
            r.Update(1.0m, currency!, null, false, TestDetailDisplayMode.Show, false, HeaderFooterMode.None));
    }

    [Fact]
    public void Update_OverlongCurrency_Throws()
    {
        var r = ReceiptSettings.CreateDefault();
        var currency = new string('x', 11);
        Assert.Throws<ArgumentException>(() =>
            r.Update(1.0m, currency, null, false, TestDetailDisplayMode.Show, false, HeaderFooterMode.None));
    }

    [Fact]
    public void Update_ClearPickupTime_SetsNull()
    {
        var r = ReceiptSettings.CreateDefault();
        r.Update(1.0m, "L.E.", new TimeOnly(9, 0), false, TestDetailDisplayMode.Show, false, HeaderFooterMode.None);
        r.Update(1.0m, "L.E.", null, false, TestDetailDisplayMode.Show, false, HeaderFooterMode.None);
        Assert.Null(r.PickupTimeDefault);
    }

    // GAP-007 tests

    [Fact]
    public void CreateDefault_AllFontAndLogoFieldsAreNull()
    {
        var r = ReceiptSettings.CreateDefault();
        Assert.Null(r.HeaderFontFamily);
        Assert.Null(r.HeaderFontSizePt);
        Assert.Null(r.HeaderFontColor);
        Assert.Null(r.FooterFontFamily);
        Assert.Null(r.FooterFontSizePt);
        Assert.Null(r.FooterFontColor);
        Assert.Null(r.LogoImagePath);
    }

    [Fact]
    public void Update_WithFontAndLogoFields_PersistsThem()
    {
        var r = ReceiptSettings.CreateDefault();
        r.Update(1.0m, "L.E.", null, false, TestDetailDisplayMode.Show, false, HeaderFooterMode.Images,
            headerFontFamily: "Arial", headerFontSizePt: 14.0m, headerFontColor: "#FF0000",
            footerFontFamily: "Tahoma", footerFontSizePt: 10.0m, footerFontColor: "#0000FF",
            logoImagePath: "Logo.png");
        Assert.Equal("Arial", r.HeaderFontFamily);
        Assert.Equal(14.0m, r.HeaderFontSizePt);
        Assert.Equal("#FF0000", r.HeaderFontColor);
        Assert.Equal("Tahoma", r.FooterFontFamily);
        Assert.Equal(10.0m, r.FooterFontSizePt);
        Assert.Equal("#0000FF", r.FooterFontColor);
        Assert.Equal("Logo.png", r.LogoImagePath);
    }

    [Fact]
    public void Update_HeaderFontSizeBelowMinimum_Throws()
    {
        var r = ReceiptSettings.CreateDefault();
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            r.Update(1.0m, "L.E.", null, false, TestDetailDisplayMode.Show, false, HeaderFooterMode.None,
                headerFontSizePt: 3.9m));
    }

    [Fact]
    public void Update_HeaderFontSizeAboveMaximum_Throws()
    {
        var r = ReceiptSettings.CreateDefault();
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            r.Update(1.0m, "L.E.", null, false, TestDetailDisplayMode.Show, false, HeaderFooterMode.None,
                headerFontSizePt: 72.1m));
    }

    [Fact]
    public void Update_FooterFontSizeBelowMinimum_Throws()
    {
        var r = ReceiptSettings.CreateDefault();
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            r.Update(1.0m, "L.E.", null, false, TestDetailDisplayMode.Show, false, HeaderFooterMode.None,
                footerFontSizePt: 3.9m));
    }

    [Fact]
    public void Update_FooterFontSizeAboveMaximum_Throws()
    {
        var r = ReceiptSettings.CreateDefault();
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            r.Update(1.0m, "L.E.", null, false, TestDetailDisplayMode.Show, false, HeaderFooterMode.None,
                footerFontSizePt: 72.1m));
    }

    [Fact]
    public void Update_FontAndLogoFieldsDefaultToNull_WhenNotProvided()
    {
        var r = ReceiptSettings.CreateDefault();
        r.Update(1.0m, "L.E.", null, false, TestDetailDisplayMode.Show, false, HeaderFooterMode.Words);
        Assert.Null(r.HeaderFontFamily);
        Assert.Null(r.HeaderFontSizePt);
        Assert.Null(r.HeaderFontColor);
        Assert.Null(r.FooterFontFamily);
        Assert.Null(r.FooterFontSizePt);
        Assert.Null(r.FooterFontColor);
        Assert.Null(r.LogoImagePath);
    }

    [Fact]
    public void Update_ClearFontAndLogoFields_SetsNull()
    {
        var r = ReceiptSettings.CreateDefault();
        r.Update(1.0m, "L.E.", null, false, TestDetailDisplayMode.Show, false, HeaderFooterMode.Images,
            headerFontFamily: "Arial", headerFontSizePt: 14.0m, headerFontColor: "#FF0000",
            footerFontFamily: "Tahoma", footerFontSizePt: 10.0m, footerFontColor: "#0000FF",
            logoImagePath: "Logo.png");
        r.Update(1.0m, "L.E.", null, false, TestDetailDisplayMode.Show, false, HeaderFooterMode.Words);
        Assert.Null(r.HeaderFontFamily);
        Assert.Null(r.HeaderFontSizePt);
        Assert.Null(r.HeaderFontColor);
        Assert.Null(r.FooterFontFamily);
        Assert.Null(r.FooterFontSizePt);
        Assert.Null(r.FooterFontColor);
        Assert.Null(r.LogoImagePath);
    }
}
