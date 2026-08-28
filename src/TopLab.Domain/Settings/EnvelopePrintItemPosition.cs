namespace TopLab.Domain.Settings;

/// <summary>PK is ItemName: Name/Code/ReferralEntity/Date.</summary>
public sealed class EnvelopePrintItemPosition
{
    public string ItemName { get; private set; } = default!;

    public bool IsEnabled { get; private set; }

    public decimal LeftOffsetCm { get; private set; }

    public decimal TopOffsetCm { get; private set; }

    private EnvelopePrintItemPosition()
    {
    }

    public EnvelopePrintItemPosition(string itemName, bool isEnabled, decimal leftOffsetCm, decimal topOffsetCm)
    {
        ItemName = itemName;
        IsEnabled = isEnabled;
        LeftOffsetCm = leftOffsetCm;
        TopOffsetCm = topOffsetCm;
    }
}
