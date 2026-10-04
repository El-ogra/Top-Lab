using System.Globalization;
using QuestPDF;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Features.Statistics.Common;
using TopLab.Application.Features.SystemAndPrintSettings.Common;

namespace TopLab.Infrastructure.Printing;

/// <summary>
/// R-F05 (BR-F05-13…16) — prints the banded result monitor: the lab header, the criteria
/// (test, band, period) and the same nine grid columns the screen shows, as an A4
/// portrait RTL table.
///
/// Follows the <see cref="PriceListPdfWriter"/> pattern exactly: its own static
/// constructor setting the licence, argument/directory/file guards, the font resolved
/// through <see cref="ArabicFontResolver"/> inside the <c>CA1416</c> pragma pair, RTL
/// throughout, and never overwriting.
///
/// This class is **specific to the banded monitor** and shares nothing with the
/// price-list or custom-group writers: different port, different DTO, different document.
/// </summary>
public sealed class BandedResultMonitorPdfWriter : IBandedResultMonitorPdfWriter
{
    static BandedResultMonitorPdfWriter()
    {
        // Same binding owner ruling as PriceListPdfWriter/CustomGroupPdfWriter: this writer
        // sets the licence in its OWN static constructor and must not rely on another
        // writer running first.
        Settings.License = LicenseType.Community;

        // Arabic shaping needs a system font with Arabic glyphs (e.g. Arial on Windows).
        Settings.UseSystemFonts = true;
    }

    public Task WritePdfAsync(
        string absolutePath,
        BandedResultMonitorDto monitor,
        LabPrintTextDto labText,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(absolutePath);
        ArgumentNullException.ThrowIfNull(monitor);
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

        var lines = BuildTextLines(monitor, labText);
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

                            column.Item().PaddingTop(4).Text(lines.CriteriaLine).SemiBold().FontSize(fontSize + 2).AlignRight();

                            column.Item().PaddingVertical(4).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(70);  // التاريخ
                                    columns.RelativeColumn(3);  // المريض
                                    columns.ConstantColumn(55);  // الرقم
                                    columns.ConstantColumn(50);  // الجنس
                                    columns.ConstantColumn(60);  // العمر
                                    columns.RelativeColumn(3);  // جهة الإحالة
                                    columns.RelativeColumn(2);  // التحليل
                                    columns.ConstantColumn(60);  // النتيجة
                                    columns.RelativeColumn(2);  // الحالة
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Text(lines.Headers.Date).SemiBold().AlignRight();
                                    header.Cell().Text(lines.Headers.Patient).SemiBold().AlignRight();
                                    header.Cell().Text(lines.Headers.PatientNumber).SemiBold().AlignRight();
                                    header.Cell().Text(lines.Headers.Sex).SemiBold().AlignRight();
                                    header.Cell().Text(lines.Headers.Age).SemiBold().AlignRight();
                                    header.Cell().Text(lines.Headers.ReferralEntity).SemiBold().AlignRight();
                                    header.Cell().Text(lines.Headers.Test).SemiBold().AlignRight();
                                    header.Cell().Text(lines.Headers.Result).SemiBold().AlignRight();
                                    header.Cell().Text(lines.Headers.Status).SemiBold().AlignRight();
                                });

                                foreach (var row in lines.Rows)
                                {
                                    table.Cell().Text(row.Date).AlignRight();
                                    table.Cell().Text(row.PatientFullName).AlignRight();
                                    table.Cell().Text(row.PatientNumber).AlignRight();
                                    table.Cell().Text(row.Sex).AlignRight();
                                    table.Cell().Text(row.Age).AlignRight();
                                    table.Cell().Text(row.ReferralEntityName).AlignRight();
                                    table.Cell().Text(row.TestName).AlignRight();
                                    table.Cell().Text(row.ResultValue).AlignRight();
                                    table.Cell().Text(row.StatusText).AlignRight();
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
    /// Pure content mapping (no PDF dependency): every line the document renders, in
    /// order — the lab header, the criteria line (test, band, period), the nine grid
    /// headers and each row — so tests can assert on real strings rather than on
    /// "it did not throw".
    /// </summary>
    public static BandedMonitorTextLines BuildTextLines(BandedResultMonitorDto monitor, LabPrintTextDto labText)
    {
        ArgumentNullException.ThrowIfNull(monitor);
        ArgumentNullException.ThrowIfNull(labText);

        var header = new List<string>();
        if (!string.IsNullOrWhiteSpace(labText.LabName))
        {
            header.Add(labText.LabName);
        }

        if (!string.IsNullOrWhiteSpace(labText.Address))
        {
            header.Add(labText.Address);
        }

        if (!string.IsNullOrWhiteSpace(labText.Phone))
        {
            header.Add(labText.Phone);
        }

        var rows = monitor.Rows
            .Select(r => new BandedMonitorTextRow(
                r.EnteredAtUtc.ToString("yyyy/MM/dd HH:mm", CultureInfo.InvariantCulture),
                r.PatientFullName,
                r.PatientId.ToString(CultureInfo.InvariantCulture),
                r.PatientSex,
                $"{r.PatientAgeValue.ToString(CultureInfo.InvariantCulture)} {r.PatientAgeUnit}",
                r.ReferralEntityName,
                r.TestName,
                r.ResultValue,
                r.StatusText))
            .ToList();

        return new BandedMonitorTextLines(
            header,
            CriteriaLine: $"التحليل: {monitor.TestName} — النطاق: {Band(monitor.MinValue)} إلى {Band(monitor.MaxValue)} — الفترة: {monitor.From.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture)} إلى {monitor.To.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture)}",
            Headers: new BandedMonitorGridHeaders(
                Date: "التاريخ",
                Patient: "المريض",
                PatientNumber: "الرقم",
                Sex: "الجنس",
                Age: "العمر",
                ReferralEntity: "جهة الإحالة",
                Test: "التحليل",
                Result: "النتيجة",
                Status: "الحالة"),
            Rows: rows,
            TotalLine: $"عدد النتائج: {monitor.Rows.Count.ToString(CultureInfo.InvariantCulture)}");
    }

    private static string Band(decimal value) => value.ToString("0.####", CultureInfo.InvariantCulture);

    public sealed record BandedMonitorTextRow(
        string Date,
        string PatientFullName,
        string PatientNumber,
        string Sex,
        string Age,
        string ReferralEntityName,
        string TestName,
        string ResultValue,
        string StatusText);

    public sealed record BandedMonitorGridHeaders(
        string Date,
        string Patient,
        string PatientNumber,
        string Sex,
        string Age,
        string ReferralEntity,
        string Test,
        string Result,
        string Status);

    public sealed record BandedMonitorTextLines(
        IReadOnlyList<string> Header,
        string CriteriaLine,
        BandedMonitorGridHeaders Headers,
        IReadOnlyList<BandedMonitorTextRow> Rows,
        string TotalLine);
}