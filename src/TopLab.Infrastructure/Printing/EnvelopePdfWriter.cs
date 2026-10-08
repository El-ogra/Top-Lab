using System.Globalization;
using QuestPDF;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Features.PatientEnvelope.Common;
using TopLab.Application.Features.SystemAndPrintSettings.Common;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Settings;
using TopLab.Infrastructure.Barcode;

namespace TopLab.Infrastructure.Printing;

/// <summary>
/// Patient-envelope document renderer using QuestPDF (Phase 1, REF-066 —
/// shared owner; REF-067 completes the Code-item barcode branch).
/// A5 portrait, RTL throughout (decision 66-B: receipt precedent, not
/// DL-envelope stock — offsets are cm-absolute so the layout ports later
/// without redesign).
/// Per-item cm offsets are honored as relative top deltas with left padding
/// (documented approximation: QuestPDF flows content top-to-bottom, so each
/// enabled item renders below the previous one by its Top-offset delta at its
/// Left offset, in canonical order Name/Code/ReferralEntity/Date).
/// </summary>
public sealed class EnvelopePdfWriter : IEnvelopePdfWriter
{
    private static readonly string[] CanonicalOrder = ["Name", "Code", "ReferralEntity", "Date"];

    private readonly BarcodeLabelRenderer _renderer;

    static EnvelopePdfWriter()
    {
        // Community-eligible reconfirmed 2026-09-29 by owner decision; revisit before commercial distribution.
        Settings.License = LicenseType.Community;

        // Arabic shaping needs a system font with Arabic glyphs (e.g. Arial on
        // Windows); QuestPDF 2026+ ships only Lato embedded and disables OS
        // font lookup by default.
        Settings.UseSystemFonts = true;
    }

    public EnvelopePdfWriter(BarcodeLabelRenderer renderer)
    {
        _renderer = renderer;
    }

    public Task WritePdfAsync(
        string absolutePath,
        EnvelopeDto envelope,
        EnvelopeSettings settings,
        IReadOnlyList<EnvelopePrintItemPosition> positions,
        LabPrintTextDto labText,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(absolutePath);
        ArgumentNullException.ThrowIfNull(envelope);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(positions);
        ArgumentNullException.ThrowIfNull(labText);

        var directory = Path.GetDirectoryName(absolutePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            throw new DirectoryNotFoundException($"Directory not found: {directory}");
        }

        var lines = BuildTextLines(envelope, settings, positions, labText);
#pragma warning disable CA1416 // Windows-only WPF app
        var fontFamily = ArabicFontResolver.Resolve(labText.FontFamily);
#pragma warning restore CA1416
        var fontSize = labText.FontSizePt > 0 ? labText.FontSizePt : 12;
        var topMarginPt = (float)settings.TopMarginCm * 28.35f;

        if (File.Exists(absolutePath))
        {
            throw new IOException($"File already exists: {absolutePath}");
        }

        // Code-item barcode image bytes are wired here (REF-066); scannable-image
        // acceptance is owned by REF-067. If image embedding proves unviable at
        // runtime, the Code item falls back to human-readable text (decision 66-A).
        byte[]? codePng = null;
        if (lines.CodeBarcodePayload is not null)
        {
            try
            {
                var label = _renderer.Render(lines.CodeBarcodePayload);
                codePng = BarcodePngEncoder.Encode(label.Pixels, label.Width, label.Height);
            }
            catch
            {
                codePng = null;
            }
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

                            var previousTopCm = 0m;
                            foreach (var item in lines.Items)
                            {
                                var deltaPt = (float)Math.Max(0m, item.TopOffsetCm - previousTopCm) * 28.35f;
                                var leftPt = (float)item.LeftOffsetCm * 28.35f;
                                previousTopCm = item.TopOffsetCm;

                                if (item.ItemName == "Code" && codePng is not null)
                                {
                                    var png = codePng;
                                    column.Item().PaddingTop(deltaPt).PaddingLeft(leftPt).Column(code =>
                                    {
                                        // ~6 x 1.5 cm box at the Code offsets, aspect
                                        // preserved (decision 67-A shape, wired here).
                                        code.Item().Width(170).Height(43).Image(png);
                                        code.Item().Text(item.Text).AlignRight();
                                    });
                                }
                                else
                                {
                                    column.Item().PaddingTop(deltaPt).PaddingLeft(leftPt).Text(item.Text).AlignRight();
                                }
                            }
                        });
                });
            })
            .GeneratePdf(absolutePath);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Pure content mapping (no PDF dependency): header words plus every enabled
    /// positioned item in canonical order, in order. Unit-testable proof that
    /// envelope data flows into the document verbatim. The Code item carries
    /// <c>CodeBarcodePayload</c> (null when Code is disabled) so mapping tests
    /// assert payload logic PDF-free.
    /// </summary>
    public static EnvelopeTextLines BuildTextLines(
        EnvelopeDto envelope,
        EnvelopeSettings settings,
        IReadOnlyList<EnvelopePrintItemPosition> positions,
        LabPrintTextDto labText)
    {
        ArgumentNullException.ThrowIfNull(envelope);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(positions);
        ArgumentNullException.ThrowIfNull(labText);

        var header = new List<string>();
        // HeaderFooterMode.Images has no image-asset pipeline in this slice, so it
        // falls back to the words header (documented; matches the receipt precedent).
        if (settings.HeaderFooterMode != HeaderFooterMode.None)
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

        var byName = positions.ToDictionary(p => p.ItemName, p => p);
        var items = new List<EnvelopeItemLine>();
        string? codeBarcodePayload = null;

        foreach (var name in CanonicalOrder)
        {
            if (!byName.TryGetValue(name, out var position) || !position.IsEnabled)
            {
                continue;
            }

            string? value = name switch
            {
                "Name" => envelope.FullName,
                "Code" => envelope.Identifier,
                "ReferralEntity" => envelope.ReferralDisplayName,
                "Date" => envelope.DateUtc.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
                _ => null
            };

            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            if (name == "Code")
            {
                codeBarcodePayload = envelope.Identifier;
            }

            string caption = name switch
            {
                "Name" => "المريض",
                "Code" => "الكود",
                "ReferralEntity" => "الجهة المحولة",
                "Date" => "التاريخ",
                _ => name
            };

            var text = settings.SuppressCaptions
                ? value
                : $"{caption}: {value}";

            items.Add(new EnvelopeItemLine(name, text, position.LeftOffsetCm, position.TopOffsetCm));
        }

        return new EnvelopeTextLines(header, items, codeBarcodePayload);
    }

    public sealed record EnvelopeItemLine(
        string ItemName,
        string Text,
        decimal LeftOffsetCm,
        decimal TopOffsetCm);

    public sealed record EnvelopeTextLines(
        IReadOnlyList<string> Header,
        IReadOnlyList<EnvelopeItemLine> Items,
        string? CodeBarcodePayload);
}
