using TopLab.Application.Features.SystemAndPrintSettings.Commands.UpdateReportItemTemplates;
using TopLab.Application.Features.SystemAndPrintSettings.Common;
using TopLab.Domain.Settings;
using Xunit;

namespace TopLab.Application.Tests.Features.SystemAndPrintSettings;

/// <summary>
/// GAP-015 (Phase 8): the update validator enforces the documented
/// line-spacing / left-offset bounds and the single-visible-character marker rule.
/// </summary>
public class UpdateReportItemTemplatesCommandLayoutValidatorTests
{
    private readonly UpdateReportItemTemplatesCommandValidator _validator = new();

    private static UpdateReportItemTemplatesCommand Valid(
        decimal? lineSpacing = null,
        decimal? leftOffsetCm = null,
        string? markerSymbol = null)
        => new([new ReportItemTemplateDto(
            ReportItemKeys.TestName, "التحليل", null, true, null, false,
            lineSpacing, leftOffsetCm, markerSymbol)]);

    [Theory]
    [InlineData(0.5)]
    [InlineData(1)]
    [InlineData(10)]
    public void AcceptsLineSpacingInsideBounds(double value)
    {
        Assert.True(_validator.Validate(Valid(lineSpacing: (decimal)value)).IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0.49)]
    [InlineData(10.01)]
    [InlineData(-2)]
    public void RejectsLineSpacingOutsideBounds(double value)
    {
        Assert.False(_validator.Validate(Valid(lineSpacing: (decimal)value)).IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    [InlineData(10)]
    public void AcceptsLeftOffsetInsideBounds(double value)
    {
        Assert.True(_validator.Validate(Valid(leftOffsetCm: (decimal)value)).IsValid);
    }

    [Theory]
    [InlineData(10.01)]
    [InlineData(-0.01)]
    public void RejectsLeftOffsetOutsideBounds(double value)
    {
        Assert.False(_validator.Validate(Valid(leftOffsetCm: (decimal)value)).IsValid);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("•")]
    [InlineData("👍")]
    [InlineData("é")]
    public void AcceptsNullOrSingleVisibleMarker(string? marker)
    {
        Assert.True(_validator.Validate(Valid(markerSymbol: marker)).IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("**")]
    [InlineData("أ ب")]
    public void RejectsBlankOrMultiCharacterMarker(string marker)
    {
        Assert.False(_validator.Validate(Valid(markerSymbol: marker)).IsValid);
    }

    [Fact]
    public void RejectsMarkerLongerThanStorageHeadroom()
    {
        var marker = new string('*', ReportItemTemplate.MaxMarkerSymbolLength + 1);

        Assert.False(_validator.Validate(Valid(markerSymbol: marker)).IsValid);
    }

    [Fact]
    public void RejectsControlCharacterMarker()
    {
        Assert.False(_validator.Validate(Valid(markerSymbol: "\n")).IsValid);
    }

    [Fact]
    public void NullLayout_IsAlwaysValid()
    {
        var result = _validator.Validate(Valid());

        Assert.True(result.IsValid);
    }
}