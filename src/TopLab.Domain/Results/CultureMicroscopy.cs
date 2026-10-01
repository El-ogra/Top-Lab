using TopLab.Domain.Common.Ids;

namespace TopLab.Domain.Results;

/// <summary>
/// W-02 S9 (WP-14): microscopy block of a culture result, one-to-one with
/// <see cref="CultureResult"/> on <c>PatientTestId</c>. Short free-text fields
/// (no enum) admit any laboratory convention ("+", "++", counts per field).
/// </summary>
public sealed class CultureMicroscopy
{
    public const int MaxFieldLength = 20;

    public PatientTestId PatientTestId { get; private set; } = default!;

    public string? PusCells { get; private set; }

    public string? RedBloodCells { get; private set; }

    public string? EpithelialCells { get; private set; }

    public string? Crystals { get; private set; }

    public string? Fungi { get; private set; }

    public string? OthersOne { get; private set; }

    public string? OthersTwo { get; private set; }

    public string? OthersThree { get; private set; }

    public bool IsDirect { get; private set; }

    private CultureMicroscopy()
    {
    }

    public CultureMicroscopy(
        PatientTestId patientTestId,
        string? pusCells = null,
        string? redBloodCells = null,
        string? epithelialCells = null,
        string? crystals = null,
        string? fungi = null,
        string? othersOne = null,
        string? othersTwo = null,
        string? othersThree = null,
        bool isDirect = false)
    {
        PatientTestId = patientTestId;
        Update(pusCells, redBloodCells, epithelialCells, crystals, fungi, othersOne, othersTwo, othersThree, isDirect);
    }

    public void Update(
        string? pusCells,
        string? redBloodCells,
        string? epithelialCells,
        string? crystals,
        string? fungi,
        string? othersOne,
        string? othersTwo,
        string? othersThree,
        bool isDirect)
    {
        PusCells = Normalize(pusCells);
        RedBloodCells = Normalize(redBloodCells);
        EpithelialCells = Normalize(epithelialCells);
        Crystals = Normalize(crystals);
        Fungi = Normalize(fungi);
        OthersOne = Normalize(othersOne);
        OthersTwo = Normalize(othersTwo);
        OthersThree = Normalize(othersThree);
        IsDirect = isDirect;
    }

    private static string? Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        if (trimmed.Length > MaxFieldLength)
        {
            throw new ArgumentException($"Microscopy field must be at most {MaxFieldLength} characters.", nameof(value));
        }

        return trimmed;
    }
}
