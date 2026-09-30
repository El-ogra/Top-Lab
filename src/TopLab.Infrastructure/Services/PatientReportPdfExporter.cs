using QuestPDF.Fluent;
using TopLab.Application.Features.ResultsEntry.Common;
using TopLab.Domain.Settings;
using TopLab.Infrastructure.Printing;

namespace TopLab.Infrastructure.Services;

/// <summary>
/// Patient-result PDF export (D2). Shares the same QuestPDF
/// <see cref="ReportDocument"/> path as <c>IReportPdfWriter</c> — no second
/// ASCII writer. Never overwrites: fails if the target already exists
/// (defense-in-depth, mirrors the Application handler).
/// </summary>
public sealed class PatientReportPdfExporter : IPatientReportPdfExporter
{
    public Task ExportAsync(string absolutePath, PatientReportPdfData data, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(absolutePath);
        ArgumentNullException.ThrowIfNull(data);

        var reportSettings = ReportSettings.CreateDefault();
        var systemSettings = SystemSettings.CreateDefault();
        var content = ReportContentBuilder.FromPatientExport(
            data, reportSettings, systemSettings);

        var directory = Path.GetDirectoryName(absolutePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            throw new DirectoryNotFoundException($"Directory not found: {directory}");
        }

        if (File.Exists(absolutePath))
        {
            throw new IOException($"File already exists: {absolutePath}");
        }

        var document = new ReportDocument(content, reportSettings);
        document.GeneratePdf(absolutePath);
        return Task.CompletedTask;
    }
}
