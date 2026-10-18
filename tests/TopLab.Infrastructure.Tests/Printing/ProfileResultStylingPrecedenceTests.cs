using System.Text.Json;
using TopLab.Application.Features.ReportProduction.Common;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Settings;
using TopLab.Infrastructure.Printing;
using Xunit;

namespace TopLab.Infrastructure.Tests.Printing;

/// <summary>
/// GAP-017 precedence rendering tests. The global Phase 9 colours and the per-profile colours are
/// deliberately different values so every assertion proves which one won, never merely that some
/// colour was emitted.
/// </summary>
public class ProfileResultStylingPrecedenceTests
{
    private const string GlobalAbnormalFont = "#111111";
    private const string GlobalAbnormalBackground = "#222222";
    private const string ProfileAbnormalFont = "#AA0000";
    private const string ProfileAbnormalBackground = "#00BB00";
    private const string ProfileNormalFont = "#00FF00";
    private const string ProfileNormalBackground = "#EEEEEE";
    private const string AnalyteName = "SAT";

    private static ReportSettings Settings(bool enable, string? font = GlobalAbnormalFont, string? background = GlobalAbnormalBackground)
    {
        var s = ReportSettings.CreateDefault();
        s.SetEnableAbnormalResultColor(enable);
        s.SetAbnormalResultFontColor(font);
        s.SetAbnormalResultBackgroundColor(background);
        return s;
    }

    private static ProfileReportLineDto AnalyteLine(int? flag) =>
        new(1, 1, AnalyteName, "12.5", "mg", flag, null);

    private static TopLab.Application.Features.ProfileResults.Common.ProfileReportLineDto ProfileReportAnalyteLine(int? flag) =>
        new(1, 1, AnalyteName, "12.5", "mg", flag, null);

    // ---- combined report: the profile's own analyte result lines ----

    private static ReportDocumentContent Combined(
        int flag,
        ProfileResultStyleDto? style,
        ReportSettings settings)
    {
        var dto = new CombinedReportDto(7, "علي", "LAB-1", new List<CombinedReportLineDto>
        {
            new(101, 2, "صورة الدم", "GLU", (int)ResultKind.SpecializedProfile, null, null, null,
                new List<ProfileReportLineDto> { AnalyteLine(flag) }, null,
                null, null, false, null, null, style)
        });

        return ReportContentBuilder.FromEnvelope(
            new ReportPrintEnvelope(ReportPrintEnvelope.Combined, JsonSerializer.Serialize(dto)),
            settings,
            SystemSettings.CreateDefault());
    }

    private static ReportContentLine ProfileLine(ReportDocumentContent content)
        => content.Sections.SelectMany(s => s.LayoutLines)
            .First(l => l.Text.Contains(AnalyteName, StringComparison.Ordinal));

    // ---- standalone profile report: one profile per document ----

    private static ReportDocumentContent ProfileReport(
        int flag,
        ProfileResultStyleDto? style,
        ReportSettings settings)
    {
        var dto = new TopLab.Application.Features.ProfileResults.Common.ProfileReportDto(
            101, 7, "علي", "LAB-1", "صورة الدم", null, true, false, false,
            new List<TopLab.Application.Features.ProfileResults.Common.ProfileReportLineDto> { ProfileReportAnalyteLine(flag) },
            false,
            style);

        return ReportContentBuilder.FromProfileReport(dto, settings, SystemSettings.CreateDefault());
    }

    private static ProfileResultStyleDto Style(
        string? normalFont = null,
        string? normalColor = null,
        string? abnormalFont = null,
        string? abnormalBackgroundColor = null)
        => new(normalFont, normalColor, abnormalFont, abnormalBackgroundColor);

    private static ProfileResultStyleDto FullStyle() =>
        Style(ProfileNormalFont, ProfileNormalBackground, ProfileAbnormalFont, ProfileAbnormalBackground);

    // (a) abnormal result + profile override: the profile styling wins.
    [Theory]
    [InlineData((int)ResultFlag.Low)]
    [InlineData((int)ResultFlag.High)]
    public void Combined_AbnormalWithProfileOverride_ProfileStylingWins(int flag)
    {
        var line = ProfileLine(Combined(flag, FullStyle(), Settings(enable: true)));

        Assert.Equal(ProfileAbnormalFont, line.FontColor);
        Assert.Equal(ProfileAbnormalBackground, line.BackgroundColor);
    }

    // (b) abnormal result without a profile override: the global Phase 9 colour applies.
    [Theory]
    [InlineData((int)ResultFlag.Low)]
    [InlineData((int)ResultFlag.High)]
    public void Combined_AbnormalWithoutProfileOverride_GlobalStylingApplies(int flag)
    {
        var line = ProfileLine(Combined(flag, null, Settings(enable: true)));

        Assert.Equal(GlobalAbnormalFont, line.FontColor);
        Assert.Equal(GlobalAbnormalBackground, line.BackgroundColor);
    }

    // A present-but-all-null style is indistinguishable from no style at all.
    [Fact]
    public void Combined_AbnormalWithAllNullStyle_GlobalStylingApplies()
    {
        var line = ProfileLine(Combined((int)ResultFlag.High, Style(), Settings(enable: true)));

        Assert.Equal(GlobalAbnormalFont, line.FontColor);
        Assert.Equal(GlobalAbnormalBackground, line.BackgroundColor);
    }

