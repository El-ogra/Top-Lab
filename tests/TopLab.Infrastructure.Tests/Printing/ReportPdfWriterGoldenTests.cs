using System.Text;
using System.Text.Json;
using TopLab.Application.Features.ReportProduction.Common;
using TopLab.Application.Features.ResultsEntry.Common;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Settings;
using TopLab.Infrastructure.Printing;
using Xunit;

namespace TopLab.Infrastructure.Tests.Printing;

/// <summary>
/// WP-01 golden tests for the QuestPDF report writer (names mandated by the plan).
/// Content assertions use the pure display-line model (Arabic preserved); PDF
/// bytes are checked for validity, page geometry, and pagination.
/// </summary>
public class ReportPdfWriterGoldenTests
{
    private static readonly ReportPdfWriter Writer = new();

    private static ReportSettings A5Settings()
    {
        var s = ReportSettings.CreateDefault();
        s.SetPaperSize(PaperSize.A5);
        return s;
    }

    private static CombinedReportDto ArabicCombined()
    {
        return new CombinedReportDto(
            7,
            "علي بن محمد الأحمد",
            "LAB-1",
            new List<CombinedReportLineDto>
            {
                new(101, 2, "سكر الدم", "GLU", 0, "5.5", 0, "4 - 6",
                    new List<ProfileReportLineDto>(), null)
            });
    }

    private static ReportPrintEnvelope CombinedEnvelope(CombinedReportDto dto)
        => new(ReportPrintEnvelope.Combined, JsonSerializer.Serialize(dto));

    private static string TempPath()
        => Path.Combine(Path.GetTempPath(), $"toplab-golden-{Guid.NewGuid():N}.pdf");

    private static async Task<string> WriteCombinedAsync(
        CombinedReportDto dto,
        ReportSettings? reportSettings = null,
        SystemSettings? systemSettings = null)
    {
        var path = TempPath();
        await Writer.WritePdfAsync(
            path,
            CombinedEnvelope(dto),
            reportSettings ?? ReportSettings.CreateDefault(),
            systemSettings ?? SystemSettings.CreateDefault());
        return path;
    }

    private static IReadOnlyList<string> DisplayLines(CombinedReportDto dto, ReportSettings settings, SystemSettings system)
    {
        var content = ReportContentBuilder.FromEnvelope(
            CombinedEnvelope(dto), settings, system);
        return content.BuildDisplayLines();
    }

    private static string PdfLatin(string path)
        => Encoding.Latin1.GetString(File.ReadAllBytes(path));

    [Fact]
    public void ReportPdfWriter_ContainsArabicPatientName()
    {
        var lines = DisplayLines(ArabicCombined(), ReportSettings.CreateDefault(), SystemSettings.CreateDefault());
        Assert.Contains(lines, l => l.Contains("علي بن محمد الأحمد", StringComparison.Ordinal));
        Assert.Contains(lines, l => l.Contains("اسم المريض", StringComparison.Ordinal));
    }

    [Fact]
    public void ReportPdfWriter_ContainsArabicTestName()
    {
        var lines = DisplayLines(ArabicCombined(), ReportSettings.CreateDefault(), SystemSettings.CreateDefault());
        Assert.Contains(lines, l => l.Contains("سكر الدم", StringComparison.Ordinal));
    }

