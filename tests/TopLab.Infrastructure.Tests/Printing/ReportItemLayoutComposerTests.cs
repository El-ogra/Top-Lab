using System.Text.Json;
using TopLab.Application.Features.ReportProduction.Common;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Settings;
using TopLab.Infrastructure.Printing;
using Xunit;

namespace TopLab.Infrastructure.Tests.Printing;

/// <summary>
/// GAP-015 (Phase 8): the report composer applies the per-item line spacing,
/// left offset and marker symbol, and a line with all three null renders exactly
/// as before.
/// </summary>
public class ReportItemLayoutComposerTests : IDisposable
{
    // Mirrors what every production writer does in its static constructor; the
    // tests build ReportDocument directly instead of going through a writer.
    static ReportItemLayoutComposerTests()
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
        QuestPDF.Settings.UseSystemFonts = true;
    }

    private readonly string _dir;

    public ReportItemLayoutComposerTests()
    {
        _dir = Path.Combine(Path.GetTempPath(), "toplab-item-layout-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_dir);
    }

    public void Dispose() => Directory.Delete(_dir, recursive: true);

    private static CombinedReportLineDto Line() => new(
        PatientTestId: 101,
        TestId: 2,
        TestName: "سكر الدم",
        TestCode: "GLU",
        ResultKind: 0,
        ResultValue: "5.5",
        ResultFlag: (int)ResultFlag.Normal,
        FrozenRangeText: "4 - 6",
        ProfileLines: Array.Empty<ProfileReportLineDto>(),
        Culture: null);

    private static ReportDocumentContent CombinedContent(params ReportItemTemplate[] templates)
    {
        var dto = new CombinedReportDto(7, "علي", "LAB-1", new[] { Line() });
        return ReportContentBuilder.FromEnvelope(
            new ReportPrintEnvelope(ReportPrintEnvelope.Combined, JsonSerializer.Serialize(dto)),
            ReportSettings.CreateDefault(),
            SystemSettings.CreateDefault(),
            itemTemplates: templates.Length == 0 ? null : templates);
    }

    private static ReportContentLine TestNameLine(ReportDocumentContent content)
        => content.Sections
            .SelectMany(s => s.LayoutLines)
            .First(l => l.Text.Contains("سكر الدم", StringComparison.Ordinal));

    private string WritePdf(ReportDocumentContent content, ReportSettings? settings = null)
    {
        settings ??= ReportSettings.CreateDefault();
        var path = Path.Combine(_dir, $"out-{Guid.NewGuid():N}.pdf");
        new ReportDocument(content, settings).WriteToFile(path);
        Assert.True(new FileInfo(path).Length > 0);
        return path;
    }

    // ---- (c) marker symbol ----

    [Fact]
    public void NullMarkerSymbol_DrawsNoMarker()
    {
        var layout = ReportPageComposer.ResolveItemLayout(new ReportContentLine("التحليل: سكر الدم"));

        Assert.Equal("التحليل: سكر الدم", layout.Text);
    }

    [Fact]
    public void ConfiguredMarkerSymbol_IsDrawnBeforeTheItem()
    {
        var template = ReportItemTemplate.Create(ReportItemKeys.TestName, "التحليل", markerSymbol: "•");
        var line = TestNameLine(CombinedContent(template));

        var layout = ReportPageComposer.ResolveItemLayout(line);

        Assert.Equal("•التحليل: سكر الدم", layout.Text);
    }

    // ---- (b) left offset ----

    [Fact]
    public void NullLeftOffsetCm_KeepsPageMarginLeftAsItemLeftPosition()
    {
        var settings = ReportSettings.CreateDefault();
        settings.SetMargins(2.0m, 1.0m);

        var layout = ReportPageComposer.ResolveItemLayout(new ReportContentLine("التحليل: سكر الدم"), settings);

        Assert.Equal(0f, layout.PaddingLeftPt);
        Assert.Equal(settings.PageMarginLeftCm, layout.LeftPositionCm);
    }

    [Fact]
    public void ConfiguredLeftOffsetCm_IsAdditiveToPageMarginLeft()
    {
        var settings = ReportSettings.CreateDefault();
        settings.SetMargins(2.0m, 1.0m);

        var template = ReportItemTemplate.Create(ReportItemKeys.TestName, "التحليل", leftOffsetCm: 1.5m);
        var layout = ReportPageComposer.ResolveItemLayout(TestNameLine(CombinedContent(template)), settings);

        Assert.Equal(3.5m, layout.LeftPositionCm);
        Assert.Equal((double)PageSizeMapper.CmToPoints(1.5m), layout.PaddingLeftPt, 3);
    }

    [Fact]
    public void LeftOffsetCm_AtBothBounds_Resolves()
    {
        var settings = ReportSettings.CreateDefault();
        settings.SetMargins(1.0m, 1.0m);

        var zero = ReportPageComposer.ResolveItemLayout(
            TestNameLine(CombinedContent(ReportItemTemplate.Create(ReportItemKeys.TestName, "التحليل", leftOffsetCm: 0m))),
            settings);
        var max = ReportPageComposer.ResolveItemLayout(
            new ReportContentLine("سطر", LeftOffsetCm: ReportItemTemplate.MaxLeftOffsetCm),
            settings);

        Assert.Equal(1.0m, zero.LeftPositionCm);
        Assert.Equal(0f, zero.PaddingLeftPt);
        Assert.Equal(11.0m, max.LeftPositionCm);
    }

    // ---- (a) line spacing ----

    [Fact]
    public void NullLineSpacing_LeavesTheComposerDefaultSpacing()
    {
        var layout = ReportPageComposer.ResolveItemLayout(new ReportContentLine("التحليل: سكر الدم"));

        Assert.Null(layout.LineHeight);
    }

    [Fact]
    public void ConfiguredLineSpacing_OverridesTheLineHeight()
    {
        var template = ReportItemTemplate.Create(ReportItemKeys.TestName, "التحليل", lineSpacing: 1.75m);
        var layout = ReportPageComposer.ResolveItemLayout(TestNameLine(CombinedContent(template)));

        Assert.Equal(1.75f, layout.LineHeight);
    }

    [Theory]
    [InlineData(0.5)]
    [InlineData(10.0)]
    public void LineSpacing_AtBothBounds_Resolves(double value)
    {
        var layout = ReportPageComposer.ResolveItemLayout(
            new ReportContentLine("سطر", LineSpacing: (decimal)value));

        Assert.Equal((float)value, layout.LineHeight);
    }

    // ---- all three fields null ----

    [Fact]
    public void NullLayoutForEveryItem_ResolvesToAnIdentityLayout()
    {
        var settings = ReportSettings.CreateDefault();
        var templates = ReportItemKeys.All
            .Select(key => ReportItemTemplate.Create(key, ReportItemTemplateCatalog.GetDefault(key).Label))
            .ToArray();

        var content = CombinedContent(templates);
        var lines = content.Sections.SelectMany(s => s.LayoutLines).ToList();
        Assert.NotEmpty(lines);
        Assert.All(lines, line => Assert.False(line.HasLayout));

        foreach (var line in lines)
        {
            var layout = ReportPageComposer.ResolveItemLayout(line, settings);
            Assert.Equal(line.Text, layout.Text);
            Assert.Null(layout.LineHeight);
            Assert.Equal(0f, layout.PaddingLeftPt);
            Assert.Equal(settings.PageMarginLeftCm, layout.LeftPositionCm);
        }
    }

    // ---- real rendering (QuestPDF accepts every accepted value) ----

    [Theory]
    [InlineData(0.5)]
    [InlineData(10.0)]
    public void LineSpacingAtBounds_RendersWithoutFailing(double value)
    {
        var template = ReportItemTemplate.Create(
            ReportItemKeys.TestName, "التحليل", lineSpacing: (decimal)value);

        WritePdf(CombinedContent(template));
    }

    [Fact]
    public void LeftOffsetAndMarker_RenderAndChangeTheOutput()
    {
        var plain = WritePdf(CombinedContent());
        var decorated = WritePdf(CombinedContent(ReportItemTemplate.Create(
            ReportItemKeys.TestName, "التحليل", lineSpacing: 2m, leftOffsetCm: 1m, markerSymbol: "•")));

        Assert.NotEqual(File.ReadAllBytes(plain), File.ReadAllBytes(decorated));
    }

    [Fact]
    public void SectionHeading_CarriesTheGroupSubtitleLayout()
    {
        var settings = ReportSettings.CreateDefault();
        settings.SetPrintOptions(printGroupSubTitle: true, suppressReprintMessage: false);

        var dto = new CombinedReportDto(7, "علي", "LAB-1", new[] { Line() with { TestGroupName = "كيمياء" } });
        var content = ReportContentBuilder.FromEnvelope(
            new ReportPrintEnvelope(ReportPrintEnvelope.Combined, JsonSerializer.Serialize(dto)),
            settings,
            SystemSettings.CreateDefault(),
            itemTemplates: new[] { ReportItemTemplate.Create(ReportItemKeys.GroupSubtitle, "المجموعة", markerSymbol: "#") });

        var heading = content.Sections.Single(s => !string.IsNullOrWhiteSpace(s.Heading));
        Assert.NotNull(heading.HeadingLine);
        Assert.Equal("#" + heading.Heading, ReportPageComposer.ResolveItemLayout(heading.HeadingLine!, settings).Text);
    }
}