using TopLab.Domain.Common;
using TopLab.Domain.Common.Ids;

namespace TopLab.Domain.Tests;

/// <summary>
/// Central catalog analyte. Per Decision 1 the analyte owns the current reference
/// range (exactly one <see cref="AnalyteReferenceRange"/> aggregate); profiles and
/// tests never own a live range. May optionally be mapped 1:1 to a simple
/// <see cref="Test"/> via nullable <c>AnalyteId</c> on <see cref="Test"/>.
/// </summary>
public sealed class Analyte : AuditableEntity<AnalyteId>
{
    public const int MaxNameLength = 150;

    public string Name { get; private set; } = default!;

    public string ReportName { get; private set; } = default!;

    public bool IsActive { get; private set; }

    /// <summary>The single current range aggregate for this analyte. Null until attached.</summary>
    public AnalyteReferenceRange? CurrentRange { get; private set; }

    private Analyte()
    {
    }

    private Analyte(AnalyteId id, string name, string reportName, bool isActive)
        : base(id)
    {
        Name = name;
        ReportName = reportName;
        IsActive = isActive;
    }

    public static Analyte Create(
        AnalyteId id,
        string name,
        string reportName,
        bool isActive = true)
    {
        Guard(name, reportName);

        return new Analyte(id, name.Trim(), reportName.Trim(), isActive);
    }

    public void Update(string name, string reportName)
    {
        Guard(name, reportName);

        Name = name.Trim();
        ReportName = reportName.Trim();
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Reactivate()
    {
        IsActive = true;
    }

    /// <summary>Attaches this analyte's single current range aggregate. Exactly one is allowed.</summary>
    public void AttachCurrentRange(AnalyteReferenceRange range)
    {
        ArgumentNullException.ThrowIfNull(range);

        if (range.AnalyteId != Id)
        {
            throw new ArgumentException("Range must belong to this analyte.", nameof(range));
        }

        if (CurrentRange is not null)
        {
            throw new InvalidOperationException("Analyte already owns exactly one current reference range.");
        }

        CurrentRange = range;
    }

    private static void Guard(string name, string reportName)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(reportName))
        {
            throw new ArgumentException("Name/ReportName required.");
        }

        if (name.Trim().Length > MaxNameLength || reportName.Trim().Length > MaxNameLength)
        {
            throw new ArgumentException($"Name/ReportName must be at most {MaxNameLength} characters.");
        }
    }
}