using TopLab.Application.Features.SystemAndPrintSettings.Commands.UpdateReportSettings;
using TopLab.Domain.Common.Enums;
using Xunit;

namespace TopLab.Application.Tests.Features.SystemAndPrintSettings;

public class UpdateReportSettingsCommandValidatorTests
{
    private readonly UpdateReportSettingsCommandValidator _validator = new();

    private static UpdateReportSettingsCommand Valid() => new(
        PageMarginLeftCm: 1.0m,
        PageMarginBottomCm: 1.0m,
        ReportTopSpaceCm: 2.0m,
        PaperSize: PaperSize.A4,
        HeaderFooterMode: HeaderFooterMode.None,
        DoctorSignatureEnabled: false,
        HistorySortMode: HistorySortMode.ByLabCode,
        HistoryAutoDisplayEnabled: true);

    [Fact]
    public void Valid_AtMaxTopSpace_Passes()
    {
        var result = _validator.Validate(Valid() with { ReportTopSpaceCm = 8m });
        Assert.True(result.IsValid);
    }

    [Fact]
    public void TopSpaceOverEight_Fails_WithExactArabicMessage()
    {
        var result = _validator.Validate(Valid() with { ReportTopSpaceCm = 8.5m });
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
            e.PropertyName == nameof(UpdateReportSettingsCommand.ReportTopSpaceCm)
            && e.ErrorMessage == "الهامش العلوي للتقرير لا يمكن أن يتجاوز 8 سم");
    }

    [Fact]
    public void NegativeMargin_Fails()
    {
        var result = _validator.Validate(Valid() with { PageMarginLeftCm = -1m });
        Assert.False(result.IsValid);
    }

    [Fact]
    public void HeaderColor_AtMaxLength_Passes()
    {
        var result = _validator.Validate(Valid() with { HeaderColor = "#FF0000" });
        Assert.True(result.IsValid);
    }

    [Fact]
    public void HeaderColor_OverMaxLength_Fails()
    {
        var result = _validator.Validate(Valid() with { HeaderColor = new string('A', 10) });
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateReportSettingsCommand.HeaderColor));
    }

    [Fact]
    public void FooterColor_AtMaxLength_Passes()
    {
        var result = _validator.Validate(Valid() with { FooterColor = "#00FF00" });
        Assert.True(result.IsValid);
    }

    [Fact]
    public void FooterColor_OverMaxLength_Fails()
    {
        var result = _validator.Validate(Valid() with { FooterColor = new string('A', 10) });
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateReportSettingsCommand.FooterColor));
    }

    [Fact]
    public void HeaderImagePath_AtMaxLength_Passes()
    {
        var result = _validator.Validate(Valid() with { HeaderImagePath = new string('A', 300) });
        Assert.True(result.IsValid);
    }

    [Fact]
    public void HeaderImagePath_OverMaxLength_Fails()
    {
        var result = _validator.Validate(Valid() with { HeaderImagePath = new string('A', 301) });
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateReportSettingsCommand.HeaderImagePath));
    }

    [Fact]
    public void FooterImagePath_AtMaxLength_Passes()
    {
        var result = _validator.Validate(Valid() with { FooterImagePath = new string('A', 300) });
        Assert.True(result.IsValid);
    }

    [Fact]
    public void FooterImagePath_OverMaxLength_Fails()
    {
        var result = _validator.Validate(Valid() with { FooterImagePath = new string('A', 301) });
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateReportSettingsCommand.FooterImagePath));
    }

    [Fact]
    public void NormalRangeMarker_AtMaxLength_Passes()
    {
        var result = _validator.Validate(Valid() with { NormalRangeMarker = new string('A', 10) });
        Assert.True(result.IsValid);
    }

    [Fact]
    public void NormalRangeMarker_OverMaxLength_Fails()
    {
        var result = _validator.Validate(Valid() with { NormalRangeMarker = new string('A', 11) });
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateReportSettingsCommand.NormalRangeMarker));
    }

    [Fact]
    public void NullColorAndImageFields_Pass()
    {
        var result = _validator.Validate(Valid() with { HeaderColor = null, FooterColor = null, HeaderImagePath = null, FooterImagePath = null, NormalRangeMarker = null });
        Assert.True(result.IsValid);
    }
}