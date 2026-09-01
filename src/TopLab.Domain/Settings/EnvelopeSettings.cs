using TopLab.Domain.Common;
using TopLab.Domain.Common.Enums;

namespace TopLab.Domain.Settings;

/// <summary>Single row, PK=1.</summary>
public sealed class EnvelopeSettings : Entity<int>
{
    public decimal TopMarginCm { get; private set; }

    public HeaderFooterMode HeaderFooterMode { get; private set; }

    public bool SuppressCaptions { get; private set; }

    private EnvelopeSettings()
    {
    }

    private EnvelopeSettings(int id) : base(id)
    {
    }

    public static EnvelopeSettings CreateDefault()
    {
        return new EnvelopeSettings(1)
        {
            TopMarginCm = 3.0m,
            HeaderFooterMode = HeaderFooterMode.None,
            SuppressCaptions = false
        };
    }

    public void Update(decimal topMarginCm, HeaderFooterMode mode, bool suppressCaptions)
    {
        if (topMarginCm < 0m || topMarginCm > 30m)
        {
            throw new ArgumentOutOfRangeException(nameof(topMarginCm), "Envelope top margin must be between 0 and 30 cm.");
        }

        TopMarginCm = topMarginCm;
        HeaderFooterMode = mode;
        SuppressCaptions = suppressCaptions;
    }
}
