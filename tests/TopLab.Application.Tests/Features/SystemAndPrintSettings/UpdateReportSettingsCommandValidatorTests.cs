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
}