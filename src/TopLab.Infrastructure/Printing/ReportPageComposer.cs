using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TopLab.Domain.Settings;

namespace TopLab.Infrastructure.Printing;

/// <summary>
/// Builds header / body / footer regions for <see cref="ReportDocument"/>.
/// Geometry and text come exclusively from <see cref="ReportDocumentContent"/>
/// and the settings snapshot — no hard-coded page boxes.
/// </summary>
public static class ReportPageComposer
{
    /// <summary>
    /// GAP-015: the resolved rendering layout of one report item line. Every
    /// member is nullable/zero when the item carries no per-item layout, so a
    /// line whose three layout fields are all null renders exactly as it did
    /// before Phase 8.
    /// </summary>
    /// <param name="Text">
    /// The text to draw: the item content, prefixed with the marker when the
    /// template defines one.
    /// </param>
    /// <param name="LineHeight">
    /// Line-height multiplier. <c>null</c> = the composer's existing default
    /// spacing (QuestPDF's own default is untouched).
    /// </param>
    /// <param name="PaddingLeftPt">
    /// Extra left indent in points, added to the page's left margin
    /// (<see cref="ReportSettings.PageMarginLeftCm"/>). <c>0</c> = the item sits
    /// exactly at the page margin (today's behaviour).
    /// </param>
    /// <param name="LeftPositionCm">
    /// Absolute left position of the item: <c>PageMarginLeftCm + LeftOffsetCm</c>.
    /// <c>null</c> when no settings snapshot is supplied (page margin unchanged).
    /// </param>
    public sealed record ReportItemLayout(
        string Text,
        float? LineHeight,
        float PaddingLeftPt,
        decimal? LeftPositionCm);

    /// <summary>
    /// GAP-015: turns a per-item layout line into the concrete rendering
    /// decisions. Pure and side-effect free so the null semantics are directly
    /// testable:
    /// <list type="bullet">
    /// <item>LineSpacing = null → <see cref="LineHeight"/> stays null: the
    /// composer applies no line-height override at all.</item>
    /// <item>LeftOffsetCm = null → <see cref="PaddingLeftPt"/> is 0, i.e. the
    /// page margin remains the item's left position (never replaced, only
    /// added to).</item>
    /// <item>MarkerSymbol = null → <see cref="Text"/> is the item content
    /// unchanged, so no marker is drawn.</item>
    /// </list>
    /// </summary>
    public static ReportItemLayout ResolveItemLayout(ReportContentLine line, ReportSettings? settings = null)
    {
        ArgumentNullException.ThrowIfNull(line);

        var text = line.MarkerSymbol is null ? line.Text : line.MarkerSymbol + line.Text;
        var paddingLeftPt = line.LeftOffsetCm is { } offsetCm
            ? PageSizeMapper.CmToPoints(offsetCm)
            : 0f;
        decimal? leftPositionCm = settings is null
            ? null
            : settings.PageMarginLeftCm + (line.LeftOffsetCm ?? 0m);

        return new ReportItemLayout(
            text,
            line.LineSpacing is { } lineSpacing ? (float)lineSpacing : null,
            paddingLeftPt,
            leftPositionCm);
    }

