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
}
