using System.Globalization;
using QuestPDF;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Features.CultureAndAntibiotics.Common;
using TopLab.Application.Features.SystemAndPrintSettings.Common;

namespace TopLab.Infrastructure.Printing;

/// <summary>
/// GAP-25 (3.9.1.6.5) — prints the antibiotic catalogue: the lab header, the list
/// title, and a table of every antibiotic with its scientific name, symbol,
/// commercial name and the pregnancy/children flags already shown on the
/// antibiotics screen.
///
/// Follows the <see cref="PriceListPdfWriter"/> pattern exactly: A4 portrait,
/// RTL throughout, the lab header from <see cref="LabPrintTextDto"/>, the font
/// resolved through <see cref="ArabicFontResolver"/>, and never overwriting.
/// QuestPDF paginates the table automatically for a long catalogue.
///
/// This class is specific to the antibiotic catalogue: its own port and its own
/// DTO list, sharing no code with the price-list or custom-group writers.
/// </summary>
public sealed class AntibioticListPdfWriter : IAntibioticListPdfWriter
{
    static AntibioticListPdfWriter()
    {
        // P-01 owner ruling (binding for S5/S6): this writer sets the licence in its OWN
        // static constructor and must not rely on another writer running first. The
        // pre-existing PatientReportPdfExporter flake showed what happens when a writer
        // depends on another's static initialiser having already run.
        Settings.License = LicenseType.Community;

        // Arabic shaping needs a system font with Arabic glyphs (e.g. Arial on Windows);
        // QuestPDF 2026+ ships only Lato embedded and disables OS font lookup by default.
        Settings.UseSystemFonts = true;
    }

    public Task WritePdfAsync(
        string absolutePath,
        IReadOnlyList<AntibioticDto> antibiotics,
        LabPrintTextDto labText,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(absolutePath);
        ArgumentNullException.ThrowIfNull(antibiotics);
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

        var lines = BuildTextLines(antibiotics, labText);
#pragma warning disable CA1416 // Windows-only WPF app
        var fontFamily = ArabicFontResolver.Resolve(labText.FontFamily);
#pragma warning restore CA1416
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

                            column.Item().PaddingTop(4).Text(lines.Title).SemiBold().FontSize(fontSize + 4).AlignCenter();

                            column.Item().PaddingVertical(4).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(3);
                                    columns.ConstantColumn(58);
                                    columns.RelativeColumn(3);
                                    columns.ConstantColumn(92);
                                    columns.ConstantColumn(92);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Text(lines.NameColumn).SemiBold().AlignRight();
                                    header.Cell().Text(lines.ScientificNameColumn).SemiBold().AlignRight();
                                    header.Cell().Text(lines.SymbolColumn).SemiBold().AlignRight();
                                    header.Cell().Text(lines.CommercialNameColumn).SemiBold().AlignRight();
                                    header.Cell().Text(lines.PregnancyColumn).SemiBold().AlignRight();
                                    header.Cell().Text(lines.ChildrenColumn).SemiBold().AlignRight();
                                });

                                foreach (var row in lines.Rows)
                                {
                                    table.Cell().Text(row.Name).AlignRight();
                                    table.Cell().Text(row.ScientificName).AlignRight();
                                    table.Cell().Text(row.Symbol).AlignRight();
                                    table.Cell().Text(row.CommercialName).AlignRight();
                                    table.Cell().Text(row.PregnancyFlag).AlignRight();
                                    table.Cell().Text(row.ChildrenFlag).AlignRight();
                                }
                            });

                            column.Item().PaddingTop(6).Text(lines.TotalLine).SemiBold().AlignRight();
                        });
                });
            })
            .GeneratePdf(absolutePath);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Pure content mapping (no PDF dependency): every line the document renders,
    /// in the order the catalogue supplied them. Unit-testable proof that the
    /// antibiotic name and its commercial name reach the document verbatim.
    /// </summary>
    public static AntibioticListTextLines BuildTextLines(
        IReadOnlyList<AntibioticDto> antibiotics,
        LabPrintTextDto labText)
    {
        ArgumentNullException.ThrowIfNull(antibiotics);
        ArgumentNullException.ThrowIfNull(labText);

        var header = new List<string>();
        if (!string.IsNullOrWhiteSpace(labText.LabName))
        {
            header.Add(labText.LabName);
        }

        var rows = antibiotics
            .Select(a => new AntibioticTextRow(
                a.Name,
                ValueOrEmpty(a.ScientificName),
                ValueOrEmpty(a.Symbol),
                ValueOrEmpty(a.CommercialName),
                Flag(a.IsPregnancyFlagged),
                Flag(a.IsChildrenFlagged)))
            .ToList();

        return new AntibioticListTextLines(
            header,
            Title: "قائمة المضادات الحيوية",
            NameColumn: "المضاد",
            ScientificNameColumn: "الاسم العلمي",
            SymbolColumn: "الرمز",
            CommercialNameColumn: "الاسم التجاري",
            PregnancyColumn: "معلَّم للحوامل",
            ChildrenColumn: "معلَّم للأطفال",
            Rows: rows,
            TotalLine: $"عدد المضادات: {antibiotics.Count.ToString(CultureInfo.InvariantCulture)}");
    }

    private static string ValueOrEmpty(string? value) =>
        string.IsNullOrWhiteSpace(value) ? string.Empty : value;

    private static string Flag(bool value) => value ? "نعم" : "لا";

    public sealed record AntibioticTextRow(
        string Name,
        string ScientificName,
        string Symbol,
        string CommercialName,
        string PregnancyFlag,
        string ChildrenFlag);

    public sealed record AntibioticListTextLines(
        IReadOnlyList<string> Header,
        string Title,
        string NameColumn,
        string ScientificNameColumn,
        string SymbolColumn,
        string CommercialNameColumn,
        string PregnancyColumn,
        string ChildrenColumn,
        IReadOnlyList<AntibioticTextRow> Rows,
        string TotalLine);
}
