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

    public HistorySortMode HistorySortMode { get; private set; }

    public bool HistoryAutoDisplayEnabled { get; private set; }

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
            HistorySortMode = HistorySortMode.ByLabCode,
            HistoryAutoDisplayEnabled = true
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
}
