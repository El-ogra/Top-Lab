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

#pragma warning disable CA1416 // Windows-only WPF app
        var fontFamily = ArabicFontResolver.Resolve(labText.FontFamily);
#pragma warning restore CA1416
        var fontSize = labText.FontSizePt > 0 ? labText.FontSizePt : 12;

        // GAP-010: TopMarginCm is the pre-printed-paper clearance knob, and the
        // settings screen allows up to 30 cm. On A5 that is more than the page is
        // tall: past the limit the printable box is smaller than the receipt body
        // and QuestPDF throws a layout exception, so no receipt could be produced
        // at all. Clamp to the limit rather than fail the print; the body below is
        // laid out against the same clamped value, so the logo bound and the page
        // geometry always agree.
        var requestedTopMarginCm = receiptSettings.TopMarginCm;
        var appliedTopMarginCm = requestedTopMarginCm > ReceiptGeometry.MaxTopMarginCm
            ? ReceiptGeometry.MaxTopMarginCm
            : requestedTopMarginCm;
        var topMarginPt = (float)appliedTopMarginCm * 28.35f;

        // Floor the page applies to MarginTop, in the same units.
        const float MinTopMarginPt = 14f;

        // GAP-007: resolve header/footer fonts, sizes, and colours
#pragma warning disable CA1416 // Windows-only WPF app
        var headerFontFamily = ArabicFontResolver.Resolve(receiptSettings.HeaderFontFamily ?? labText.FontFamily);
        var footerFontFamily = ArabicFontResolver.Resolve(receiptSettings.FooterFontFamily ?? labText.FontFamily);
