namespace TopLab.Infrastructure.Printing;

/// <summary>
/// Stable null-safe contract for the culture (microbiology) block on a report.
/// WP-01 builds this contract assuming missing fields; WP-14 fills the richer
/// culture data later without changing the writer (circle-break with WP-14).
/// </summary>
public sealed record ReportCultureSection(
    string? Sample,
    string? OrganismA,
    string? OrganismB,
    string? OrganismC,
    string? CultureCondition,
    string? ColonyCount,
    string? MicroscopyPusCells = null,
    string? MicroscopyRedBloodCells = null,
    string? MicroscopyEpithelialCells = null,
    string? MicroscopyCrystals = null,
    string? MicroscopyFungi = null,
    string? MicroscopyOthersOne = null,
    string? MicroscopyOthersTwo = null,
    string? MicroscopyOthersThree = null,
    bool MicroscopyIsDirect = false,
    IReadOnlyList<ReportCultureSensitivityRow>? SensitivityRows = null)
{
    public bool HasAnyContent =>
        !string.IsNullOrWhiteSpace(Sample)
        || !string.IsNullOrWhiteSpace(OrganismA)
        || !string.IsNullOrWhiteSpace(OrganismB)
        || !string.IsNullOrWhiteSpace(OrganismC)
        || !string.IsNullOrWhiteSpace(CultureCondition)
        || !string.IsNullOrWhiteSpace(ColonyCount)
        || HasMicroscopyContent
        || (SensitivityRows?.Count ?? 0) > 0;

    public bool HasMicroscopyContent =>
        !string.IsNullOrWhiteSpace(MicroscopyPusCells)
        || !string.IsNullOrWhiteSpace(MicroscopyRedBloodCells)
        || !string.IsNullOrWhiteSpace(MicroscopyEpithelialCells)
        || !string.IsNullOrWhiteSpace(MicroscopyCrystals)
        || !string.IsNullOrWhiteSpace(MicroscopyFungi)
        || !string.IsNullOrWhiteSpace(MicroscopyOthersOne)
        || !string.IsNullOrWhiteSpace(MicroscopyOthersTwo)
        || !string.IsNullOrWhiteSpace(MicroscopyOthersThree)
        || MicroscopyIsDirect;

    public IReadOnlyList<string> BuildLines()
    {
        var lines = new List<string>();
        if (!string.IsNullOrWhiteSpace(Sample))
        {
            lines.Add($"العينة: {Sample}");
        }

        var organisms = new[] { OrganismA, OrganismB, OrganismC }
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToList();
        if (organisms.Count > 0)
        {
            lines.Add($"الكائنات: {string.Join("، ", organisms)}");
        }

        if (!string.IsNullOrWhiteSpace(CultureCondition))
        {
            lines.Add($"شرط المزرعة: {CultureCondition}");
        }

        if (!string.IsNullOrWhiteSpace(ColonyCount))
        {
            lines.Add($"عدد المستعمرات: {ColonyCount}");
        }

        return lines;
    }

    public IReadOnlyList<string> BuildMicroscopyLines()
    {
        var lines = new List<string>();
        if (!HasMicroscopyContent)
        {
            return lines;
        }

        lines.Add("الفحص المجهري:");
        AddMicroscopyLine(lines, "صديدية", MicroscopyPusCells);
        AddMicroscopyLine(lines, "كريات حمراء", MicroscopyRedBloodCells);
        AddMicroscopyLine(lines, "خلايا بطانية", MicroscopyEpithelialCells);
        AddMicroscopyLine(lines, "بلورات", MicroscopyCrystals);
        AddMicroscopyLine(lines, "فطريات", MicroscopyFungi);
        AddMicroscopyLine(lines, "أخرى 1", MicroscopyOthersOne);
        AddMicroscopyLine(lines, "أخرى 2", MicroscopyOthersTwo);
        AddMicroscopyLine(lines, "أخرى 3", MicroscopyOthersThree);
        if (MicroscopyIsDirect)
        {
            lines.Add("مباشر: نعم");
        }

        return lines;
    }

    private static void AddMicroscopyLine(List<string> lines, string label, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            lines.Add($"{label}: {value}");
        }
    }

    /// <summary>
    /// W-02 S11 (WP-14): sensitivity table rows. Zone decimals use the invariant
    /// culture so 18.5 never renders as 18,5. No commercial-name column (SD-2).
    /// </summary>
    public IReadOnlyList<IReadOnlyList<string>> BuildSensitivityGrid()
    {
        return (SensitivityRows ?? Enumerable.Empty<ReportCultureSensitivityRow>())
            .Select(r => (IReadOnlyList<string>)new[]
            {
                r.AntibioticName,
                r.SensitivityLabel,
                r.InhibitionZoneMm?.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
                r.ScientificName ?? string.Empty
            })
            .ToList();
    }
}

/// <summary>W-02 S11 (WP-14): one sensitivity-table row (no commercial name, SD-2).</summary>
public sealed record ReportCultureSensitivityRow(
    string AntibioticName,
    string SensitivityLabel,
    decimal? InhibitionZoneMm,
    string? ScientificName);
