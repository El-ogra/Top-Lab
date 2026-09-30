using QuestPDF;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Features.ReportProduction.Common;
using TopLab.Domain.Settings;

namespace TopLab.Infrastructure.Printing;

/// <summary>
/// QuestPDF implementation of <see cref="IReportPdfWriter"/> (WP-01). Renders the
/// clinical report in Arabic RTL via <see cref="ReportDocument"/>, honouring paper
/// size and margins from settings. Never overwrites: fails if the target exists.
/// </summary>
public sealed class ReportPdfWriter : IReportPdfWriter
{
    static ReportPdfWriter()
    {
        // Community-eligible reconfirmed 2026-09-29 by owner decision; revisit before commercial distribution.
        Settings.License = LicenseType.Community;

        // Arabic shaping needs a system font with Arabic glyphs (e.g. Arial on
        // Windows); QuestPDF 2026+ ships only Lato embedded and disables OS
        // font lookup by default.
        Settings.UseSystemFonts = true;
    }

    private readonly ITestDisplayNameResolver _nameResolver;
    private readonly ILabPrintTextStore? _labTextStore;

    public ReportPdfWriter(
        ITestDisplayNameResolver? nameResolver = null,
        ILabPrintTextStore? labTextStore = null)
    {
        _nameResolver = nameResolver ?? new TestDisplayNameResolver();
        _labTextStore = labTextStore;
    }

    public async Task WritePdfAsync(
        string absolutePath,
        ReportPrintEnvelope envelope,
        ReportSettings reportSettings,
        SystemSettings systemSettings,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(absolutePath);
        ArgumentNullException.ThrowIfNull(envelope);
        ArgumentNullException.ThrowIfNull(reportSettings);
        ArgumentNullException.ThrowIfNull(systemSettings);

        string? labName = null, labAddress = null, labPhone = null, fontFamily = null;
        if (_labTextStore is not null)
        {
            var labText = await _labTextStore.GetAsync(LabPrintTextScope.Report, cancellationToken);
            if (labText.IsSuccess && labText.Value is not null)
            {
                labName = labText.Value.LabName;
                labAddress = labText.Value.Address;
                labPhone = labText.Value.Phone;
                fontFamily = labText.Value.FontFamily;
            }
        }

        var content = ReportContentBuilder.FromEnvelope(
            envelope, reportSettings, systemSettings,
            labName, labAddress, labPhone,
            softwareBar: "Computer Software / real lab system",
            nameResolver: _nameResolver);

        var document = new ReportDocument(content, reportSettings, fontFamily);

        var directory = Path.GetDirectoryName(absolutePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            throw new DirectoryNotFoundException($"Directory not found: {directory}");
        }

        if (File.Exists(absolutePath))
        {
            throw new IOException($"File already exists: {absolutePath}");
        }

        document.GeneratePdf(absolutePath);
    }
}