    [Fact]
    public async Task ReportPdfWriter_HonoursPaperSizeA5()
    {
        var path = await WriteCombinedAsync(ArabicCombined(), A5Settings());
        try
        {
            var bytes = File.ReadAllBytes(path);
            Assert.Equal("%PDF", Encoding.ASCII.GetString(bytes, 0, 4));
            var latin = PdfLatin(path);
            // A5 portrait = 420×595 pt
            Assert.Contains("420", latin);
            Assert.Contains("595", latin);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task ReportPdfWriter_HonoursPaperSizeA4()
    {
        var path = await WriteCombinedAsync(ArabicCombined(), ReportSettings.CreateDefault());
        try
        {
            var latin = PdfLatin(path);
            // A4 portrait = 595×842 pt
            Assert.Contains("595", latin);
            Assert.Contains("842", latin);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task ReportPdfWriter_AppliesLeftAndBottomMargins()
    {
        var settings = ReportSettings.CreateDefault();
        settings.SetMargins(3m, 2m);
        var path = await WriteCombinedAsync(ArabicCombined(), settings);
        try
        {
            Assert.True(new FileInfo(path).Length > 200);
            var content = ReportContentBuilder.FromEnvelope(
                CombinedEnvelope(ArabicCombined()), settings, SystemSettings.CreateDefault());
            Assert.False(string.IsNullOrEmpty(content.ReportTitle));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task ReportPdfWriter_AppliesTopSpaceSetting()
    {
        var settings = ReportSettings.CreateDefault();
        settings.SetTopSpace(5m);
        var path = await WriteCombinedAsync(ArabicCombined(), settings);
        try
        {
            Assert.True(File.Exists(path));
            // Geometry helper is the single source of truth for cm→pt.
            Assert.Equal((float)(5m * 28.3465m), PageSizeMapper.CmToPoints(5m), 2);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task ReportPdfWriter_DrawsHeaderWhenModeIsWords()
    {
        var settings = ReportSettings.CreateDefault();
        settings.SetHeaderFooterMode(HeaderFooterMode.Words);
        var content = ReportContentBuilder.FromEnvelope(
            CombinedEnvelope(ArabicCombined()), settings, SystemSettings.CreateDefault(),
            labName: "مختبر الشفاء");
        Assert.True(content.DrawHeader);
        Assert.Contains(content.BuildDisplayLines(), l => l.Contains("مختبر الشفاء", StringComparison.Ordinal));

        var path = await WriteCombinedAsync(ArabicCombined(), settings);
        try
        {
            Assert.True(File.Exists(path));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task ReportPdfWriter_DrawsFooterWhenModeIsWords()
    {
        var settings = ReportSettings.CreateDefault();
        settings.SetHeaderFooterMode(HeaderFooterMode.Words);
        var content = ReportContentBuilder.FromEnvelope(
            CombinedEnvelope(ArabicCombined()), settings, SystemSettings.CreateDefault());
        Assert.True(content.DrawFooter);

        var path = await WriteCombinedAsync(ArabicCombined(), settings);
        try
        {
            Assert.True(File.Exists(path));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task ReportPdfWriter_DrawsDoctorSignatureWhenEnabled()
    {
        var settings = ReportSettings.CreateDefault();
        settings.SetDoctorSignature(true);
        var content = ReportContentBuilder.FromEnvelope(
            CombinedEnvelope(ArabicCombined()), settings, SystemSettings.CreateDefault());
        Assert.Contains("توقيع الطبيب", content.BuildDisplayLines());

        var path = await WriteCombinedAsync(ArabicCombined(), settings);
        try
        {
            Assert.True(File.Exists(path));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task ReportPdfWriter_PaginatesContentBeyondOnePage()
    {
        var lines = new List<CombinedReportLineDto>();
        for (var i = 0; i < 80; i++)
        {
            lines.Add(new CombinedReportLineDto(
                1000 + i, 2, $"تحليل رقم {i}", $"T{i}", 0, "5.5", 0, "4 - 6",
                new List<ProfileReportLineDto>(), null));
        }

        var dto = new CombinedReportDto(7, "مريض طويل", "LAB-1", lines);
        var path = await WriteCombinedAsync(dto);
        try
        {
            var latin = PdfLatin(path);
            var pageCount = CountPages(latin);
            Assert.True(pageCount >= 2, $"Expected multi-page report, found {pageCount} page(s).");
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void ReportPdfWriter_RendersCultureSectionWhenPresent()
    {
        var dto = new CombinedReportDto(
            7, "علي", "LAB-1",
            new List<CombinedReportLineDto>
            {
                new(101, 2, "مزرعة", "CUL", 0, null, null, null,
                    new List<ProfileReportLineDto>(),
                    new CultureReportSummaryDto("دم", "Staphylococcus", null, null, "هوائي", "10^4"))
            });

        var content = ReportContentBuilder.FromEnvelope(
            CombinedEnvelope(dto), ReportSettings.CreateDefault(), SystemSettings.CreateDefault());
        var all = string.Join('\n', content.BuildDisplayLines());
        Assert.Contains("العينة: دم", all, StringComparison.Ordinal);
        Assert.Contains("Staphylococcus", all, StringComparison.Ordinal);
    }

    [Fact]
    public void ReportPdfWriter_ToleratesMissingCultureFields()
    {
        var dto = new CombinedReportDto(
            7, "علي", "LAB-1",
            new List<CombinedReportLineDto>
            {
                new(101, 2, "مزرعة", "CUL", 0, null, null, null,
                    new List<ProfileReportLineDto>(),
                    new CultureReportSummaryDto(null, null, null, null, null, null))
            });

        var content = ReportContentBuilder.FromEnvelope(
            CombinedEnvelope(dto), ReportSettings.CreateDefault(), SystemSettings.CreateDefault());
        Assert.False(string.IsNullOrEmpty(content.PatientFullName));
    }

    [Fact]
    public void ReportPdfWriter_HistorySection_PrintsDates()
    {
        var history = new PatientHistoryDto(
            7, "علي", "LAB-1", "ByLabCode", true,
            new List<HistoryEntryDto>
            {
                new(101, 7, 2, "سكر الدم", "GLU", 0, "5.5", 0, true,
                    new DateTime(2026, 5, 1, 10, 0, 0, DateTimeKind.Utc), null)
            });

        var envelope = new ReportPrintEnvelope(ReportPrintEnvelope.History, JsonSerializer.Serialize(history));
        var content = ReportContentBuilder.FromEnvelope(
            envelope, ReportSettings.CreateDefault(), SystemSettings.CreateDefault());
        var all = string.Join('\n', content.BuildDisplayLines());
        Assert.Contains("2026/05/01", all, StringComparison.Ordinal);
        Assert.Contains("سكر الدم", all, StringComparison.Ordinal);
    }

    [Fact]
    public void ReportPdfWriter_BlankReport_RendersPatientBlock()
    {
        var blank = new BlankReportDto(7, "علي بن محمد", "LAB-1", "ذكر", 30, "سنة", "د. حسن", "الشفاء");
        var envelope = new ReportPrintEnvelope(ReportPrintEnvelope.Blank, JsonSerializer.Serialize(blank));
        var content = ReportContentBuilder.FromEnvelope(
            envelope, ReportSettings.CreateDefault(), SystemSettings.CreateDefault());
        var lines = content.BuildDisplayLines();
        Assert.Contains(lines, l => l.Contains("علي بن محمد", StringComparison.Ordinal));
        Assert.Contains(lines, l => l.Contains("ذكر", StringComparison.Ordinal));
        Assert.Contains(lines, l => l.Contains("د. حسن", StringComparison.Ordinal));
        Assert.Contains(lines, l => l.Contains("الشفاء", StringComparison.Ordinal));
    }

    [Fact]
    public void Printing_ContainsNoToAsciiHelper()
    {
        var printingDir = Path.Combine(FindRepoRoot(), "src", "TopLab.Infrastructure", "Printing");
        Assert.True(Directory.Exists(printingDir), $"Missing directory: {printingDir}");

        foreach (var file in Directory.EnumerateFiles(printingDir, "*.cs", SearchOption.AllDirectories))
        {
            var text = File.ReadAllText(file);
            Assert.DoesNotContain("ToAscii", text, StringComparison.Ordinal);
            Assert.DoesNotContain("BuildMinimalPdf", text, StringComparison.Ordinal);
        }

        var exporter = Path.Combine(
            FindRepoRoot(), "src", "TopLab.Infrastructure", "Services", "PatientReportPdfExporter.cs");
        Assert.False(File.ReadAllText(exporter).Contains("ToAscii", StringComparison.Ordinal));
        Assert.False(File.ReadAllText(exporter).Contains("BuildMinimalPdf", StringComparison.Ordinal));
    }

    [Fact]
    public void ReportPdf_ContainsNoEnglishLabels()
    {
        var dto = ArabicCombined();
        var content = ReportContentBuilder.FromEnvelope(
            CombinedEnvelope(dto), ReportSettings.CreateDefault(), SystemSettings.CreateDefault());
        Assert.False(content.ContainsEnglishLabels());

        var blank = new BlankReportDto(7, "علي", "LAB-1", "ذكر", 30, "سنة", "د. حسن", "الشفاء");
        var blankContent = ReportContentBuilder.FromEnvelope(
            new ReportPrintEnvelope(ReportPrintEnvelope.Blank, JsonSerializer.Serialize(blank)),
            ReportSettings.CreateDefault(), SystemSettings.CreateDefault());
        Assert.False(blankContent.ContainsEnglishLabels());
    }

    [Fact]
    public async Task ReportPdfExporter_ContainsArabicPatientName()
    {
        var data = new PatientReportPdfData(
            7,
            "علي بن محمد الأحمد",
            "LAB-1",
            new[]
            {
                new PatientReportPdfLine(
                    101, "سكر الدم", "GLU", "5.5", 0, null,
                    new FrozenRangeDto(10, null, "Year", 0, 100, 4m, 10m, null, null, DateTimeOffset.UtcNow),
                    Array.Empty<string>(),
                    null)
            });

        var content = ReportContentBuilder.FromPatientExport(
            data, ReportSettings.CreateDefault(), SystemSettings.CreateDefault());
        var all = string.Join('\n', content.BuildDisplayLines());
        Assert.Contains("علي بن محمد الأحمد", all, StringComparison.Ordinal);
        Assert.Contains("سكر الدم", all, StringComparison.Ordinal);

        var path = TempPath();
        try
        {
            await new TopLab.Infrastructure.Services.PatientReportPdfExporter().ExportAsync(path, data);
            Assert.True(File.Exists(path));
            Assert.Equal("%PDF", Encoding.ASCII.GetString(File.ReadAllBytes(path), 0, 4));
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    [Fact]
    public void ReportPdf_RendersLowComment_WhenFlagIsLow()
    {
        var dto = new CombinedReportDto(
            7, "علي", "LAB-1",
            new List<CombinedReportLineDto>
            {
                new(101, 2, "سكر الدم", "GLU", 0, "1.0", (int)ResultFlag.Low, "4 - 6",
                    new List<ProfileReportLineDto>(), null,
                    LowComment: "منخفض عن الحد الطبيعي", HighComment: "مرتفع عن الحد الطبيعي")
            });

        var content = ReportContentBuilder.FromEnvelope(
            CombinedEnvelope(dto), ReportSettings.CreateDefault(), SystemSettings.CreateDefault());
        var all = string.Join('\n', content.BuildDisplayLines());
        Assert.Contains("منخفض عن الحد الطبيعي", all, StringComparison.Ordinal);
    }

    [Fact]
    public void ReportPdf_OmitsComment_WhenFlagIsNormal()
    {
        var dto = new CombinedReportDto(
            7, "علي", "LAB-1",
            new List<CombinedReportLineDto>
            {
                new(101, 2, "سكر الدم", "GLU", 0, "5.0", (int)ResultFlag.Normal, "4 - 6",
                    new List<ProfileReportLineDto>(), null,
                    LowComment: "منخفض", HighComment: "مرتفع")
            });

        var content = ReportContentBuilder.FromEnvelope(
            CombinedEnvelope(dto), ReportSettings.CreateDefault(), SystemSettings.CreateDefault());
        var all = string.Join('\n', content.BuildDisplayLines());
        Assert.DoesNotContain("منخفض", all, StringComparison.Ordinal);
        Assert.DoesNotContain("مرتفع", all, StringComparison.Ordinal);
    }

    private static int CountPages(string latinPdf)
    {
        var count = 0;
        var idx = 0;
        while ((idx = latinPdf.IndexOf("/Type /Page", idx, StringComparison.Ordinal)) >= 0)
        {
            // Skip /Type /Pages
            if (!latinPdf.AsSpan(idx).StartsWith("/Type /Pages", StringComparison.Ordinal))
            {
                count++;
            }

            idx += 10;
        }

        return count;
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "TopLab.sln")))
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        throw new InvalidOperationException("Could not locate TopLab.sln from test base directory.");
    }
}
