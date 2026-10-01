using TopLab.Domain.Common.Ids;

namespace TopLab.Domain.Tests;

/// <summary>Composite PK: TestId + AntibioticId.</summary>
public sealed class CultureAntibioticAttachment
{
    public TestId TestId { get; private set; } = default!;

    public AntibioticId AntibioticId { get; private set; } = default!;

    /// <summary>W-02 S9 (WP-14): zone threshold in millimetres suggesting the category.</summary>
    public decimal? SensitivityThresholdMm { get; private set; }

    private CultureAntibioticAttachment()
    {
    }

    public CultureAntibioticAttachment(TestId testId, AntibioticId antibioticId)
    {
        TestId = testId;
        AntibioticId = antibioticId;
    }

    /// <summary>W-02 S9 (WP-14): threshold in millimetres, 0–100.</summary>
    public void SetThreshold(decimal? millimetres)
    {
        if (millimetres is < 0 or > 100)
        {
            throw new ArgumentException("Threshold must be between 0 and 100 mm.", nameof(millimetres));
        }

        SensitivityThresholdMm = millimetres;
    }
}
