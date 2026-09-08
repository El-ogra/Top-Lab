using TopLab.Application.Features.ResultsEntry.Common;

namespace TopLab.Application.Features.ResultsEntry.Common;

public sealed record PatientReportPdfLine(
    int PatientTestId,
    string TestName,
    string TestCode,
    string? ResultValue,
    int? ResultFlag,
    string? Notes,
    FrozenRangeDto? FrozenRange,
    IReadOnlyList<string> ProfileItems,
    string? CultureSummary);

public sealed record PatientReportPdfData(
    int PatientId,
    string PatientFullName,
    string? LabId,
    IReadOnlyList<PatientReportPdfLine> Lines);

public interface IPatientReportPdfExporter
{
    Task ExportAsync(string absolutePath, PatientReportPdfData data, CancellationToken cancellationToken = default);
}
