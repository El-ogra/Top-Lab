using TopLab.Application.Features.SystemAndPrintSettings.Queries.GetReceiptSettings;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Settings;
using Xunit;

namespace TopLab.Application.Tests.Features.SystemAndPrintSettings;

public class GetReceiptSettingsQueryHandlerTests
{
    [Fact]
    public async Task GetReceiptSettings_ReturnsSeededDefaults()
    {
        var db = new FakeApplicationDbContext();
        db.ReceiptSettings.Add(ReceiptSettings.CreateDefault());

        var handler = new GetReceiptSettingsQueryHandler(db);
        var result = await handler.Handle(new GetReceiptSettingsQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1.0m, result.Value!.TopMarginCm);
        Assert.Equal("L.E.", result.Value.Currency);
        Assert.Null(result.Value.PickupTimeDefault);
        Assert.False(result.Value.PrintOnce);
        Assert.Equal(TopLab.Domain.Common.Enums.TestDetailDisplayMode.Show, result.Value.TestDetailDisplayMode);
        Assert.False(result.Value.CashierPrinterEnabled);
        Assert.Equal(TopLab.Domain.Common.Enums.HeaderFooterMode.None, result.Value.HeaderFooterMode);
    }

    // GAP-007 tests

    [Fact]
    public async Task GetReceiptSettings_ReturnsFontAndLogoFields()
    {
        var db = new FakeApplicationDbContext();
        var existing = ReceiptSettings.CreateDefault();
        existing.Update(1m, "L.E.", null, false, TopLab.Domain.Common.Enums.TestDetailDisplayMode.Show, false, TopLab.Domain.Common.Enums.HeaderFooterMode.Images,
            headerFontFamily: "Arial", headerFontSizePt: 14m, headerFontColor: "#FF0000",
            footerFontFamily: "Tahoma", footerFontSizePt: 10m, footerFontColor: "#0000FF",
            logoImagePath: "Logo.png");
        db.ReceiptSettings.Add(existing);

        var handler = new GetReceiptSettingsQueryHandler(db);
        var result = await handler.Handle(new GetReceiptSettingsQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Arial", result.Value!.HeaderFontFamily);
        Assert.Equal(14m, result.Value.HeaderFontSizePt);
        Assert.Equal("#FF0000", result.Value.HeaderFontColor);
        Assert.Equal("Tahoma", result.Value.FooterFontFamily);
        Assert.Equal(10m, result.Value.FooterFontSizePt);
        Assert.Equal("#0000FF", result.Value.FooterFontColor);
        Assert.Equal("Logo.png", result.Value.LogoImagePath);
    }

    [Fact]
    public async Task GetReceiptSettings_FontAndLogoFieldsDefaultToNull_WhenNotSet()
    {
        var db = new FakeApplicationDbContext();
        db.ReceiptSettings.Add(ReceiptSettings.CreateDefault());

        var handler = new GetReceiptSettingsQueryHandler(db);
        var result = await handler.Handle(new GetReceiptSettingsQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value!.HeaderFontFamily);
        Assert.Null(result.Value.HeaderFontSizePt);
        Assert.Null(result.Value.HeaderFontColor);
        Assert.Null(result.Value.FooterFontFamily);
        Assert.Null(result.Value.FooterFontSizePt);
        Assert.Null(result.Value.FooterFontColor);
        Assert.Null(result.Value.LogoImagePath);
    }
}
