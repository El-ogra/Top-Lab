using TopLab.Application.Features.SystemAndPrintSettings.Commands.UpdateReceiptSettings;
using TopLab.Domain.Common.Enums;
using Xunit;

namespace TopLab.Application.Tests.Features.SystemAndPrintSettings;

public class UpdateReceiptSettingsCommandValidatorTests
{
    private readonly UpdateReceiptSettingsCommandValidator _validator = new();

    private static UpdateReceiptSettingsCommand Valid() => new(
        TopMarginCm: 1.0m,
        Currency: "L.E.",
        PickupTimeDefault: null,
        PrintOnce: false,
        TestDetailDisplayMode: TestDetailDisplayMode.Show,
        CashierPrinterEnabled: false,
        HeaderFooterMode: HeaderFooterMode.None);

    [Fact]
    public void Valid_Passes()
    {
        var result = _validator.Validate(Valid());
        Assert.True(result.IsValid);
    }

    [Fact]
    public void EmptyCurrency_Fails()
    {
        var result = _validator.Validate(Valid() with { Currency = "" });
        Assert.False(result.IsValid);
    }

    [Fact]
    public void OverlongCurrency_Fails()
    {
        var result = _validator.Validate(Valid() with { Currency = new string('x', 11) });
        Assert.False(result.IsValid);
    }
}