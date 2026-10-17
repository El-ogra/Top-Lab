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

    // GAP-007 tests

    [Fact]
    public void Valid_WithFontAndLogoFields_Passes()
    {
        var result = _validator.Validate(Valid() with
        {
            HeaderFontFamily = "Arial",
            HeaderFontSizePt = 14.0m,
            HeaderFontColor = "#FF0000",
            FooterFontFamily = "Tahoma",
            FooterFontSizePt = 10.0m,
            FooterFontColor = "#0000FF",
            LogoImagePath = "Logo.png"
        });
        Assert.True(result.IsValid);
    }

    [Fact]
    public void OverlongHeaderFontFamily_Fails()
    {
        var result = _validator.Validate(Valid() with { HeaderFontFamily = new string('x', 101) });
        Assert.False(result.IsValid);
    }

    [Fact]
    public void OverlongFooterFontFamily_Fails()
    {
        var result = _validator.Validate(Valid() with { FooterFontFamily = new string('x', 101) });
        Assert.False(result.IsValid);
    }

    [Fact]
    public void OverlongHeaderFontColor_Fails()
    {
        var result = _validator.Validate(Valid() with { HeaderFontColor = new string('x', 10) });
        Assert.False(result.IsValid);
    }

    [Fact]
    public void OverlongFooterFontColor_Fails()
    {
        var result = _validator.Validate(Valid() with { FooterFontColor = new string('x', 10) });
        Assert.False(result.IsValid);
    }

    [Fact]
    public void OverlongLogoImagePath_Fails()
    {
        var result = _validator.Validate(Valid() with { LogoImagePath = new string('x', 301) });
        Assert.False(result.IsValid);
    }

    [Fact]
    public void HeaderFontSizeBelowMinimum_Fails()
    {
        var result = _validator.Validate(Valid() with { HeaderFontSizePt = 3.9m });
        Assert.False(result.IsValid);
    }

    [Fact]
    public void HeaderFontSizeAboveMaximum_Fails()
    {
        var result = _validator.Validate(Valid() with { HeaderFontSizePt = 72.1m });
        Assert.False(result.IsValid);
    }

    [Fact]
    public void FooterFontSizeBelowMinimum_Fails()
    {
        var result = _validator.Validate(Valid() with { FooterFontSizePt = 3.9m });
        Assert.False(result.IsValid);
    }

    [Fact]
    public void FooterFontSizeAboveMaximum_Fails()
    {
        var result = _validator.Validate(Valid() with { FooterFontSizePt = 72.1m });
        Assert.False(result.IsValid);
    }

    [Fact]
    public void BoundaryFontSize_Passes()
    {
        var result = _validator.Validate(Valid() with { HeaderFontSizePt = 4.0m, FooterFontSizePt = 72.0m });
        Assert.True(result.IsValid);
    }
}
