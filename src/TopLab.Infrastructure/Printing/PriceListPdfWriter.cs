using System.Globalization;
using QuestPDF;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Common;
using TopLab.Application.Features.SystemAndPrintSettings.Common;

namespace TopLab.Infrastructure.Printing;

/// <summary>
/// P-01 F8 (PP-03) — prints a test price list (RLS p.111): the lab header, the
/// list name, and a table of the list's items with their prices.
///
/// Follows the <see cref="WorkSheetPdfWriter"/> pattern exactly: A4 portrait,
/// RTL throughout, the lab header from <see cref="LabPrintTextDto"/>, the font
/// resolved through <see cref="ArabicFontResolver"/>, and never overwriting.
///
/// This class is **specific to price lists** and shares nothing with the custom
/// test-group writer (C-6): different port, different DTO, different document.
/// </summary>
public sealed class PriceListPdfWriter : IPriceListPdfWriter
{
    static PriceListPdfWriter()
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
        PriceListDetailDto priceList,
        LabPrintTextDto labText,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(absolutePath);
        ArgumentNullException.ThrowIfNull(priceList);
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

        var lines = BuildTextLines(priceList, labText);
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

                            column.Item().PaddingTop(4).Text(priceList.Name).SemiBold().FontSize(fontSize + 4).AlignCenter();

                            column.Item().PaddingVertical(4).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(2);
                                    columns.ConstantColumn(90);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Text(lines.TestColumn).SemiBold().AlignRight();
                                    header.Cell().Text(lines.CodeColumn).SemiBold().AlignRight();
                                    header.Cell().Text(lines.PriceColumn).SemiBold().AlignRight();
                                });

                                foreach (var row in lines.Rows)
                                {
                                    table.Cell().Text(row.TestName).AlignRight();
                                    table.Cell().Text(row.TestCode).AlignRight();
                                    table.Cell().Text(row.Price).AlignRight();
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
    /// in order. Unit-testable proof that list name, item names and prices flow
    /// into the document verbatim (VG-05 "assert real output, not just no throw").
    /// </summary>
    public static PriceListTextLines BuildTextLines(PriceListDetailDto priceList, LabPrintTextDto labText)
    {
        ArgumentNullException.ThrowIfNull(priceList);
        ArgumentNullException.ThrowIfNull(labText);

        var header = new List<string>();
        if (!string.IsNullOrWhiteSpace(labText.LabName))
        {
            header.Add(labText.LabName);
        }

        var rows = priceList.Items
            .Select(i => new PriceListTextRow(
                i.TestName,
                i.TestCode,
                i.Price.ToString("0.##", CultureInfo.InvariantCulture)))
            .ToList();

        return new PriceListTextLines(
            header,
            TestColumn: "التحليل",
            CodeColumn: "الكود",
            PriceColumn: "السعر",
            Rows: rows,
            TotalLine: $"عدد التحاليل: {priceList.Items.Count.ToString(CultureInfo.InvariantCulture)}");
    }

    public sealed record PriceListTextRow(string TestName, string TestCode, string Price);

    public sealed record PriceListTextLines(
        IReadOnlyList<string> Header,
        string TestColumn,
        string CodeColumn,
        string PriceColumn,
        IReadOnlyList<PriceListTextRow> Rows,
        string TotalLine);
}