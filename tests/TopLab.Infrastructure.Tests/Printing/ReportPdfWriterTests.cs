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

    private static string WrittenText(string path)
    {
        Assert.True(File.Exists(path), $"Expected generated PDF at {path}");
        return Encoding.ASCII.GetString(File.ReadAllBytes(path));
    }

    [Fact]
    public async Task WritePdfAsync_WritesValidMinimalPdf()
    {
        var path = TempPath();
        try
        {
            await Writer.WritePdfAsync(path, CombinedEnvelope(), ReportSettings.CreateDefault(), SystemSettings.CreateDefault());

            Assert.StartsWith("%PDF-1.4", WrittenText(path));
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
    public async Task IdentifierLine_UsesPatientId_WhenPrintLabIdDisabled()
    {
        var path = TempPath();
        try
        {
            var settings = SystemSettings.CreateDefault();
            settings.SetGeneralFlags(false, false, false, false, false, false, false, false);

            await Writer.WritePdfAsync(path, CombinedEnvelope(), ReportSettings.CreateDefault(), settings);

            var text = WrittenText(path);
            Assert.Contains("PatientId: 7", text);
            Assert.DoesNotContain("LabId:", text);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task IdentifierLine_UsesLabId_WhenPrintLabIdEnabled()
    {
        var path = TempPath();
        try
        {
            var settings = SystemSettings.CreateDefault();
            settings.SetGeneralFlags(false, false, false, false, false, true, false, false);

            await Writer.WritePdfAsync(path, CombinedEnvelope(), ReportSettings.CreateDefault(), settings);

            var text = WrittenText(path);
            Assert.Contains("LabId: LAB-1", text);
            Assert.DoesNotContain("PatientId:", text);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task PaperAndTopSpaceLines_ReflectReportSettingsAtCallTime()
    {
        var path = TempPath();
        try
        {
            var reportSettings = ReportSettings.CreateDefault();
            reportSettings.SetPaperSize(PaperSize.A5);
            reportSettings.SetTopSpace(5m);

            await Writer.WritePdfAsync(path, CombinedEnvelope(), reportSettings, SystemSettings.CreateDefault());

            var text = WrittenText(path);
            Assert.Contains("Paper: A5", text);
            Assert.Contains("TopSpace: 5", text);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task HeaderFooterAndDoctorSignatureLines_ReflectSettings()
    {
        var path = TempPath();
        try
        {
            var reportSettings = ReportSettings.CreateDefault();
            reportSettings.SetHeaderFooterMode(HeaderFooterMode.Words);
            reportSettings.SetDoctorSignature(true);

            await Writer.WritePdfAsync(path, CombinedEnvelope(), reportSettings, SystemSettings.CreateDefault());

            var text = WrittenText(path);
            Assert.Contains("HeaderFooter: Words", text);
            Assert.Contains("Doctor Signature: Yes", text);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task BlankEnvelope_RendersPatientData()
    {
        var path = TempPath();
        try
        {
            await Writer.WritePdfAsync(path, BlankEnvelope(), ReportSettings.CreateDefault(), SystemSettings.CreateDefault());

            var text = WrittenText(path);
            Assert.Contains("TopLab Blank Report", text);
            Assert.Contains("Sex: Male", text);
            Assert.Contains("Age: 30 Year", text);
            Assert.Contains("Doctor: Dr.Hassan", text);
            Assert.Contains("Referral: Al-Shifa", text);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task HistoryEnvelope_RendersEntriesWithReviewedFlag()
    {
        var path = TempPath();
        try
        {
            await Writer.WritePdfAsync(path, HistoryEnvelope(), ReportSettings.CreateDefault(), SystemSettings.CreateDefault());

            var text = WrittenText(path);
            Assert.Contains("TopLab History Report", text);
            Assert.Contains("SortMode: ByLabCode", text);
            Assert.Contains("AutoDisplay: True", text);
            Assert.Contains("Reviewed: Yes", text);
            Assert.Contains("GLU Glucose: 5.5", text);
        }
        finally
        {
            File.Delete(path);
        }
    }
}