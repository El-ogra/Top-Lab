using System.Text;
using System.Text.Json;
using TopLab.Application.Features.ReportProduction.Common;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Settings;
using TopLab.Infrastructure.Printing;
using Xunit;

namespace TopLab.Infrastructure.Tests.Printing;

public class ReportPdfWriterTests
{
    private static readonly IReportPdfWriter Writer = new ReportPdfWriter();

    private static ReportPrintEnvelope CombinedEnvelope()
    {
        var dto = new CombinedReportDto(
            7,
            "Ali",
            "LAB-1",
            new List<CombinedReportLineDto>
            {
                new(101, 2, "Glucose", "GLU", 0, "5.5", 0, "4-6", new List<ProfileReportLineDto>(), null)
            });

        return new ReportPrintEnvelope(ReportPrintEnvelope.Combined, JsonSerializer.Serialize(dto));
    }

    private static ReportPrintEnvelope BlankEnvelope()
    {
        var dto = new BlankReportDto(7, "Ali", "LAB-1", "Male", 30, "Year", "Dr.Hassan", "Al-Shifa");
        return new ReportPrintEnvelope(ReportPrintEnvelope.Blank, JsonSerializer.Serialize(dto));
    }

    private static ReportPrintEnvelope HistoryEnvelope()
    {
        var dto = new PatientHistoryDto(
            7,
            "Ali",
            "LAB-1",
            "ByLabCode",
            true,
            new List<HistoryEntryDto>
            {
                new(101, 7, 2, "Glucose", "GLU", 0, "5.5", 0, true, DateTime.UtcNow, DateTime.UtcNow)
            });

        return new ReportPrintEnvelope(ReportPrintEnvelope.History, JsonSerializer.Serialize(dto));
    }

    private static string TempPath()
    {
        return Path.Combine(Path.GetTempPath(), $"toplab-writer-{Guid.NewGuid():N}.pdf");
    }

    private static IReadOnlyList<string> ContentLines(ReportPrintEnvelope envelope)
    {
        return ReportContentBuilder
            .FromEnvelope(envelope, ReportSettings.CreateDefault(), SystemSettings.CreateDefault())
            .BuildDisplayLines();
    }

    [Fact]
    public async Task WritePdfAsync_WritesValidMinimalPdf()
    {
        var path = TempPath();
        try
        {
            await Writer.WritePdfAsync(path, CombinedEnvelope(), ReportSettings.CreateDefault(), SystemSettings.CreateDefault());

            Assert.True(File.Exists(path));
            var bytes = File.ReadAllBytes(path);
            Assert.Equal("%PDF", Encoding.ASCII.GetString(bytes, 0, 4));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task WritePdfAsync_ExistingTarget_NeverOverwrites()
    {
        var path = TempPath();
        File.WriteAllText(path, "occupied");
        try
        {
            await Assert.ThrowsAsync<IOException>(() =>
                Writer.WritePdfAsync(path, CombinedEnvelope(), ReportSettings.CreateDefault(), SystemSettings.CreateDefault()));

            Assert.Equal("occupied", File.ReadAllText(path));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void IdentifierLine_UsesPatientId_WhenPrintLabIdDisabled()
    {
        var settings = SystemSettings.CreateDefault();
        settings.SetGeneralFlags(false, false, false, false, false, false, false, false);

        var lines = ReportContentBuilder
            .FromEnvelope(CombinedEnvelope(), ReportSettings.CreateDefault(), settings)
            .BuildDisplayLines();

        Assert.Contains(lines, l => l.Contains("الرقم: 7", StringComparison.Ordinal));
        Assert.DoesNotContain(lines, l => l.Contains("رقم الملف", StringComparison.Ordinal));
    }

    [Fact]
    public void IdentifierLine_UsesLabId_WhenPrintLabIdEnabled()
    {
        var settings = SystemSettings.CreateDefault();
        settings.SetGeneralFlags(false, false, false, false, false, true, false, false);

        var lines = ReportContentBuilder
            .FromEnvelope(CombinedEnvelope(), ReportSettings.CreateDefault(), settings)
            .BuildDisplayLines();

        Assert.Contains(lines, l => l.Contains("رقم الملف: LAB-1", StringComparison.Ordinal));
        Assert.DoesNotContain(lines, l => l.StartsWith("الرقم:", StringComparison.Ordinal));
    }

    [Fact]
    public void BlankEnvelope_RendersPatientData()
    {
        var lines = ContentLines(BlankEnvelope());
        var all = string.Join('\n', lines);
        Assert.Contains("تقرير فارغ", all, StringComparison.Ordinal);
        Assert.Contains("Ali", all, StringComparison.Ordinal);
        Assert.Contains("Dr.Hassan", all, StringComparison.Ordinal);
        Assert.Contains("Al-Shifa", all, StringComparison.Ordinal);
    }

    [Fact]
    public void HistoryEnvelope_RendersEntriesWithReviewedFlag()
    {
        var lines = ContentLines(HistoryEnvelope());
        var all = string.Join('\n', lines);
        Assert.Contains("تقرير التاريخ", all, StringComparison.Ordinal);
        Assert.Contains("Glucose", all, StringComparison.Ordinal);
        Assert.Contains("نعم", all, StringComparison.Ordinal);
    }
}
