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
    string? ColonyCount)
{
    public bool HasAnyContent =>
        !string.IsNullOrWhiteSpace(Sample)
        || !string.IsNullOrWhiteSpace(OrganismA)
        || !string.IsNullOrWhiteSpace(OrganismB)
        || !string.IsNullOrWhiteSpace(OrganismC)
        || !string.IsNullOrWhiteSpace(CultureCondition)
        || !string.IsNullOrWhiteSpace(ColonyCount);

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
}
