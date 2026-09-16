using System.Globalization;
using QuestPDF;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Features.PatientBilling.Common;
using TopLab.Application.Features.SystemAndPrintSettings.Common;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Settings;

namespace TopLab.Infrastructure.Printing;

/// <summary>
/// Cashier receipt document renderer using QuestPDF (S-01 slice S2, Settled
/// Decision SD-2: proper Arabic shaping/bidi via HarfBuzzSharp — the
/// hand-rolled ASCII-only <c>ReportPdfWriter</c> cannot render Arabic).
/// A5 portrait, RTL throughout.
/// </summary>
public sealed class ReceiptPdfWriter : IReceiptPdfWriter
{
    static ReceiptPdfWriter()
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
        ReceiptDto receipt,
        ReceiptSettings receiptSettings,
        LabPrintTextDto labText,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(absolutePath);
        ArgumentNullException.ThrowIfNull(receipt);
        ArgumentNullException.ThrowIfNull(receiptSettings);
        ArgumentNullException.ThrowIfNull(labText);

        var directory = Path.GetDirectoryName(absolutePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            throw new DirectoryNotFoundException($"Directory not found: {directory}");
        }

        var lines = BuildTextLines(receipt, receiptSettings, labText);
        var fontFamily = string.IsNullOrWhiteSpace(labText.FontFamily) ? "Arial" : labText.FontFamily;
        var fontSize = labText.FontSizePt > 0 ? labText.FontSizePt : 12;
        var topMarginPt = (float)receiptSettings.TopMarginCm * 28.35f;

        if (File.Exists(absolutePath))
        {
            throw new IOException($"File already exists: {absolutePath}");
        }

        Document
            .Create(document =>
            {
                document.Page(page =>
                {
                    page.Size(PageSizes.A5);
                    page.MarginTop(Math.Max(topMarginPt, 14));
                    page.MarginLeft(28);
                    page.MarginRight(28);
                    page.MarginBottom(28);
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

                            if (lines.Header.Count > 0)
                            {
                                column.Item().PaddingVertical(4).LineHorizontal(1);
                            }

                            foreach (var line in lines.Patient)
                            {
                                column.Item().Text(line).AlignRight();
                            }

                            if (lines.Items.Count > 0)
                            {
                                column.Item().PaddingTop(6).Text("التحاليل:").SemiBold().AlignRight();
                                column.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(3);
                                        columns.RelativeColumn(2);
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

                            if (lines.Pickup is not null)
                            {
                                column.Item().PaddingTop(4).Text(lines.Pickup).AlignRight();
                            }

                            if (lines.Footer.Count > 0)
                            {
                                column.Item().PaddingTop(4).LineHorizontal(1);
                                foreach (var line in lines.Footer)
                                {
                                    column.Item().Text(line).FontSize(fontSize - 2).AlignCenter();
                                }
                            }
                        });
                });
            })
            .GeneratePdf(absolutePath);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Pure content mapping (no PDF dependency): every line the document renders,
    /// in order. Unit-testable proof that patient/test/totals data — including
    /// Arabic names — flows into the document verbatim.
    /// </summary>
    public static ReceiptTextLines BuildTextLines(
        ReceiptDto receipt,
        ReceiptSettings receiptSettings,
        LabPrintTextDto labText)
    {
        ArgumentNullException.ThrowIfNull(receipt);
        ArgumentNullException.ThrowIfNull(receiptSettings);
        ArgumentNullException.ThrowIfNull(labText);

        var header = new List<string>();
        // HeaderFooterMode.Images has no image-asset pipeline in this slice, so it
        // falls back to the words header (documented; matches the plan).
        if (receiptSettings.HeaderFooterMode != HeaderFooterMode.None)
        {
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
        }

        var patient = new List<string>
        {
            $"المريض: {receipt.PatientFullName}",
            string.IsNullOrWhiteSpace(receipt.LabId)
                ? $"رقم المريض: {receipt.PatientId.ToString(CultureInfo.InvariantCulture)}"
                : $"رقم المعمل: {receipt.LabId}"
        };

        var items = new List<ReceiptItemLine>();
        if (receiptSettings.TestDetailDisplayMode != TestDetailDisplayMode.Hide)
        {
            foreach (var test in receipt.ChargedTests)
            {
                var description = receiptSettings.TestDetailDisplayMode == TestDetailDisplayMode.ShowWithCode
                    ? $"{test.TestCode} — {test.TestName}"
                    : test.ReceiptName;
                items.Add(new ReceiptItemLine(
                    description,
                    FormatMoney(test.PriceAtOrderTime, receipt.Currency)));
            }
        }

        var totals = new List<string>
        {
            $"إجمالي التحاليل: {FormatMoney(receipt.TotalCharged, receipt.Currency)}",
            $"الخصم: {FormatMoney(receipt.TotalDiscount, receipt.Currency)}",
            $"المدفوع: {FormatMoney(receipt.TotalPaid, receipt.Currency)}",
            $"الباقي: {FormatMoney(receipt.Balance, receipt.Currency)}"
        };

        string? pickup = receiptSettings.PickupTimeDefault.HasValue
            ? $"موعد الاستلام: {receiptSettings.PickupTimeDefault.Value.ToString("HH:mm", CultureInfo.InvariantCulture)}"
            : null;

        return new ReceiptTextLines(header, patient, items, totals, pickup, new List<string>());
    }

    private static string FormatMoney(decimal amount, string currency)
    {
        return $"{amount.ToString("0.00", CultureInfo.InvariantCulture)} {currency}";
    }

    public sealed record ReceiptItemLine(string Description, string Price);

    public sealed record ReceiptTextLines(
        IReadOnlyList<string> Header,
        IReadOnlyList<string> Patient,
        IReadOnlyList<ReceiptItemLine> Items,
        IReadOnlyList<string> Totals,
        string? Pickup,
        IReadOnlyList<string> Footer);
}
