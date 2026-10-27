using System.Globalization;
using QuestPDF;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Features.Statistics.Common;
using TopLab.Application.Features.SystemAndPrintSettings.Common;

namespace TopLab.Infrastructure.Printing;

public sealed class StatisticsReportPdfWriter : IStatisticsReportPdfWriter
{
    static StatisticsReportPdfWriter()
    {
        Settings.License = LicenseType.Community;
        Settings.UseSystemFonts = true;
    }

    public Task WritePdfAsync(
        string absolutePath,
        StatisticsReportModel report,
        LabPrintTextDto labText,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(absolutePath);
        ArgumentNullException.ThrowIfNull(report);
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

#pragma warning disable CA1416
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
                    page.Header().Column(column =>
                    {
                        if (!string.IsNullOrWhiteSpace(labText.LabName)) column.Item().Text(labText.LabName).SemiBold().FontSize(fontSize + 2).AlignRight();
                        if (!string.IsNullOrWhiteSpace(labText.Address)) column.Item().Text(labText.Address).AlignRight();
                        if (!string.IsNullOrWhiteSpace(labText.Phone)) column.Item().Text(labText.Phone).AlignRight();
                    });

                    page.Content().Column(column =>
                    {
                        column.Item().PaddingTop(6).Text("تقرير الإحصاء").SemiBold().FontSize(fontSize + 4).AlignRight();
                        column.Item().Text($"الفترة: {report.From.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture)} إلى {report.To.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture)}").AlignRight();
                        column.Item().Text($"الإجمالي: {report.TotalCount.ToString(CultureInfo.InvariantCulture)}").SemiBold().AlignRight();

                        foreach (var section in report.Sections)
                        {
                            column.Item().PaddingTop(8).Text(section.Title).SemiBold().AlignRight();
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(4);
                                    columns.ConstantColumn(60);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Text("التصنيف").SemiBold().AlignRight();
                                    header.Cell().Text("العدد").SemiBold().AlignRight();
                                });

                                foreach (var row in section.Rows)
                                {
                                    table.Cell().Text(row.Label).AlignRight();
                                    table.Cell().Text(row.Count.ToString(CultureInfo.InvariantCulture)).AlignRight();
                                }
                            });
                        }

                        if (report.Money is not null)
                        {
                            column.Item().PaddingTop(6).Text($"المدفوعات: {report.Money.AmountsPaid.ToString(CultureInfo.InvariantCulture)}").SemiBold().AlignRight();
                        }
                    });

                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.CurrentPageNumber();
                        text.Span(" / ");
                        text.TotalPages();
                    });
                });
            })
            .GeneratePdf(absolutePath);

        return Task.CompletedTask;
    }
}