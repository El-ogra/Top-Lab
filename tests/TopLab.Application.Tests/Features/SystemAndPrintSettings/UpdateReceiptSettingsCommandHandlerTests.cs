using TopLab.Application.Features.SystemAndPrintSettings.Commands.UpdateReceiptSettings;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Settings;
using Xunit;

namespace TopLab.Application.Tests.Features.SystemAndPrintSettings;

public class UpdateReceiptSettingsCommandHandlerTests
{
    private static UpdateReceiptSettingsCommand DefaultCommand() => new(
        TopMarginCm: 2.0m,
        Currency: "$",
        PickupTimeDefault: new TimeOnly(10, 30),
        PrintOnce: true,
        TestDetailDisplayMode: TestDetailDisplayMode.ShowWithCode,
        CashierPrinterEnabled: true,
        HeaderFooterMode: HeaderFooterMode.Words);

    [Fact]
    public async Task UpdateReceiptSettings_RoundTrips()
    {
        var db = new FakeApplicationDbContext();
        db.ReceiptSettings.Add(ReceiptSettings.CreateDefault());

        var handler = new UpdateReceiptSettingsCommandHandler(db);
        var result = await handler.Handle(DefaultCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2.0m, db.ReceiptSettings[0].TopMarginCm);
        Assert.Equal("$", db.ReceiptSettings[0].Currency);
        Assert.Equal(new TimeOnly(10, 30), db.ReceiptSettings[0].PickupTimeDefault);
        Assert.True(db.ReceiptSettings[0].PrintOnce);
        Assert.Equal(TestDetailDisplayMode.ShowWithCode, db.ReceiptSettings[0].TestDetailDisplayMode);
        Assert.True(db.ReceiptSettings[0].CashierPrinterEnabled);
        Assert.Equal(HeaderFooterMode.Words, db.ReceiptSettings[0].HeaderFooterMode);
    }

    [Fact]
    public async Task UpdateReceiptSettings_OverlongCurrency_ThrowsAndUnchanged()
    {
        var db = new FakeApplicationDbContext();
        db.ReceiptSettings.Add(ReceiptSettings.CreateDefault());

        var cmd = DefaultCommand() with { Currency = new string('x', 11) };
        var handler = new UpdateReceiptSettingsCommandHandler(db);

        await Assert.ThrowsAsync<ArgumentException>(() => handler.Handle(cmd, CancellationToken.None));
        Assert.Equal("L.E.", db.ReceiptSettings[0].Currency);
    }

    [Fact]
    public async Task UpdateReceiptSettings_MissingRow_ReturnsUnexpected()
    {
        var db = new FakeApplicationDbContext();
        var handler = new UpdateReceiptSettingsCommandHandler(db);
        var result = await handler.Handle(DefaultCommand(), CancellationToken.None);
        Assert.False(result.IsSuccess);
        Assert.Equal(TopLab.Application.Common.Results.ErrorType.Unexpected, result.Error!.Type);
    }

    // GAP-007 tests

    [Fact]
    public async Task UpdateReceiptSettings_WithFontAndLogoFields_PersistsThem()
    {
        var db = new FakeApplicationDbContext();
        db.ReceiptSettings.Add(ReceiptSettings.CreateDefault());

        var cmd = DefaultCommand() with
        {
            HeaderFontFamily = "Arial",
            HeaderFontSizePt = 14.0m,
            HeaderFontColor = "#FF0000",
            FooterFontFamily = "Tahoma",
            FooterFontSizePt = 10.0m,
            FooterFontColor = "#0000FF",
            LogoImagePath = "Logo.png"
        };
        var handler = new UpdateReceiptSettingsCommandHandler(db);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var row = db.ReceiptSettings[0];
        Assert.Equal("Arial", row.HeaderFontFamily);
        Assert.Equal(14.0m, row.HeaderFontSizePt);
        Assert.Equal("#FF0000", row.HeaderFontColor);
        Assert.Equal("Tahoma", row.FooterFontFamily);
        Assert.Equal(10.0m, row.FooterFontSizePt);
        Assert.Equal("#0000FF", row.FooterFontColor);
        Assert.Equal("Logo.png", row.LogoImagePath);
    }

    [Fact]
    public async Task UpdateReceiptSettings_FontAndLogoFieldsDefaultToNull_WhenNotProvided()
    {
        var db = new FakeApplicationDbContext();
        db.ReceiptSettings.Add(ReceiptSettings.CreateDefault());

        var handler = new UpdateReceiptSettingsCommandHandler(db);
        var result = await handler.Handle(DefaultCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var row = db.ReceiptSettings[0];
        Assert.Null(row.HeaderFontFamily);
        Assert.Null(row.HeaderFontSizePt);
        Assert.Null(row.HeaderFontColor);
        Assert.Null(row.FooterFontFamily);
        Assert.Null(row.FooterFontSizePt);
        Assert.Null(row.FooterFontColor);
        Assert.Null(row.LogoImagePath);
    }

    [Fact]
    public async Task UpdateReceiptSettings_InvalidFontSize_Throws()
    {
        var db = new FakeApplicationDbContext();
        db.ReceiptSettings.Add(ReceiptSettings.CreateDefault());

        var cmd = DefaultCommand() with { HeaderFontSizePt = 100m };
        var handler = new UpdateReceiptSettingsCommandHandler(db);
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => handler.Handle(cmd, CancellationToken.None));
    }

    [Fact]
    public async Task UpdateReceiptSettings_ClearFontAndLogoFields_SetsNull()
    {
        var db = new FakeApplicationDbContext();
        var existing = ReceiptSettings.CreateDefault();
        existing.Update(1m, "L.E.", null, false, TestDetailDisplayMode.Show, false, HeaderFooterMode.Images,
            headerFontFamily: "Arial", headerFontSizePt: 14m, headerFontColor: "#FF0000",
            footerFontFamily: "Tahoma", footerFontSizePt: 10m, footerFontColor: "#0000FF",
            logoImagePath: "Logo.png");
        db.ReceiptSettings.Add(existing);

        var cmd = DefaultCommand();
        var handler = new UpdateReceiptSettingsCommandHandler(db);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var row = db.ReceiptSettings[0];
        Assert.Null(row.HeaderFontFamily);
        Assert.Null(row.HeaderFontSizePt);
        Assert.Null(row.HeaderFontColor);
        Assert.Null(row.FooterFontFamily);
        Assert.Null(row.FooterFontSizePt);
        Assert.Null(row.FooterFontColor);
        Assert.Null(row.LogoImagePath);
    }
}