#pragma warning restore CA1416
        var headerFontSize = receiptSettings.HeaderFontSizePt ?? (fontSize + 2);
        var headerFontColor = ParseColor(receiptSettings.HeaderFontColor);

        var footerFontSize = receiptSettings.FooterFontSizePt ?? (fontSize - 2);
        var footerFontColor = ParseColor(receiptSettings.FooterFontColor);

        // GAP-010: the persisted lab logo, resolved and validated to a file that
        // can be drawn. Null means "no logo" — either nothing was configured or
        // the configured file is missing, unsupported or unreadable — in which
        // case the words header below is the documented fallback.
        var logoPath = ResolveDrawableLogoPath(_imageAssetStore, receiptSettings);

        // GAP-010: the logo replaces the words header for this print, so the
        // pure content map is asked to omit it rather than the renderer
        // guessing. Nothing changes when no logo is drawable.
        var renderWordsHeader = logoPath is null;
        var lines = BuildTextLines(receipt, receiptSettings, labText, barcodePayload, renderWordsHeader);

        // GAP-010: the logo is bounded to the receipt's printable box so an
        // over-tall image can neither exhaust the page nor collide with
        // pre-printed stationery. Both bounds come from the page geometry the
        // document below actually applies (28 pt left/right/bottom, and the
        // same MarginTop floor), so the logo can never push the body off the
        // paper and always stays inside the printed area.
        var receiptBodyWidthPt = ReceiptGeometry.ContentWidthPt(28f, 28f);
        var receiptLogoMaxHeightPt = ReceiptGeometry.LogoMaxHeightPt(
            Math.Max(topMarginPt, MinTopMarginPt), 28f);

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
                            // GAP-007 / GAP-010: the persisted lab logo, at the top of the receipt.
                            //
                            // The two paper choices are the two halves of this one block,
                            // and both come from the same persisted LogoImagePath column:
                            //   * plain paper       -> HeaderFooterMode.Words or Images, with a
                            //                       configured logo. The logo is drawn here and
                            //                       the words header is omitted for this print.
                            //   * pre-printed paper -> the logo is already on the stock, so
                            //                       the user sets HeaderFooterMode.None and uses
                            //                       TopMarginCm to clear the letterhead. Nothing is
                            //                       drawn, which is what stops the logo appearing
                            //                       twice.
                            // A logo that is configured but missing, of an unsupported type
                            // or unreadable falls back to the words header.
                            if (logoPath is not null && receiptSettings.HeaderFooterMode != HeaderFooterMode.None)
                            {
                                var path = logoPath;
                                column.Item().PaddingBottom(6).MaxWidth(receiptBodyWidthPt)
                                    .MaxHeight(receiptLogoMaxHeightPt).AlignCenter()
                                    .Image(path).FitArea();
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
    /// <param name="renderWordsHeader">
    /// GAP-010: false when a logo is being drawn for this print, in which case
    /// the words header is omitted instead of printed beneath the logo.
    /// Defaults to true, so every existing caller is unaffected.
    /// </param>
    public static ReceiptTextLines BuildTextLines(
        ReceiptDto receipt,
        ReceiptSettings receiptSettings,
        LabPrintTextDto labText,
        string barcodePayload,
        bool renderWordsHeader = true)
    {
        ArgumentNullException.ThrowIfNull(receipt);
        ArgumentNullException.ThrowIfNull(receiptSettings);
        ArgumentNullException.ThrowIfNull(labText);
        ArgumentException.ThrowIfNullOrWhiteSpace(barcodePayload);

        var header = new List<string>();
        // HeaderFooterMode.Images and .Words both carry a logo pipeline (GAP-007).
        // The words header is the fallback: it is suppressed only while a
        // drawable logo replaces it for this print (GAP-010).
        if (renderWordsHeader && receiptSettings.HeaderFooterMode != HeaderFooterMode.None)
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

        // GAP-010: no footer text exists yet (there is no footer content in the
        // receipt content map); the slot is kept so the header/footer mode is
        // honoured uniformly by the renderer.
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

    /// <summary>
    /// GAP-010: the persisted lab logo, resolved and validated to a file that can
    /// actually be drawn — or null when no logo may be rendered.
    /// <para>
    /// The resolution order is: a configured relative path, resolved through the
    /// asset store; then the file must exist, be of a type QuestPDF can decode
    /// (PNG, JPEG, WEBP), and pass a format check on its header. A file that is
    /// present but unreadable, unsupported or malformed yields null rather than
    /// aborting the receipt, so the caller falls back to the words header and the
    /// receipt still prints.
    /// </para>
    /// </summary>
    internal static string? ResolveDrawableLogoPath(
        IReceiptImageAssetStore? assetStore,
        ReceiptSettings receiptSettings)
    {
        if (assetStore is null || string.IsNullOrWhiteSpace(receiptSettings.LogoImagePath))
        {
            return null;
        }

        var path = assetStore.ResolveLogoPath(receiptSettings.LogoImagePath);
        if (path is null || !File.Exists(path))
        {
            return null;
        }

        var extension = Path.GetExtension(path).ToLowerInvariant();
        if (!SupportedLogoExtensions.Contains(extension))
        {
            return null;
        }

        if (!HasReadableHeader(path))
        {
            // Corrupt or truncated image: never let a logo break a receipt.
            return null;
        }

        return path;
    }

    /// <summary>QuestPDF's documented image support set (PNG, JPEG, WEBP).</summary>
    private static readonly string[] SupportedLogoExtensions = { ".png", ".jpg", ".jpeg", ".webp" };

    /// <summary>
    /// Reads the first bytes and confirms the file still carries the magic number
    /// of its extension. A file that passes the type check but fails here (a
    /// renamed text file, a truncated download) is treated as unusable.
    /// </summary>
    private static bool HasReadableHeader(string path)
    {
        try
        {
            Span<byte> header = stackalloc byte[12];
            using var stream = File.OpenRead(path);
            var read = 0;
            while (read < header.Length)
            {
                var chunk = stream.Read(header[read..]);
                if (chunk <= 0)
                {
                    break;
                }

                read += chunk;
            }

            if (read < 12)
            {
                return false;
            }

            if (header[..8].SequenceEqual(PngSignature))
            {
                return true;
            }

            if (header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF)
            {
                return true;
            }

            // RIFF....WEBP
            return header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46
                && header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50;
        }
        catch (IOException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
    }

    private static readonly byte[] PngSignature = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };

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
