using System.Globalization;
using QuestPDF;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Features.ResultDelivery.Common;
using TopLab.Application.Features.SystemAndPrintSettings.Common;
using TopLab.Infrastructure.Barcode;

namespace TopLab.Infrastructure.Printing;

/// <summary>
/// Group-patient report slip renderer using QuestPDF (Phase 6, GAP-013).
/// One independently archivable slip per selected patient, one page per patient,
/// in a single PDF.
///
/// Follows the <see cref="LabOrderPdfWriter"/> slip pattern (A5 portrait, RTL
/// throughout, lab header from <see cref="LabPrintTextDto"/>, font via
/// <see cref="ArabicFontResolver"/>, never overwrites) and reuses the shared
/// identifier pipeline (<see cref="BarcodeLabelRenderer"/> +
/// <see cref="BarcodePngEncoder"/>) and the shared identifier rule
/// (<c>BarcodePayload</c>) rather than re-implementing either.
///
/// Archive safety: each patient gets its own <c>container.Page</c> with its own
/// lab header and its own patient block, so a page is self-describing and no
/// content can bleed between patients. A slip with no ordered tests still prints
/// its identity block plus a no-tests notice (the lab-order/worksheet
/// empty-state idiom), and an empty selection prints one explanatory page
/// instead of a malformed zero-page document.
/// </summary>
public sealed class GroupPatientSlipPdfWriter : IGroupPatientSlipPdfWriter
{
    private const string Title = "قصاصة تقرير مريض";
    private const string NoTestsNotice = "لا تحاليل مطلوبة";
    private const string NoPatientsNotice = "لا يوجد مرضى محددون للطباعة";

    private readonly BarcodeLabelRenderer _renderer;

    static GroupPatientSlipPdfWriter()
    {
        // P-01 owner ruling (binding for the slip writers): this writer sets the
        // licence in its OWN static constructor and must not rely on another
        // writer running first.
        Settings.License = LicenseType.Community;

        // Arabic shaping needs a system font with Arabic glyphs (e.g. Arial on
        // Windows); QuestPDF 2026+ ships only Lato embedded and disables OS
        // font lookup by default.
        Settings.UseSystemFonts = true;
    }

    public GroupPatientSlipPdfWriter(BarcodeLabelRenderer renderer)
    {
        _renderer = renderer;
    }

    public Task WritePdfAsync(
        string absolutePath,
        GroupPatientSlipBatchDto batch,
        LabPrintTextDto labText,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(absolutePath);
        ArgumentNullException.ThrowIfNull(batch);
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

        var pages = BuildTextPages(batch, labText);
#pragma warning disable CA1416 // Windows-only WPF app
        var fontFamily = ArabicFontResolver.Resolve(labText.FontFamily);
#pragma warning restore CA1416
        var fontSize = labText.FontSizePt > 0 ? labText.FontSizePt : 12;

        Document
            .Create(document =>
            {
                // One page per slip: the pagination convention that guarantees the
                // strongest separation (no shared flow that content could bleed
                // across) and lets every page carry its own lab + patient identity.
                foreach (var slip in pages)
                {
                    // Barcode image via the shared pipeline; on failure the slip
                    // falls back to the human-readable identifier (decision 66-A idiom).
                    byte[]? barcodePng = null;
                    if (!string.IsNullOrWhiteSpace(slip.Identifier))
                    {
                        try
                        {
                            var label = _renderer.Render(slip.Identifier);
                            barcodePng = BarcodePngEncoder.Encode(label.Pixels, label.Width, label.Height);
                        }
                        catch
                        {
                            barcodePng = null;
                        }
                    }

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
                                foreach (var line in slip.Header)
                                {
                                    column.Item().Text(line).SemiBold().FontSize(fontSize + 2).AlignRight();
                                }

                                column.Item().PaddingVertical(4).Text(slip.Title).SemiBold().FontSize(fontSize + 6).AlignCenter();
                                column.Item().PaddingBottom(4).LineHorizontal(1);

                                foreach (var line in slip.Patient)
                                {
                                    column.Item().Text(line).AlignRight();
                                }

                                if (slip.Rows.Count > 0)
                                {
                                    column.Item().PaddingTop(6).Text("التحاليل:").SemiBold().AlignRight();
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

                                        foreach (var row in slip.Rows)
                                        {
                                            table.Cell().Text(row.TestName).AlignRight();
                                            table.Cell().Text(row.TestCode).AlignLeft();
                                        }
                                    });
                                }
                                else if (slip.EmptyNotice is not null)
                                {
                                    column.Item().PaddingTop(6).Text(slip.EmptyNotice).AlignRight();
                                }

                                if (!string.IsNullOrWhiteSpace(slip.Identifier))
                                {
                                    column.Item().PaddingTop(6).LineHorizontal(1);

                                    if (barcodePng is not null)
                                    {
                                        var png = barcodePng;
                                        column.Item().PaddingTop(6).Width(170).Height(43).Image(png);
                                    }

                                    column.Item().Text(slip.Identifier).AlignCenter();
                                }
                            });
                    });
                }
            })
            .GeneratePdf(absolutePath);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Pure content mapping (no PDF dependency): one entry per rendered page, in
    /// page order. Unit-testable proof that each selected patient gets exactly one
    /// self-contained slip — its own lab header, its own identity block and its own
    /// test lines — and that an empty selection yields the safe notice page.
    /// </summary>
    public static IReadOnlyList<GroupPatientSlipTextPage> BuildTextPages(
        GroupPatientSlipBatchDto batch,
        LabPrintTextDto labText)
    {
        ArgumentNullException.ThrowIfNull(batch);
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

        if (batch.Slips.Count == 0)
        {
            return new[]
            {
                new GroupPatientSlipTextPage(
                    header,
                    Title,
                    Array.Empty<string>(),
                    Array.Empty<GroupPatientSlipTextRow>(),
                    NoPatientsNotice,
                    string.Empty)
            };
        }

        return batch.Slips
            .Select(slip => new GroupPatientSlipTextPage(
                header,
                Title,
                new List<string>
                {
                    $"المريض: {slip.FullName}",
                    string.IsNullOrWhiteSpace(slip.LabId)
                        ? $"رقم المريض: {slip.PatientId.ToString(CultureInfo.InvariantCulture)}"
                        : $"رقم المعمل: {slip.LabId}",
                    $"التاريخ: {slip.RegistrationDateUtc.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)}"
                },
                slip.Lines
                    .Select(l => new GroupPatientSlipTextRow(l.TestCode, l.TestName))
                    .ToList(),
                slip.Lines.Count == 0 ? NoTestsNotice : null,
                slip.Identifier))
            .ToList();
    }

    public sealed record GroupPatientSlipTextRow(string TestCode, string TestName);

    public sealed record GroupPatientSlipTextPage(
        IReadOnlyList<string> Header,
        string Title,
        IReadOnlyList<string> Patient,
        IReadOnlyList<GroupPatientSlipTextRow> Rows,
        string? EmptyNotice,
        string Identifier);
}
