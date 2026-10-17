using System.Text.Json;
using TopLab.Application.Features.ReportProduction.Common;
using TopLab.Application.Features.ResultsEntry.Common;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Settings;
using TopLab.Application.Common.Interfaces;
using TopLab.Infrastructure.Printing;
using Xunit;

namespace TopLab.Infrastructure.Tests.Printing;

/// <summary>
/// Phase 1 tests: normal-range marker substitution, header/footer image rendering,
/// and the image asset store.
/// </summary>
public class ReportContentBuilderPhase1Tests
{
    private static ReportSettings SettingsWithMarker(string marker)
    {
        var s = ReportSettings.CreateDefault();
        s.SetNormalRangeMarker(marker);
        return s;
    }

    private static CombinedReportDto SimpleCombined()
        => new(7, "علي", "LAB-1", new List<CombinedReportLineDto>
        {
            new(101, 2, "سكر الدم", "GLU", 0, "5.5", 0, "4 - 6",
                new List<ProfileReportLineDto>(), null)
        });

    private static ReportPrintEnvelope CombinedEnvelope(CombinedReportDto dto)
        => new(ReportPrintEnvelope.Combined, JsonSerializer.Serialize(dto));

    [Fact]
    public void Marker_WhenNull_UsesDefaultArabicLabel()
    {
        var settings = ReportSettings.CreateDefault();
        var content = ReportContentBuilder.FromEnvelope(
            CombinedEnvelope(SimpleCombined()), settings, SystemSettings.CreateDefault());
        var all = string.Join("\n", content.BuildDisplayLines());
        Assert.Contains("المدى: 4 - 6", all, StringComparison.Ordinal);
    }

    [Fact]
    public void Marker_WhenSet_SubstitutesInOutput()
    {
        var settings = SettingsWithMarker("N");
        var content = ReportContentBuilder.FromEnvelope(
            CombinedEnvelope(SimpleCombined()), settings, SystemSettings.CreateDefault());
        var all = string.Join("\n", content.BuildDisplayLines());
        Assert.Contains("N 4 - 6", all, StringComparison.Ordinal);
        Assert.DoesNotContain("المدى:", all, StringComparison.Ordinal);
    }

    [Fact]
    public void Marker_WhenEmpty_UsesDefaultArabicLabel()
    {
        var settings = SettingsWithMarker("");
        var content = ReportContentBuilder.FromEnvelope(
            CombinedEnvelope(SimpleCombined()), settings, SystemSettings.CreateDefault());
        var all = string.Join("\n", content.BuildDisplayLines());
        Assert.Contains("المدى: 4 - 6", all, StringComparison.Ordinal);
    }

    [Fact]
    public void ImagePaths_AreCarriedIntoContent()
    {
        var settings = ReportSettings.CreateDefault();
        settings.SetHeaderImagePath("Header.png");
        settings.SetFooterImagePath("Footer.png");
        var content = ReportContentBuilder.FromEnvelope(
            CombinedEnvelope(SimpleCombined()), settings, SystemSettings.CreateDefault(),
            resolvedHeaderImagePath: @"C:\assets\Header.png",
            resolvedFooterImagePath: @"C:\assets\Footer.png");
        Assert.Equal(@"C:\assets\Header.png", content.HeaderImagePath);
        Assert.Equal(@"C:\assets\Footer.png", content.FooterImagePath);
    }

    [Fact]
    public void ImagePaths_WhenNull_AreNullInContent()
    {
        var settings = ReportSettings.CreateDefault();
        var content = ReportContentBuilder.FromEnvelope(
            CombinedEnvelope(SimpleCombined()), settings, SystemSettings.CreateDefault());
        Assert.Null(content.HeaderImagePath);
        Assert.Null(content.FooterImagePath);
    }

    [Fact]
    public void Marker_IsCarriedIntoContent()
    {
        var settings = SettingsWithMarker("*");
        var content = ReportContentBuilder.FromEnvelope(
            CombinedEnvelope(SimpleCombined()), settings, SystemSettings.CreateDefault());
        Assert.Equal("*", content.NormalRangeMarker);
    }
}