    // A profile that overrides only one slot keeps the global value for the other slot.
    [Fact]
    public void Combined_AbnormalPartialOverride_KeepsGlobalForTheUnsetSlot()
    {
        var line = ProfileLine(Combined(
            (int)ResultFlag.High,
            Style(abnormalFont: ProfileAbnormalFont),
            Settings(enable: true)));

        Assert.Equal(ProfileAbnormalFont, line.FontColor);
        Assert.Equal(GlobalAbnormalBackground, line.BackgroundColor);
    }

    // (c) normal result + profile normal styling: the profile's normal styling applies.
    [Fact]
    public void Combined_NormalWithProfileStyling_ProfileNormalStylingApplies()
    {
        var line = ProfileLine(Combined((int)ResultFlag.Normal, FullStyle(), Settings(enable: true)));

        Assert.Equal(ProfileNormalFont, line.FontColor);
        Assert.Equal(ProfileNormalBackground, line.BackgroundColor);
    }

    // Normal styling does not depend on the abnormal master switch: there is no global normal colour.
    [Fact]
    public void Combined_NormalWithProfileStyling_AppliesEvenWhenAbnormalColouringDisabled()
    {
        var line = ProfileLine(Combined((int)ResultFlag.Normal, FullStyle(), Settings(enable: false)));

        Assert.Equal(ProfileNormalFont, line.FontColor);
        Assert.Equal(ProfileNormalBackground, line.BackgroundColor);
    }

    // (d) normal result without profile styling: uncoloured, exactly as Phase 9 left it.
    [Fact]
    public void Combined_NormalWithoutProfileStyling_NoColour()
    {
        var line = ProfileLine(Combined((int)ResultFlag.Normal, null, Settings(enable: true)));

        Assert.Null(line.FontColor);
        Assert.Null(line.BackgroundColor);
    }

    // (e) master switch off: a profile abnormal override must NOT apply.
    [Theory]
    [InlineData((int)ResultFlag.Low)]
    [InlineData((int)ResultFlag.High)]
    public void Combined_MasterSwitchOff_ProfileAbnormalOverrideDoesNotApply(int flag)
    {
        var line = ProfileLine(Combined(flag, FullStyle(), Settings(enable: false)));

        Assert.Null(line.FontColor);
        Assert.Null(line.BackgroundColor);
    }

    // Master switch on: the same override does apply (the counterpart half of rule (e)).
    [Fact]
    public void Combined_MasterSwitchOn_ProfileAbnormalOverrideApplies()
    {
        var line = ProfileLine(Combined((int)ResultFlag.High, FullStyle(), Settings(enable: true)));

        Assert.Equal(ProfileAbnormalFont, line.FontColor);
        Assert.Equal(ProfileAbnormalBackground, line.BackgroundColor);
    }

    // (f) all-null defaults render exactly as before.
    [Fact]
    public void Combined_AllNullDefaults_GlobalAbnormalOnlyAndNoNormalColour()
    {
        var settings = Settings(enable: true);

        var flagged = ProfileLine(Combined((int)ResultFlag.High, null, settings));
        Assert.Equal(GlobalAbnormalFont, flagged.FontColor);
        Assert.Equal(GlobalAbnormalBackground, flagged.BackgroundColor);

        var normal = ProfileLine(Combined((int)ResultFlag.Normal, null, settings));
        Assert.Null(normal.FontColor);
        Assert.Null(normal.BackgroundColor);
    }

    // ---- the standalone profile report applies the same rules ----

    [Fact]
    public void ProfileReport_AbnormalWithProfileOverride_ProfileStylingWins()
    {
        var line = ProfileLine(ProfileReport((int)ResultFlag.High, FullStyle(), Settings(enable: true)));

        Assert.Equal(ProfileAbnormalFont, line.FontColor);
        Assert.Equal(ProfileAbnormalBackground, line.BackgroundColor);
    }

    [Fact]
    public void ProfileReport_AbnormalWithoutProfileOverride_GlobalStylingApplies()
    {
        var line = ProfileLine(ProfileReport((int)ResultFlag.Low, null, Settings(enable: true)));

        Assert.Equal(GlobalAbnormalFont, line.FontColor);
        Assert.Equal(GlobalAbnormalBackground, line.BackgroundColor);
    }

    [Fact]
    public void ProfileReport_NormalWithProfileStyling_ProfileNormalStylingApplies()
    {
        var line = ProfileLine(ProfileReport((int)ResultFlag.Normal, FullStyle(), Settings(enable: true)));

        Assert.Equal(ProfileNormalFont, line.FontColor);
        Assert.Equal(ProfileNormalBackground, line.BackgroundColor);
    }

    [Fact]
    public void ProfileReport_NormalWithoutProfileStyling_NoColour()
    {
        var line = ProfileLine(ProfileReport((int)ResultFlag.Normal, null, Settings(enable: true)));

        Assert.Null(line.FontColor);
        Assert.Null(line.BackgroundColor);
    }

    [Fact]
    public void ProfileReport_MasterSwitchOff_ProfileAbnormalOverrideDoesNotApply()
    {
        var line = ProfileLine(ProfileReport((int)ResultFlag.High, FullStyle(), Settings(enable: false)));

        Assert.Null(line.FontColor);
        Assert.Null(line.BackgroundColor);
    }
}