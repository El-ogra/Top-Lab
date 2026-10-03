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
/// P-01 F9 (PP-03) — prints a custom test-group list (RLS p.121): the lab header,
/// the group name, and a table of the group's tests with their prices.
///
/// Mirrors the <see cref="WorkSheetPdfWriter"/> document pattern (A4 portrait, RTL, the
/// lab header from <see cref="LabPrintTextDto"/>, font via <see cref="ArabicFontResolver"/>,
/// never overwrites) but is its **own class over its own DTO**. It shares no code with
/// <see cref="PriceListPdfWriter"/> and no port with <c>IPriceListPdfWriter</c> (C-6).
/// </summary>
public sealed class CustomGroupPdfWriter : ICustomGroupPdfWriter
{
    static CustomGroupPdfWriter()
    {
        // P-01 owner ruling (binding for S5/S6): this writer sets the licence in its OWN
        // static constructor and must not rely on another writer running first.
        Settings.License = LicenseType.Community;

        // Arabic shaping needs a system font with Arabic glyphs (e.g. Arial on Windows);
        // QuestPDF 2026+ ships only Lato embedded and disables OS font lookup by default.
        Settings.UseSystemFonts = true;
    }

    public Task WritePdfAsync(
        string absolutePath,
        CustomGroupDetailDto group,
        LabPrintTextDto labText,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(absolutePath);
        ArgumentNullException.ThrowIfNull(group);
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

        var lines = BuildTextLines(group, labText);
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

                            column.Item().PaddingTop(4).Text(group.Name).SemiBold().FontSize(fontSize + 4).AlignCenter();

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
    /// in order. Unit-testable proof that group name, item names and prices flow
    /// into the document verbatim (VG-06 "assert real output, not just no throw").
    /// </summary>
    public static CustomGroupTextLines BuildTextLines(CustomGroupDetailDto group, LabPrintTextDto labText)
    {
        ArgumentNullException.ThrowIfNull(group);
        ArgumentNullException.ThrowIfNull(labText);

        var header = new List<string>();
        if (!string.IsNullOrWhiteSpace(labText.LabName))
        {
            header.Add(labText.LabName);
        }

        var rows = group.Items
            .Select(i => new CustomGroupTextRow(
                i.TestName,
                i.TestCode,
                i.Price.ToString("0.##", CultureInfo.InvariantCulture)))
            .ToList();

        return new CustomGroupTextLines(
            header,
            TestColumn: "التحليل",
            CodeColumn: "الكود",
            PriceColumn: "السعر",
            Rows: rows,
            TotalLine: $"عدد التحاليل: {group.Items.Count.ToString(CultureInfo.InvariantCulture)}");
    }

    public sealed record CustomGroupTextRow(string TestName, string TestCode, string Price);

    public sealed record CustomGroupTextLines(
        IReadOnlyList<string> Header,
        string TestColumn,
        string CodeColumn,
        string PriceColumn,
        IReadOnlyList<CustomGroupTextRow> Rows,
        string TotalLine);
}