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
}