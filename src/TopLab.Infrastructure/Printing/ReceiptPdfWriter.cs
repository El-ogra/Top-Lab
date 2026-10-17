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
using TopLab.Infrastructure.Barcode;

namespace TopLab.Infrastructure.Printing;

/// <summary>
/// Cashier receipt document renderer using QuestPDF (S-01 slice S2, Settled
/// Decision SD-2: proper Arabic shaping/bidi via HarfBuzzSharp — the
/// hand-rolled ASCII-only <c>ReportPdfWriter</c> cannot render Arabic).
/// A5 portrait, RTL throughout.
/// </summary>
public sealed class ReceiptPdfWriter : IReceiptPdfWriter
{
    private readonly BarcodeLabelRenderer _renderer;
    private readonly IReceiptImageAssetStore? _imageAssetStore;

    static ReceiptPdfWriter()
    {
        // Community-eligible reconfirmed 2026-09-29 by owner decision; revisit before commercial distribution.
        Settings.License = LicenseType.Community;

        // Arabic shaping needs a system font with Arabic glyphs (e.g. Arial on
        // Windows); QuestPDF 2026+ ships only Lato embedded and disables OS
        // font lookup by default.
        Settings.UseSystemFonts = true;
    }

    public ReceiptPdfWriter(BarcodeLabelRenderer renderer, IReceiptImageAssetStore? imageAssetStore = null)
    {
        _renderer = renderer;
        _imageAssetStore = imageAssetStore;
    }

    public Task WritePdfAsync(
        string absolutePath,
        ReceiptDto receipt,
        ReceiptSettings receiptSettings,
        LabPrintTextDto labText,
        string barcodePayload,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(absolutePath);
        ArgumentNullException.ThrowIfNull(receipt);
        ArgumentNullException.ThrowIfNull(receiptSettings);
        ArgumentNullException.ThrowIfNull(labText);
        ArgumentException.ThrowIfNullOrWhiteSpace(barcodePayload);

        var directory = Path.GetDirectoryName(absolutePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            throw new DirectoryNotFoundException($"Directory not found: {directory}");
        }

        var lines = BuildTextLines(receipt, receiptSettings, labText, barcodePayload);
#pragma warning disable CA1416 // Windows-only WPF app
        var fontFamily = ArabicFontResolver.Resolve(labText.FontFamily);
#pragma warning restore CA1416
        var fontSize = labText.FontSizePt > 0 ? labText.FontSizePt : 12;
        var topMarginPt = (float)receiptSettings.TopMarginCm * 28.35f;

        // GAP-007: resolve header/footer fonts, sizes, and colours
#pragma warning disable CA1416 // Windows-only WPF app
        var headerFontFamily = ArabicFontResolver.Resolve(receiptSettings.HeaderFontFamily ?? labText.FontFamily);
        var footerFontFamily = ArabicFontResolver.Resolve(receiptSettings.FooterFontFamily ?? labText.FontFamily);
#pragma warning restore CA1416
        var headerFontSize = receiptSettings.HeaderFontSizePt ?? (fontSize + 2);
        var headerFontColor = ParseColor(receiptSettings.HeaderFontColor);

        var footerFontSize = receiptSettings.FooterFontSizePt ?? (fontSize - 2);
        var footerFontColor = ParseColor(receiptSettings.FooterFontColor);

        // GAP-007: resolve logo path through the asset store
        string? logoPath = null;
        if (!string.IsNullOrWhiteSpace(receiptSettings.LogoImagePath) && _imageAssetStore is not null)
        {
            logoPath = _imageAssetStore.ResolveLogoPath(receiptSettings.LogoImagePath);
        }

        if (File.Exists(absolutePath))
        {
            throw new IOException($"File already exists: {absolutePath}");
        }

        // Phase 1 REF-127: scannable identifier barcode via the shared pipeline.
        // On renderer/encoder failure the readable identifier line below still
        // prints (envelope fallback idiom, decision 66-A).
        byte[]? barcodePng = null;
        try
        {
            var label = _renderer.Render(lines.BarcodePayload);
            barcodePng = BarcodePngEncoder.Encode(label.Pixels, label.Width, label.Height);
        }
        catch
        {
            barcodePng = null;
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
                            // GAP-007: render the lab logo at the top of the receipt
                            if (logoPath is not null && File.Exists(logoPath))
                            {
                                column.Item().PaddingBottom(6).AlignCenter().Image(logoPath);
                            }

                            foreach (var line in lines.Header)
                            {
                                column.Item().Text(line).SemiBold()
                                    .FontSize((float)headerFontSize)
                                    .AlignRight()
                                    .FontColor(headerFontColor)
                                    .FontFamily(headerFontFamily);
                            }

                            if (lines.Header.Count > 0)
                            {
                                column.Item().PaddingVertical(4).LineHorizontal(1);
                            }

                            foreach (var line in lines.Patient)
                            {
                                column.Item().Text(line).AlignRight();
                            }

                            // REF-127: barcode block immediately after the patient
                            // identification lines (cashier scan-first workflow,
                            // decision 127-B). The payload is the pure identifier
                            // (digits/Latin), bidi-neutral inside the RTL layout.
                            if (barcodePng is not null)
                            {
                                var png = barcodePng;
                                column.Item().PaddingTop(6).Width(170).Height(43).Image(png);
                            }

                            column.Item().Text(lines.BarcodePayload).AlignRight();

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
                                    column.Item().Text(line)
                                        .FontSize((float)footerFontSize)
                                        .AlignCenter()
                                        .FontColor(footerFontColor)
                                        .FontFamily(footerFontFamily);
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
    /// Arabic names — flows into the document verbatim. <c>barcodePayload</c> is
    /// the identifier-rule payload (Phase 1 REF-127) drawn as a scannable image
    /// plus the readable line after the patient block.
    /// </summary>
    public static ReceiptTextLines BuildTextLines(
        ReceiptDto receipt,
        ReceiptSettings receiptSettings,
        LabPrintTextDto labText,
        string barcodePayload)
    {
        ArgumentNullException.ThrowIfNull(receipt);
        ArgumentNullException.ThrowIfNull(receiptSettings);
        ArgumentNullException.ThrowIfNull(labText);
        ArgumentException.ThrowIfNullOrWhiteSpace(barcodePayload);

        var header = new List<string>();
        // HeaderFooterMode.Images now has a logo pipeline (GAP-007); the words
        // header is still rendered as a fallback when no logo is configured.
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

        return new ReceiptTextLines(header, patient, items, totals, pickup, new List<string>(), barcodePayload);
    }

    private static string ParseColor(string? colorString)
    {
        if (string.IsNullOrWhiteSpace(colorString))
        {
            return "#000000";
        }

        // Accept hex color strings (e.g. "#FF0000" or "FF0000")
        var hex = colorString.StartsWith("#") ? colorString : "#" + colorString;
        if (hex.Length == 7 || hex.Length == 9)
        {
            return hex;
        }

        return "#000000";
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
        IReadOnlyList<string> Footer,
        string BarcodePayload);
}
