using System.Text;
using TopLab.Application.Features.ResultsEntry.Common;
using TopLab.Infrastructure.Services;
using Xunit;

namespace TopLab.Infrastructure.Tests.Services;

public class PatientReportPdfExporterTests
{
    private static PatientReportPdfData SampleData()
    {
        return new PatientReportPdfData(
            1,
            "Test Patient",
            "LAB-1",
            new[]
            {
                new PatientReportPdfLine(
                    101, "CBC", "T10", "5.5", 0, null,
                    new FrozenRangeDto(10, null, "Year", 0, 100, 4m, 10m, null, null, DateTimeOffset.UtcNow),
                    Array.Empty<string>(),
                    null)
            });
    }

    [Fact]
    public async Task Export_Creates_ValidPdf()
    {
        var exporter = new PatientReportPdfExporter();
        var path = Path.Combine(Path.GetTempPath(), $"toplab-pdf-{Guid.NewGuid():N}.pdf");

        try
        {
            await exporter.ExportAsync(path, SampleData());

            Assert.True(File.Exists(path));
            var bytes = await File.ReadAllBytesAsync(path);
            Assert.True(bytes.Length > 0);
            Assert.Equal("%PDF", Encoding.ASCII.GetString(bytes, 0, 4));
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
    public async Task Export_DoesNotOverwrite_ExistingFile()
    {
        var exporter = new PatientReportPdfExporter();
        var path = Path.Combine(Path.GetTempPath(), $"toplab-pdf-{Guid.NewGuid():N}.pdf");
        await File.WriteAllBytesAsync(path, new byte[] { 1, 2, 3 });

        try
        {
            await Assert.ThrowsAsync<IOException>(() => exporter.ExportAsync(path, SampleData()));
            Assert.Equal(new byte[] { 1, 2, 3 }, await File.ReadAllBytesAsync(path));
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
    public async Task Export_MissingDirectory_Throws()
    {
        var exporter = new PatientReportPdfExporter();
        var path = Path.Combine(Path.GetTempPath(), $"missing-{Guid.NewGuid():N}", "report.pdf");

        await Assert.ThrowsAsync<DirectoryNotFoundException>(() => exporter.ExportAsync(path, SampleData()));
    }
}
