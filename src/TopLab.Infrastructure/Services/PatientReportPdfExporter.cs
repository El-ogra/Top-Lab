using QuestPDF;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
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
    /// <summary>
    /// P-02 D-1: <c>QuestPDF.Settings</c> is process-global, and this exporter is reached
    /// directly from the patient-report export command. Before this constructor existed the
    /// export succeeded or threw depending on whether some <em>other</em> writer had run
    /// first in the session — a real, pre-existing production defect.
    /// It now sets the licence itself, exactly as the six other writers do.
    /// </summary>
    static PatientReportPdfExporter()
    {
        Settings.License = LicenseType.Community;

        // Arabic shaping needs a system font with Arabic glyphs (e.g. Arial on Windows);
        // QuestPDF 2026+ ships only Lato embedded and disables OS font lookup by default.
        Settings.UseSystemFonts = true;
    }

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
