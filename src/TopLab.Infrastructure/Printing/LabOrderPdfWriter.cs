using System.Globalization;
using QuestPDF;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Features.PatientEnvelope.Common;
using TopLab.Application.Features.SystemAndPrintSettings.Common;
using TopLab.Infrastructure.Barcode;

namespace TopLab.Infrastructure.Printing;

/// <summary>
/// Laboratory-order (requisition) slip renderer using QuestPDF (Phase 1,
/// REF-068). A5 portrait, RTL throughout: lab header words, patient block,
/// ordered-test table (code + name), footer identifier barcode image +
/// human-readable identifier text. The barcode image reuses REF-066's shared
/// pipeline (<c>BarcodeLabelRenderer</c> + <c>BarcodePngEncoder</c>).
/// An empty order list still prints the patient block + barcode with a
/// no-tests line (decision 68-D, worksheet empty-state idiom).
/// </summary>
public sealed class LabOrderPdfWriter : ILabOrderPdfWriter
{
    private readonly BarcodeLabelRenderer _renderer;

    static LabOrderPdfWriter()
    {
        // Community-eligible reconfirmed 2026-09-29 by owner decision; revisit before commercial distribution.
        Settings.License = LicenseType.Community;

        // Arabic shaping needs a system font with Arabic glyphs (e.g. Arial on
        // Windows); QuestPDF 2026+ ships only Lato embedded and disables OS
        // font lookup by default.
        Settings.UseSystemFonts = true;
    }

    public LabOrderPdfWriter(BarcodeLabelRenderer renderer)
    {
        _renderer = renderer;
    }

    public Task WritePdfAsync(
        string absolutePath,
        LabOrderDto order,
        LabPrintTextDto labText,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(absolutePath);
        ArgumentNullException.ThrowIfNull(order);
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

        var lines = BuildTextLines(order, labText);
#pragma warning disable CA1416 // Windows-only WPF app
        var fontFamily = ArabicFontResolver.Resolve(labText.FontFamily);
#pragma warning restore CA1416
        var fontSize = labText.FontSizePt > 0 ? labText.FontSizePt : 12;

        // Footer barcode image via the shared pipeline; on failure the slip
        // falls back to the human-readable identifier (decision 66-A idiom).
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

                            column.Item().PaddingVertical(4).Text("طلب تحاليل").SemiBold().FontSize(fontSize + 6).AlignCenter();
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
                                        header.Cell().Text("الكود").SemiBold().AlignLeft();
                                    });

                                    foreach (var item in lines.Items)
                                    {
                                        table.Cell().Text(item.TestName).AlignRight();
                                        table.Cell().Text(item.TestCode).AlignLeft();
                                    }
                                });
                            }
                            else if (lines.EmptyNotice is not null)
                            {
                                column.Item().PaddingTop(6).Text(lines.EmptyNotice).AlignRight();
                            }

                            column.Item().PaddingTop(6).LineHorizontal(1);

                            if (barcodePng is not null)
                            {
                                var png = barcodePng;
                                column.Item().PaddingTop(6).Width(170).Height(43).Image(png);
                            }

                            column.Item().Text(lines.BarcodePayload).AlignCenter();
                        });
                });
            })
            .GeneratePdf(absolutePath);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Pure content mapping (no PDF dependency): every line the slip renders,
    /// in order. Unit-testable proof that order data flows into the document
    /// verbatim, including the empty-order notice.
    /// </summary>
    public static LabOrderTextLines BuildTextLines(LabOrderDto order, LabPrintTextDto labText)
    {
        ArgumentNullException.ThrowIfNull(order);
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

        var patient = new List<string>
        {
            $"المريض: {order.FullName}",
            string.IsNullOrWhiteSpace(order.LabId)
                ? $"رقم المريض: {order.PatientId.ToString(CultureInfo.InvariantCulture)}"
                : $"رقم المعمل: {order.LabId}",
            $"التاريخ: {order.DateUtc.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)}"
        };

        var items = order.Lines
            .Select(l => new LabOrderItemLine(l.TestCode, l.TestName))
            .ToList();

        string? emptyNotice = items.Count == 0 ? "لا تحاليل مطلوبة" : null;

        return new LabOrderTextLines(header, patient, items, order.Identifier, emptyNotice);
    }

    public sealed record LabOrderItemLine(string TestCode, string TestName);

    public sealed record LabOrderTextLines(
        IReadOnlyList<string> Header,
        IReadOnlyList<string> Patient,
        IReadOnlyList<LabOrderItemLine> Items,
        string BarcodePayload,
        string? EmptyNotice);
}
