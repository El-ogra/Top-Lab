using TopLab.Domain.Common;
using TopLab.Domain.Common.Enums;

namespace TopLab.Domain.Settings;

/// <summary>Single row, PK=1.</summary>
public sealed class ReportSettings : Entity<int>
{
    public decimal PageMarginLeftCm { get; private set; }

    public decimal PageMarginBottomCm { get; private set; }

    public decimal ReportTopSpaceCm { get; private set; }

    public PaperSize PaperSize { get; private set; }

    public HeaderFooterMode HeaderFooterMode { get; private set; }

    public bool DoctorSignatureEnabled { get; private set; }

    public string? HeaderColor { get; private set; }

    public string? FooterColor { get; private set; }

    public bool EnableAbnormalResultColor { get; private set; }

    public string? AbnormalResultFontColor { get; private set; }

    public string? AbnormalResultBackgroundColor { get; private set; }

    public HistorySortMode HistorySortMode { get; private set; }

    public bool HistoryAutoDisplayEnabled { get; private set; }

    /// <summary>W-02 S7 (WP-13): print each group's sub-title (group name) in the report.</summary>
    public bool PrintGroupSubTitle { get; private set; }

    /// <summary>W-02 S7 (WP-13): reprint without asking for confirmation.</summary>
    public bool SuppressReprintMessage { get; private set; }

    /// <summary>Relative path (filename) of the header image in the app-managed asset store. Null = no image.</summary>
    public string? HeaderImagePath { get; private set; }

    /// <summary>Relative path (filename) of the footer image in the app-managed asset store. Null = no image.</summary>
    public string? FooterImagePath { get; private set; }

    /// <summary>Marker character(s) for the normal range. Null/empty = use default.</summary>
    public string? NormalRangeMarker { get; private set; }

    private ReportSettings()
    {
    }

    private ReportSettings(int id) : base(id)
    {
    }

    public static ReportSettings CreateDefault()
    {
        return new ReportSettings(1)
        {
            PageMarginLeftCm = 1.0m,
            PageMarginBottomCm = 1.0m,
            ReportTopSpaceCm = 2.0m,
            PaperSize = PaperSize.A4,
            HeaderFooterMode = HeaderFooterMode.None,
            DoctorSignatureEnabled = false,
            HeaderColor = null,
            FooterColor = null,
            EnableAbnormalResultColor = false,
            AbnormalResultFontColor = null,
            AbnormalResultBackgroundColor = null,
            HistorySortMode = HistorySortMode.ByLabCode,
            HistoryAutoDisplayEnabled = true,
            PrintGroupSubTitle = false,
            SuppressReprintMessage = false,
            HeaderImagePath = null,
            FooterImagePath = null,
            NormalRangeMarker = null
        };
    }

    public void SetTopSpace(decimal value)
    {
        if (value > 8m)
        {
            throw new ArgumentException("ReportTopSpaceCm must be <= 8.", nameof(value));
        }

        ReportTopSpaceCm = value;
    }

    public void SetMargins(decimal leftCm, decimal bottomCm)
    {
        if (leftCm < 0m || leftCm > 30m)
        {
            throw new ArgumentOutOfRangeException(nameof(leftCm), "Page margin left must be between 0 and 30 cm.");
        }

        if (bottomCm < 0m || bottomCm > 30m)
        {
            throw new ArgumentOutOfRangeException(nameof(bottomCm), "Page margin bottom must be between 0 and 30 cm.");
        }

        PageMarginLeftCm = leftCm;
        PageMarginBottomCm = bottomCm;
    }

    public void SetPaperSize(PaperSize size)
    {
        PaperSize = size;
    }

    public void SetHeaderFooterMode(HeaderFooterMode mode)
    {
        HeaderFooterMode = mode;
    }

    public void SetDoctorSignature(bool enabled)
    {
        DoctorSignatureEnabled = enabled;
    }

    public void SetHistoryOptions(HistorySortMode sortMode, bool autoDisplay)
    {
        HistorySortMode = sortMode;
        HistoryAutoDisplayEnabled = autoDisplay;
    }

    /// <summary>W-02 S7 (WP-13).</summary>
    public void SetPrintOptions(bool printGroupSubTitle, bool suppressReprintMessage)
    {
        PrintGroupSubTitle = printGroupSubTitle;
        SuppressReprintMessage = suppressReprintMessage;
    }

    public void SetHeaderColor(string? color)
    {
        HeaderColor = color;
    }

    public void SetFooterColor(string? color)
    {
        FooterColor = color;
    }

    public void SetEnableAbnormalResultColor(bool enabled)
    {
        EnableAbnormalResultColor = enabled;
    }

    public void SetAbnormalResultFontColor(string? color)
    {
        AbnormalResultFontColor = color;
    }

    public void SetAbnormalResultBackgroundColor(string? color)
    {
        AbnormalResultBackgroundColor = color;
    }

    public void SetAbnormalResultColor(bool enabled, string? fontColor = null, string? backgroundColor = null)
    {
        EnableAbnormalResultColor = enabled;
        AbnormalResultFontColor = fontColor;
        AbnormalResultBackgroundColor = backgroundColor;
    }

    public void SetAbnormalResultColors(bool enabled, string? fontColor = null, string? backgroundColor = null)
    {
        EnableAbnormalResultColor = enabled;
        AbnormalResultFontColor = fontColor;
        AbnormalResultBackgroundColor = backgroundColor;
    }

    public void SetHeaderImagePath(string? path)
    {
        HeaderImagePath = path;
    }

    public void SetFooterImagePath(string? path)
    {
        FooterImagePath = path;
    }

    public void SetNormalRangeMarker(string? marker)
    {
        NormalRangeMarker = marker;
    }
}
