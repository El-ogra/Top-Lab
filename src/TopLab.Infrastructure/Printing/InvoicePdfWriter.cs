using System.Globalization;
using QuestPDF;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Features.PatientBilling.Common;
using TopLab.Application.Features.SystemAndPrintSettings.Common;

namespace TopLab.Infrastructure.Printing;

/// <summary>
/// Itemized invoice document renderer using QuestPDF (S-01 slice S3, Settled
/// Decision SD-2). A5 portrait, RTL throughout: lab header, invoice number +
/// date, patient block, itemized table (test name, frozen price), totals block
/// with currency.
/// </summary>
public sealed class InvoicePdfWriter : IInvoicePdfWriter
{
    static InvoicePdfWriter()
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
        InvoiceDto invoice,
        LabPrintTextDto labText,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(absolutePath);
        ArgumentNullException.ThrowIfNull(invoice);
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

        var lines = BuildTextLines(invoice, labText);
        var fontFamily = string.IsNullOrWhiteSpace(labText.FontFamily) ? "Arial" : labText.FontFamily;
        var fontSize = labText.FontSizePt > 0 ? labText.FontSizePt : 12;

        Document
            .Create(document =>
            {
                document.Page(page =>
                {
                    page.Size(PageSizes.A5);
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

                            column.Item().PaddingVertical(4).Text("فاتورة").SemiBold().FontSize(fontSize + 6).AlignCenter();
                            column.Item().Text(lines.InvoiceTitle).SemiBold().AlignRight();
                            column.Item().PaddingBottom(4).LineHorizontal(1);

                            foreach (var line in lines.Patient)
                            {
                                column.Item().Text(line).AlignRight();
                            }

                            if (lines.Items.Count > 0)
                            {
                                column.Item().PaddingTop(6).Text("التحاليل المطلوبة:").SemiBold().AlignRight();
                                column.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(3);
                                        columns.RelativeColumn(2);
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().Text("التحليل").SemiBold().AlignRight();
                                        header.Cell().Text("السعر").SemiBold().AlignLeft();
                                    });

                                    foreach (var item in lines.Items)
                                    {
                                        table.Cell().Text(item.Description).AlignRight();
                                        table.Cell().Text(item.Price).AlignLeft();
                                    }
                                });
                            }

                            column.Item().PaddingTop(6).LineHorizontal(1);

                            foreach (var line in lines.Totals)
                            {
                                column.Item().Text(line).AlignRight();
                            }
                        });
                });
            })
            .GeneratePdf(absolutePath);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Pure content mapping (no PDF dependency): every line the document renders,
    /// in order. Unit-testable proof that invoice data — including Arabic names
    /// and frozen line prices — flows into the document verbatim.
    /// </summary>
    public static InvoiceTextLines BuildTextLines(InvoiceDto invoice, LabPrintTextDto labText)
    {
        ArgumentNullException.ThrowIfNull(invoice);
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

        var title = invoice.InvoiceNumber.HasValue
            ? $"رقم الفاتورة: {invoice.InvoiceNumber.Value.ToString(CultureInfo.InvariantCulture)}"
            : "معاينة — بدون رقم";
        if (invoice.IssuedAtUtc.HasValue)
        {
            title += $" — {invoice.IssuedAtUtc.Value.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)}";
        }

        var patient = new List<string>
        {
            $"المريض: {invoice.PatientFullName}",
            string.IsNullOrWhiteSpace(invoice.LabId)
                ? $"رقم المريض: {invoice.PatientId.ToString(CultureInfo.InvariantCulture)}"
                : $"رقم المعمل: {invoice.LabId}"
        };

        var items = invoice.ChargedTests
            .Select(t => new InvoiceItemLine(
                t.TestName,
                $"{t.PriceAtOrderTime.ToString("0.00", CultureInfo.InvariantCulture)} {invoice.Currency}"))
            .ToList();

        var totals = new List<string>
        {
            $"إجمالي التحاليل: {FormatMoney(invoice.TotalCharged, invoice.Currency)}",
            $"الخصم: {FormatMoney(invoice.TotalDiscount, invoice.Currency)}",
            $"المدفوع: {FormatMoney(invoice.TotalPaid, invoice.Currency)}",
            $"الباقي: {FormatMoney(invoice.Balance, invoice.Currency)}"
        };

        return new InvoiceTextLines(header, title, patient, items, totals);
    }

    private static string FormatMoney(decimal amount, string currency)
    {
        return $"{amount.ToString("0.00", CultureInfo.InvariantCulture)} {currency}";
    }

    public sealed record InvoiceItemLine(string Description, string Price);

    public sealed record InvoiceTextLines(
        IReadOnlyList<string> Header,
        string InvoiceTitle,
        IReadOnlyList<string> Patient,
        IReadOnlyList<InvoiceItemLine> Items,
        IReadOnlyList<string> Totals);
}
