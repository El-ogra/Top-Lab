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
    string? ReportNumberText)
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

/// <summary>One body section: optional heading + free lines and/or a grid.</summary>
public sealed record ReportSection(
    string? Heading,
    IReadOnlyList<string> DisplayLines,
    ReportGrid? Grid)
{
    public static ReportSection FromLines(string? heading, IEnumerable<string> lines)
        => new(heading, lines.ToList(), null);
}

/// <summary>Column grid rendered as a QuestPDF table (RTL).</summary>
public sealed record ReportGrid(
    IReadOnlyList<string> Headers,
    IReadOnlyList<IReadOnlyList<string>> Rows);
