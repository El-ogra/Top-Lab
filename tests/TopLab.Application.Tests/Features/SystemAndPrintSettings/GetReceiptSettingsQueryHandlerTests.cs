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
}