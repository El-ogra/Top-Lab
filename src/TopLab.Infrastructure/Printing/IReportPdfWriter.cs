using TopLab.Application.Features.ReportProduction.Common;
using TopLab.Domain.Settings;

namespace TopLab.Infrastructure.Printing;

/// <summary>
/// Renders a <see cref="ReportPrintEnvelope"/> to a minimal valid PDF file at the
/// caller-supplied absolute path, honouring the print settings snapshot read at
/// print time. Never overwrites: uses <c>FileMode.CreateNew</c> (defense-in-depth,
/// mirrors <c>PatientReportPdfExporter</c>). Generation/write failures throw and
/// are mapped by <see cref="ReportPrintingService"/> to <c>Error.Unexpected</c>.
/// </summary>
public interface IReportPdfWriter
{
    Task WritePdfAsync(
        string absolutePath,
        ReportPrintEnvelope envelope,
        ReportSettings reportSettings,
        SystemSettings systemSettings,
        CancellationToken cancellationToken = default);
}