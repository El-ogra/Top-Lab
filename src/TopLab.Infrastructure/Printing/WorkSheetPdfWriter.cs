using System.Globalization;
using QuestPDF;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Features.SystemAndPrintSettings.Common;
using TopLab.Application.Features.WorkSheets.Common;

namespace TopLab.Infrastructure.Printing;

/// <summary>
/// Per-visit bench-sheet renderer using QuestPDF (S-01 slice S4, Settled
/// Decision SD-2). A4 portrait, RTL throughout: sections = test groups, lines =
/// tests with the textual catalog barcode, sample-kind letters, and a draw
/// checkbox column. Barcodes print as text values; composing scannable images
/// per line from the S1 renderer is a noted future enhancement, out of scope.
/// </summary>
public sealed class WorkSheetPdfWriter : IWorkSheetPdfWriter
{
    static WorkSheetPdfWriter()
    {
        // License fit for Top-Lab's scale owner-confirmed (S-01 SD-2).
        Settings.License = LicenseType.Community;

        // Arabic shaping needs a system font with Arabic glyphs (e.g. Arial on
        // Windows); QuestPDF 2026+ ships only Lato embedded and disables OS
        // font lookup by default.
        Settings.UseSystemFonts = true;
    }

    public Task WritePdfAsync(
        string absolutePath,
        VisitWorkSheetDto sheet,
        LabPrintTextDto labText,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(absolutePath);
        ArgumentNullException.ThrowIfNull(sheet);
        ArgumentNullException.ThrowIfNull(labText);

        var directory = Path.GetDirectoryName(absolutePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            throw new DirectoryNotFoundException($"Directory not found: {directory}");
        }

        if (File.Exists(absolutePath))
        {
            throw new IOException($"File already exists: {absolutePath}");
        }

        var lines = BuildTextLines(sheet, labText);
        var fontFamily = string.IsNullOrWhiteSpace(labText.FontFamily) ? "Arial" : labText.FontFamily;
        var fontSize = labText.FontSizePt > 0 ? labText.FontSizePt : 12;

        Document
            .Create(document =>
            {
                document.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(28);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(style => style
                        .FontFamily(fontFamily)
                        .FontSize(fontSize)
                        .DirectionFromRightToLeft());

                    page.ContentFromRightToLeft();
                    page.Content()
                        .Column(column =>
                        {
                            foreach (var line in lines.Header)
                            {
                                column.Item().Text(line).SemiBold().FontSize(fontSize + 2).AlignRight();
                            }

                            column.Item().PaddingVertical(4).Text("ورقة عمل الزيارة").SemiBold().FontSize(fontSize + 6).AlignCenter();

                            foreach (var line in lines.Patient)
                            {
                                column.Item().Text(line).AlignRight();
                            }

                            column.Item().PaddingTop(4).LineHorizontal(1);

                            foreach (var section in lines.Sections)
                            {
                                column.Item().PaddingTop(6).Text(section.Name).SemiBold().FontSize(fontSize + 1).AlignRight();
                                column.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(4);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(1);
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().Text("التحليل").SemiBold().AlignRight();
                                        header.Cell().Text("الباركود").SemiBold().AlignRight();
                                        header.Cell().Text("العينة").SemiBold().AlignRight();
                                        header.Cell().Text("سحب").SemiBold().AlignCenter();
                                    });

                                    foreach (var row in section.Rows)
                                    {
                                        table.Cell().Text(row.TestName).AlignRight();
                                        table.Cell().Text(row.Barcode).AlignRight();
                                        table.Cell().Text(row.SampleKinds).AlignRight();
                                        table.Cell().Text(row.Drawn).AlignCenter();
                                    }
                                });
                            }

                            column.Item().PaddingTop(6).Text(lines.TotalLine).SemiBold().AlignRight();
                        });
                });
            })
            .GeneratePdf(absolutePath);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Pure content mapping (no PDF dependency): every line the document renders,
    /// in order. Unit-testable proof that per-line sample flags and textual
    /// barcodes flow into the document verbatim.
    /// </summary>
    public static WorkSheetTextLines BuildTextLines(VisitWorkSheetDto sheet, LabPrintTextDto labText)
    {
        ArgumentNullException.ThrowIfNull(sheet);
        ArgumentNullException.ThrowIfNull(labText);

        var header = new List<string>();
        if (!string.IsNullOrWhiteSpace(labText.LabName))
        {
            header.Add(labText.LabName);
        }

        var identifier = sheet.PrintLabIdInsteadOfPatientId && !string.IsNullOrWhiteSpace(sheet.LabId)
            ? $"رقم المعمل: {sheet.LabId}"
            : $"رقم المريض: {sheet.PatientId.ToString(CultureInfo.InvariantCulture)}";

        var patient = new List<string>
        {
            $"المريض: {sheet.PatientFullName}",
            identifier
        };

        if (sheet.PrintDateTimeOnTubeBarcode)
        {
            patient.Add($"تاريخ الطباعة: {DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)}");
        }

        if (sheet.PrintFileExternalBarcode)
        {
            patient.Add($"باركود الملف: {identifier}");
        }

        var samplesByLine = sheet.Samples.ToDictionary(s => s.PatientTestId);

        var sections = sheet.Sections
            .Select(s => new WorkSheetTextSection(
                s.SectionName,
                s.Lines.Select(l => new WorkSheetTextRow(
                    l.TestName,
                    string.IsNullOrWhiteSpace(l.Barcode) ? "—" : l.Barcode,
                    SampleKinds(samplesByLine.TryGetValue(l.PatientTestId, out var sample) ? sample : null),
                    l.IsSampleDrawn ? "[X]" : "[ ]")).ToList()))
            .ToList();

        return new WorkSheetTextLines(
            header,
            patient,
            sections,
            $"إجمالي التحاليل: {sheet.TotalTests.ToString(CultureInfo.InvariantCulture)}");
    }

    public static string SampleKinds(VisitWorkSheetSampleDto? sample)
    {
        if (sample is null)
        {
            return "—";
        }

        var kinds = new List<string>(6);
        if (sample.IsUrine)
        {
            kinds.Add("U");
        }

        if (sample.IsStool)
        {
            kinds.Add("S");
        }

        if (sample.IsBlood)
        {
            kinds.Add("B");
        }

        if (sample.IsSemen)
        {
            kinds.Add("Se");
        }

        if (sample.IsCsf)
        {
            kinds.Add("CSF");
        }

        if (sample.IsTakenOutsideLab)
        {
            kinds.Add("خارج");
        }

        return kinds.Count == 0 ? "—" : string.Join(" ", kinds);
    }

    public sealed record WorkSheetTextRow(string TestName, string Barcode, string SampleKinds, string Drawn);

    public sealed record WorkSheetTextSection(string Name, IReadOnlyList<WorkSheetTextRow> Rows);

    public sealed record WorkSheetTextLines(
        IReadOnlyList<string> Header,
        IReadOnlyList<string> Patient,
        IReadOnlyList<WorkSheetTextSection> Sections,
        string TotalLine);
}
