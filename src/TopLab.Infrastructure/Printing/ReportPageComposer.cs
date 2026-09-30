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
    public static void ComposeHeader(IContainer container, ReportDocumentContent content, float fontSize)
    {
        if (!content.DrawHeader)
        {
            return;
        }

        container.Column(column =>
        {
            if (!string.IsNullOrWhiteSpace(content.LabName))
            {
                column.Item().Text(content.LabName!).SemiBold().FontSize(fontSize + 2).AlignCenter();
            }

            if (!string.IsNullOrWhiteSpace(content.SoftwareBar))
            {
                column.Item().PaddingTop(2).Text(content.SoftwareBar!).FontSize(fontSize - 1).AlignCenter().FontColor(Colors.Grey.Medium);
            }

            var contact = string.Join(" · ", new[] { content.LabAddress, content.LabPhone }
                .Where(s => !string.IsNullOrWhiteSpace(s)));
            if (contact.Length > 0)
            {
                column.Item().PaddingTop(2).Text(contact).FontSize(fontSize - 1).AlignCenter();
            }

            column.Item().PaddingTop(4).LineHorizontal(1);
        });
    }

    public static void ComposeBody(IContainer container, ReportDocumentContent content, float fontSize, string fontFamily)
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
                    column.Item().PaddingTop(8).Text(section.Heading!).SemiBold().FontSize(fontSize + 1).AlignRight();
                }

                foreach (var line in section.DisplayLines)
                {
                    column.Item().Text(line).FontSize(fontSize).AlignRight();
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

    public static void ComposeFooter(IContainer container, ReportDocumentContent content, float fontSize)
    {
        if (!content.DrawFooter)
        {
            container.AlignCenter().Text(text =>
            {
                text.Span("صفحة ").FontSize(fontSize - 1);
                text.CurrentPageNumber().FontSize(fontSize - 1);
                text.Span(" من ").FontSize(fontSize - 1);
                text.TotalPages().FontSize(fontSize - 1);
            });
            return;
        }

        container.Column(column =>
        {
            column.Item().LineHorizontal(1);
            if (!string.IsNullOrWhiteSpace(content.ReportNumberText))
            {
                column.Item().PaddingTop(2).Text(content.ReportNumberText!).FontSize(fontSize - 1).AlignRight();
            }

            if (!string.IsNullOrWhiteSpace(content.ReportDateText))
            {
                column.Item().Text(content.ReportDateText!).FontSize(fontSize - 1).AlignRight();
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
