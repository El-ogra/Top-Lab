using System.Text.Json;
using TopLab.Application.Features.ReportProduction.Common;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Settings;
using TopLab.Infrastructure.Printing;
using Xunit;

namespace TopLab.Infrastructure.Tests.Printing;

public class ReportAbnormalResultColorTests
{
    private static CombinedReportDto Combined(int flag) => new(7, "علي", "LAB-1", new List<CombinedReportLineDto>
    {
        new(101, 2, "سكر الدم", "GLU", 0, "5.5", flag, "4 - 6",
            new List<ProfileReportLineDto>(), null)
    });

    private static ReportDocumentContent Content(int flag, ReportSettings settings)
        => ReportContentBuilder.FromEnvelope(
            new ReportPrintEnvelope(ReportPrintEnvelope.Combined, JsonSerializer.Serialize(Combined(flag))),
            settings, TopLab.Domain.Settings.SystemSettings.CreateDefault());

    private static ReportContentLine ResultLine(ReportDocumentContent content)
        => content.Sections.SelectMany(s => s.LayoutLines)
            .First(l => l.Text.Contains("النتيجة", StringComparison.Ordinal));

    [Theory]
    [InlineData((int)ResultFlag.Low)]
    [InlineData((int)ResultFlag.High)]
    public void Enabled_BothColors_Flagged_LineCarriesBoth(int flag)
    {
        var s = ReportSettings.CreateDefault();
        s.SetEnableAbnormalResultColor(true);
        s.SetAbnormalResultFontColor("#FF0000");
        s.SetAbnormalResultBackgroundColor("#FFFF00");

        var line = ResultLine(Content(flag, s));
        Assert.Equal("#FF0000", line.FontColor);
        Assert.Equal("#FFFF00", line.BackgroundColor);
    }

    [Theory]
    [InlineData((int)ResultFlag.Low)]
    [InlineData((int)ResultFlag.High)]
    public void Enabled_FontOnly_Flagged_LineCarriesFontOnly(int flag)
    {
        var s = ReportSettings.CreateDefault();
        s.SetEnableAbnormalResultColor(true);
        s.SetAbnormalResultFontColor("#FF0000");

        var line = ResultLine(Content(flag, s));
        Assert.Equal("#FF0000", line.FontColor);
        Assert.Null(line.BackgroundColor);
    }

    [Theory]
    [InlineData((int)ResultFlag.Low)]
    [InlineData((int)ResultFlag.High)]
    public void Enabled_BackgroundOnly_Flagged_LineCarriesBackgroundOnly(int flag)
    {
        var s = ReportSettings.CreateDefault();
        s.SetEnableAbnormalResultColor(true);
        s.SetAbnormalResultBackgroundColor("#FFFF00");

        var line = ResultLine(Content(flag, s));
        Assert.Null(line.FontColor);
        Assert.Equal("#FFFF00", line.BackgroundColor);
    }

    [Theory]
    [InlineData((int)ResultFlag.Low)]
    [InlineData((int)ResultFlag.High)]
    public void Enabled_Neither_Flagged_LineCarriesNoColor(int flag)
    {
        var s = ReportSettings.CreateDefault();
        s.SetEnableAbnormalResultColor(true);

        var line = ResultLine(Content(flag, s));
        Assert.Null(line.FontColor);
        Assert.Null(line.BackgroundColor);
    }

    [Theory]
    [InlineData((int)ResultFlag.Low)]
    [InlineData((int)ResultFlag.High)]
    public void Disabled_Flagged_LineCarriesNoColor(int flag)
    {
        var s = ReportSettings.CreateDefault();
        s.SetEnableAbnormalResultColor(false);
        s.SetAbnormalResultFontColor("#FF0000");
        s.SetAbnormalResultBackgroundColor("#FFFF00");

        var line = ResultLine(Content(flag, s));
        Assert.Null(line.FontColor);
        Assert.Null(line.BackgroundColor);
    }

    [Fact]
    public void Enabled_NotFlagged_LineCarriesNoColor()
    {
        var s = ReportSettings.CreateDefault();
        s.SetEnableAbnormalResultColor(true);
        s.SetAbnormalResultFontColor("#FF0000");
        s.SetAbnormalResultBackgroundColor("#FFFF00");

        var line = ResultLine(Content((int)ResultFlag.Normal, s));
        Assert.Null(line.FontColor);
        Assert.Null(line.BackgroundColor);
    }
}