    public static void ComposeHeader(IContainer container, ReportDocumentContent content, float fontSize, ReportSettings settings)
    {
        if (!content.DrawHeader)
        {
            return;
        }

        var headerColor = ParseColor(settings.HeaderColor);

        container.Column(column =>
        {
            if (!string.IsNullOrWhiteSpace(content.HeaderImagePath) && File.Exists(content.HeaderImagePath))
            {
                // GAP-011: bound the logo so an over-tall image cannot exhaust the
                // page and abort PDF generation; wide logos are unaffected.
                var (imageWidth, imageMaxHeight) = PageSizeMapper.FitHeaderFooterImage(
                    settings.PaperSize,
                    settings.PageMarginLeftCm,
                    ContentHeightPt(settings) * PageSizeMapper.HeaderFooterImageMaxHeightFraction);
                column.Item().PaddingBottom(4).MaxWidth(imageWidth).MaxHeight(imageMaxHeight)
                    .Image(content.HeaderImagePath);
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(content.LabName))
                {
                    column.Item().Text(content.LabName!).SemiBold().FontSize(fontSize + 2).AlignCenter()
                        .FontColor(headerColor);
                }

                if (!string.IsNullOrWhiteSpace(content.SoftwareBar))
                {
                    column.Item().PaddingTop(2).Text(content.SoftwareBar!).FontSize(fontSize - 1).AlignCenter()
                        .FontColor(Colors.Grey.Medium);
                }

                var contact = string.Join(" · ", new[] { content.LabAddress, content.LabPhone }
                    .Where(s => !string.IsNullOrWhiteSpace(s)));
                if (contact.Length > 0)
                {
                    column.Item().PaddingTop(2).Text(contact).FontSize(fontSize - 1).AlignCenter();
                }
            }

            column.Item().PaddingTop(4).LineHorizontal(1);
        });
    }

    /// <summary>
    /// Body region. <paramref name="settings"/> is optional so existing callers
    /// keep working; it is only read to resolve the GAP-015 left position.
    /// </summary>
    public static void ComposeBody(IContainer container, ReportDocumentContent content, float fontSize, string fontFamily, ReportSettings? settings = null)
    {
        container.Column(column =>
        {
            column.Item().Text(content.ReportTitle).SemiBold().FontSize(fontSize + 3).AlignCenter();
            column.Item().PaddingTop(6).Text(content.PreferLabId && !string.IsNullOrWhiteSpace(content.LabId)
                    ? $"رقم الملف: {content.LabId}"
                    : $"الرقم: {content.PatientId}")
                .AlignRight();
            column.Item().Text($"اسم المريض: {content.PatientFullName}").AlignRight();

            if (!string.IsNullOrWhiteSpace(content.Sex))
            {
                column.Item().Text($"الجنس: {content.Sex}").AlignRight();
            }

            if (!string.IsNullOrWhiteSpace(content.AgeText))
            {
                column.Item().Text($"العمر: {content.AgeText}").AlignRight();
            }

            if (!string.IsNullOrWhiteSpace(content.TreatingDoctorName))
            {
                column.Item().Text($"الطبيب المعالج: {content.TreatingDoctorName}").AlignRight();
            }

            if (!string.IsNullOrWhiteSpace(content.ReferralEntityName))
            {
                column.Item().Text($"جهة الإحالة: {content.ReferralEntityName}").AlignRight();
            }

            column.Item().PaddingTop(4).LineHorizontal(1);

            foreach (var section in content.Sections)
            {
                if (!string.IsNullOrWhiteSpace(section.Heading))
                {
                    ComposeItemLine(
                        column.Item().PaddingTop(8),
                        section.HeadingLine ?? ReportContentLine.Default(section.Heading!),
                        fontSize + 1,
                        settings,
                        semiBold: true);
                }

                foreach (var line in section.LayoutLines)
                {
                    ComposeItemLine(column.Item(), line, fontSize, settings);
                }

                if (section.Grid is { } grid && grid.Rows.Count > 0)
                {
                    column.Item().PaddingTop(4).Table(table =>
                    {
                        var colCount = Math.Max(grid.Headers.Count, grid.Rows.Max(r => r.Count));
                        table.ColumnsDefinition(columns =>
                        {
                            for (var i = 0; i < colCount; i++)
                            {
                                columns.RelativeColumn();
                            }
                        });

                        table.Header(header =>
                        {
                            foreach (var title in grid.Headers)
                            {
                                header.Cell().Text(title).SemiBold().FontSize(fontSize - 1).AlignRight();
                            }
                        });

                        foreach (var row in grid.Rows)
                        {
                            for (var i = 0; i < colCount; i++)
                            {
                                var cell = i < row.Count ? row[i] : string.Empty;
                                table.Cell().Text(cell).FontSize(fontSize - 1).AlignRight();
                            }
                        }
                    });
                }
            }

            if (content.DoctorSignatureEnabled)
            {
                column.Item().PaddingTop(18).AlignLeft().Text("توقيع الطبيب").FontSize(fontSize);
                column.Item().PaddingTop(4).Width(120).LineHorizontal(1);
            }
        });
    }

    public static void ComposeFooter(IContainer container, ReportDocumentContent content, float fontSize, ReportSettings settings)
    {
        var footerColor = ParseColor(settings.FooterColor);

        if (!content.DrawFooter)
        {
            container.AlignCenter().Text(text =>
            {
                text.Span("صفحة ").FontSize(fontSize - 1).FontColor(footerColor);
                text.CurrentPageNumber().FontSize(fontSize - 1);
                text.Span(" من ").FontSize(fontSize - 1);
                text.TotalPages().FontSize(fontSize - 1);
            });
            return;
        }

        container.Column(column =>
        {
            column.Item().LineHorizontal(1);

            if (!string.IsNullOrWhiteSpace(content.FooterImagePath) && File.Exists(content.FooterImagePath))
            {
                // GAP-011: same bound as the header image (see ComposeHeader).
                var (imageWidth, imageMaxHeight) = PageSizeMapper.FitHeaderFooterImage(
                    settings.PaperSize,
                    settings.PageMarginLeftCm,
                    ContentHeightPt(settings) * PageSizeMapper.HeaderFooterImageMaxHeightFraction);
                column.Item().PaddingTop(4).MaxWidth(imageWidth).MaxHeight(imageMaxHeight)
                    .Image(content.FooterImagePath);
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(content.ReportNumberText))
                {
                    column.Item().PaddingTop(2).Text(content.ReportNumberText!).FontSize(fontSize - 1).AlignRight()
                        .FontColor(footerColor);
                }

                if (!string.IsNullOrWhiteSpace(content.ReportDateText))
                {
                    column.Item().Text(content.ReportDateText!).FontSize(fontSize - 1).AlignRight()
                        .FontColor(footerColor);
                }
            }

            column.Item().AlignCenter().Text(text =>
            {
                text.Span("صفحة ").FontSize(fontSize - 1);
                text.CurrentPageNumber().FontSize(fontSize - 1);
                text.Span(" من ").FontSize(fontSize - 1);
                text.TotalPages().FontSize(fontSize - 1);
            });
        });
    }

    /// <summary>
    /// GAP-015: draws one report item line, applying its per-item layout.
    /// Every override is conditional, so a line with null layout emits exactly
    /// the same elements (padding, line height, text) as before Phase 8.
    /// The left offset uses the same <c>PaddingLeft</c> shape as the envelope's
    /// proven <see cref="EnvelopePrintItemPosition"/> offsets.
    /// </summary>
    private static void ComposeItemLine(
        IContainer container,
        ReportContentLine line,
        float fontSize,
        ReportSettings? settings,
        bool semiBold = false)
    {
        var layout = ResolveItemLayout(line, settings);

        if (layout.PaddingLeftPt > 0f)
        {
            container = container.PaddingLeft(layout.PaddingLeftPt);
        }

        if (!string.IsNullOrWhiteSpace(line.BackgroundColor))
        {
            container = container.Background(ParseColor(line.BackgroundColor));
        }

        // Same element order as before Phase 8; the two style calls are
        // conditional so an unconfigured item emits an identical text element.
        var text = container.Text(layout.Text);
        if (semiBold)
        {
            text.SemiBold();
        }

        if (layout.LineHeight is { } lineHeight)
        {
            text.LineHeight(lineHeight);
        }

        if (!string.IsNullOrWhiteSpace(line.FontColor))
        {
            text.FontColor(ParseColor(line.FontColor));
        }

        text.FontSize(fontSize).AlignRight();
    }

    /// <summary>
    /// Printable content height in points: page height minus the top space and
    /// the bottom margin. Used to bound header/footer images to a fraction of
    /// the page (GAP-011).
    /// </summary>
    private static float ContentHeightPt(ReportSettings settings)
    {
        var (_, pageHeight) = PageSizeMapper.ToPoints(settings.PaperSize);
        return Math.Max(0f,
            pageHeight
            - PageSizeMapper.CmToPoints(settings.ReportTopSpaceCm)
            - PageSizeMapper.CmToPoints(settings.PageMarginBottomCm));
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

    public static void ApplyPageGeometry(PageDescriptor page, ReportSettings settings)
    {
        var (width, height) = PageSizeMapper.ToPoints(settings.PaperSize);
        page.Size(width, height);
        page.MarginTop(PageSizeMapper.CmToPoints(settings.ReportTopSpaceCm));
        page.MarginLeft(PageSizeMapper.CmToPoints(settings.PageMarginLeftCm));
        page.MarginRight(PageSizeMapper.CmToPoints(settings.PageMarginLeftCm));
        page.MarginBottom(PageSizeMapper.CmToPoints(settings.PageMarginBottomCm));
        page.PageColor(Colors.White);
    }
}
