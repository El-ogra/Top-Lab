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
            HeaderFooterMode = HeaderFooterMode.None
        };
    }

    public void Update(
        decimal topMarginCm,
        string currency,
        TimeOnly? pickupTimeDefault,
        bool printOnce,
        TestDetailDisplayMode mode,
        bool cashierPrinterEnabled,
        HeaderFooterMode headerFooterMode)
    {
        if (topMarginCm < 0m || topMarginCm > 30m)
        {
            throw new ArgumentOutOfRangeException(nameof(topMarginCm), "Receipt top margin must be between 0 and 30 cm.");
        }

        if (string.IsNullOrWhiteSpace(currency) || currency.Length > 10)
        {
            throw new ArgumentException("Currency must be non-empty and at most 10 characters.", nameof(currency));
        }

        TopMarginCm = topMarginCm;
        Currency = currency.Trim();
        PickupTimeDefault = pickupTimeDefault;
        PrintOnce = printOnce;
        TestDetailDisplayMode = mode;
        CashierPrinterEnabled = cashierPrinterEnabled;
        HeaderFooterMode = headerFooterMode;
    }
}
