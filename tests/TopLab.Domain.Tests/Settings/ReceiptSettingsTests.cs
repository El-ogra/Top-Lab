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
}