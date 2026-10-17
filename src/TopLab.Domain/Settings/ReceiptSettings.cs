using TopLab.Domain.Common;
using TopLab.Domain.Common.Enums;

namespace TopLab.Domain.Settings;

/// <summary>Single row, PK=1.</summary>
public sealed class ReceiptSettings : Entity<int>
{
    public decimal TopMarginCm { get; private set; }

    public string Currency { get; private set; } = "L.E.";

    public TimeOnly? PickupTimeDefault { get; private set; }

    public bool PrintOnce { get; private set; }

    public TestDetailDisplayMode TestDetailDisplayMode { get; private set; }

    public bool CashierPrinterEnabled { get; private set; }

    public HeaderFooterMode HeaderFooterMode { get; private set; }

    /// <summary>Font family for the receipt header text. null = use the lab text font.</summary>
    public string? HeaderFontFamily { get; private set; }

    /// <summary>Font size (pt) for the receipt header text. null = use the lab text size.</summary>
    public decimal? HeaderFontSizePt { get; private set; }

    /// <summary>Font colour (hex) for the receipt header text. null = black.</summary>
    public string? HeaderFontColor { get; private set; }

    /// <summary>Font family for the receipt footer text. null = use the lab text font.</summary>
    public string? FooterFontFamily { get; private set; }

    /// <summary>Font size (pt) for the receipt footer text. null = use the lab text size.</summary>
    public decimal? FooterFontSizePt { get; private set; }

    /// <summary>Font colour (hex) for the receipt footer text. null = black.</summary>
    public string? FooterFontColor { get; private set; }

    /// <summary>Relative path (filename) of the lab logo in the app-managed asset store. null = no logo.</summary>
    public string? LogoImagePath { get; private set; }

    private ReceiptSettings()
    {
    }

    private ReceiptSettings(int id) : base(id)
    {
    }

    public static ReceiptSettings CreateDefault()
    {
        return new ReceiptSettings(1)
        {
            TopMarginCm = 1.0m,
            Currency = "L.E.",
            PickupTimeDefault = null,
            PrintOnce = false,
            TestDetailDisplayMode = TestDetailDisplayMode.Show,
            CashierPrinterEnabled = false,
            HeaderFooterMode = HeaderFooterMode.None,
            HeaderFontFamily = null,
            HeaderFontSizePt = null,
            HeaderFontColor = null,
            FooterFontFamily = null,
            FooterFontSizePt = null,
            FooterFontColor = null,
            LogoImagePath = null
        };
    }

    public void Update(
        decimal topMarginCm,
        string currency,
        TimeOnly? pickupTimeDefault,
        bool printOnce,
        TestDetailDisplayMode mode,
        bool cashierPrinterEnabled,
        HeaderFooterMode headerFooterMode,
        string? headerFontFamily = null,
        decimal? headerFontSizePt = null,
        string? headerFontColor = null,
        string? footerFontFamily = null,
        decimal? footerFontSizePt = null,
        string? footerFontColor = null,
        string? logoImagePath = null)
    {
        if (topMarginCm < 0m || topMarginCm > 30m)
        {
            throw new ArgumentOutOfRangeException(nameof(topMarginCm), "Receipt top margin must be between 0 and 30 cm.");
        }

        if (string.IsNullOrWhiteSpace(currency) || currency.Length > 10)
        {
            throw new ArgumentException("Currency must be non-empty and at most 10 characters.", nameof(currency));
        }

        if (headerFontSizePt.HasValue && (headerFontSizePt.Value < 4m || headerFontSizePt.Value > 72m))
        {
            throw new ArgumentOutOfRangeException(nameof(headerFontSizePt), "Receipt header font size must be between 4 and 72 pt.");
        }

        if (footerFontSizePt.HasValue && (footerFontSizePt.Value < 4m || footerFontSizePt.Value > 72m))
        {
            throw new ArgumentOutOfRangeException(nameof(footerFontSizePt), "Receipt footer font size must be between 4 and 72 pt.");
        }

        TopMarginCm = topMarginCm;
        Currency = currency.Trim();
        PickupTimeDefault = pickupTimeDefault;
        PrintOnce = printOnce;
        TestDetailDisplayMode = mode;
        CashierPrinterEnabled = cashierPrinterEnabled;
        HeaderFooterMode = headerFooterMode;
        HeaderFontFamily = headerFontFamily;
        HeaderFontSizePt = headerFontSizePt;
        HeaderFontColor = headerFontColor;
        FooterFontFamily = footerFontFamily;
        FooterFontSizePt = footerFontSizePt;
        FooterFontColor = footerFontColor;
        LogoImagePath = logoImagePath;
    }
}
