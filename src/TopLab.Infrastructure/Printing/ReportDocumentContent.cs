using System.Text;

namespace TopLab.Infrastructure.Printing;

/// <summary>
/// Pure, testable content model for the clinical report document. Every string
/// that can appear on the page is collected here (Arabic preserved verbatim) so
/// golden tests assert on real content without depending on PDF text extraction.
/// </summary>
public sealed record ReportDocumentContent(
    string ReportTitle,
    string PatientFullName,
    int PatientId,
    string? LabId,
    bool PreferLabId,
    string? LabName,
    string? LabAddress,
    string? LabPhone,
    string? SoftwareBar,
    string? TreatingDoctorName,
    string? ReferralEntityName,
    string? Sex,
    string? AgeText,
    IReadOnlyList<ReportSection> Sections,
    bool DrawHeader,
    bool DrawFooter,
    bool DoctorSignatureEnabled,
    string? ReportDateText,
    string? ReportNumberText,
    string? NormalRangeMarker = null,
    string? HeaderImagePath = null,
    string? FooterImagePath = null)
{
    /// <summary>Every display line, in print order — used by golden content tests.</summary>
    public IReadOnlyList<string> BuildDisplayLines()
    {
        var lines = new List<string>();

        if (DrawHeader)
        {
            if (!string.IsNullOrWhiteSpace(LabName))
            {
                lines.Add(LabName!);
            }

            if (!string.IsNullOrWhiteSpace(LabAddress))
            {
                lines.Add(LabAddress!);
            }

            if (!string.IsNullOrWhiteSpace(LabPhone))
            {
                lines.Add(LabPhone!);
            }

            if (!string.IsNullOrWhiteSpace(SoftwareBar))
            {
                lines.Add(SoftwareBar!);
            }
        }

        lines.Add(ReportTitle);
        lines.Add(PreferLabId && !string.IsNullOrWhiteSpace(LabId)
            ? $"رقم الملف: {LabId}"
            : $"الرقم: {PatientId}");
        lines.Add($"اسم المريض: {PatientFullName}");

        if (!string.IsNullOrWhiteSpace(Sex))
        {
            lines.Add($"الجنس: {Sex}");
        }

        if (!string.IsNullOrWhiteSpace(AgeText))
        {
            lines.Add($"العمر: {AgeText}");
        }

        if (!string.IsNullOrWhiteSpace(TreatingDoctorName))
        {
            lines.Add($"الطبيب المعالج: {TreatingDoctorName}");
        }

        if (!string.IsNullOrWhiteSpace(ReferralEntityName))
        {
            lines.Add($"جهة الإحالة: {ReferralEntityName}");
        }

        foreach (var section in Sections)
        {
            if (!string.IsNullOrWhiteSpace(section.Heading))
            {
                lines.Add(section.Heading!);
            }

            lines.AddRange(section.DisplayLines);

            if (section.Grid is { } grid)
            {
                lines.AddRange(grid.Headers);
                foreach (var row in grid.Rows)
                {
                    lines.AddRange(row);
                }
            }
        }

        if (DoctorSignatureEnabled)
        {
            lines.Add("توقيع الطبيب");
        }

        if (DrawFooter)
        {
            if (!string.IsNullOrWhiteSpace(ReportNumberText))
            {
                lines.Add(ReportNumberText!);
            }

            if (!string.IsNullOrWhiteSpace(ReportDateText))
            {
                lines.Add(ReportDateText!);
            }
        }

        return lines;
    }

    private static readonly string[] ForbiddenEnglishLabels =
    [
        "LabId:", "PatientId:", "Name:", "Paper:", "TopSpace:", "HeaderFooter:",
        "Doctor Signature: Yes", "Doctor Signature:", "Sex:", "Age: ", "Doctor:",
        "Referral:", "Flag:", "Range:", "Reviewed:", "SortMode:", "AutoDisplay:"
    ];

    public bool ContainsEnglishLabels()
    {
        var all = string.Join('\n', BuildDisplayLines());
        return ForbiddenEnglishLabels.Any(f => all.Contains(f, StringComparison.Ordinal));
    }
}

/// <summary>
/// One body section: optional heading + free lines and/or a grid.
/// </summary>
/// <param name="Heading">Plain heading text (golden-content surface).</param>
/// <param name="LayoutLines">
/// Body lines with their per-item layout (GAP-015). The layout is consumed by
/// <c>ReportPageComposer</c>; the text is unchanged by it.
/// </param>
/// <param name="Grid">Optional table rendered under the lines.</param>
/// <param name="HeadingLine">
/// Layout for the heading itself (GAP-015), for the template-controllable
/// group sub-title. <c>null</c> = the heading carries no per-item layout.
/// </param>
public sealed record ReportSection(
    string? Heading,
    IReadOnlyList<ReportContentLine> LayoutLines,
    ReportGrid? Grid,
    ReportContentLine? HeadingLine = null)
{
    /// <summary>Text-only projection of <see cref="LayoutLines"/> (unchanged surface).</summary>
    public IReadOnlyList<string> DisplayLines => LayoutLines.Select(l => l.Text).ToList();

    public static ReportSection FromLines(string? heading, IEnumerable<string> lines)
        => new(heading, lines.Select(l => ReportContentLine.Default(l)).ToList(), null);

    public static ReportSection FromLayoutLines(
        string? heading,
        IEnumerable<ReportContentLine> lines,
        ReportContentLine? headingLine = null)
        => new(heading, lines.ToList(), null, headingLine);
}

/// <summary>
/// GAP-015: a rendered report line and the per-item layout requested for it.
/// All three layout members are nullable and <c>null</c> always means "the
/// composer's existing behaviour" (default line spacing, the page margin as the
/// left position, and no marker).
/// </summary>
public sealed record ReportContentLine(
    string Text,
    decimal? LineSpacing = null,
    decimal? LeftOffsetCm = null,
    string? MarkerSymbol = null)
{
    /// <summary>A line with no per-item layout (renders exactly as before Phase 8).</summary>
    public static ReportContentLine Default(string text) => new(text);

    /// <summary>True when the line carries at least one per-item layout override.</summary>
    public bool HasLayout => LineSpacing is not null || LeftOffsetCm is not null || MarkerSymbol is not null;
}

/// <summary>Column grid rendered as a QuestPDF table (RTL).</summary>
public sealed record ReportGrid(
    IReadOnlyList<string> Headers,
    IReadOnlyList<IReadOnlyList<string>> Rows);
