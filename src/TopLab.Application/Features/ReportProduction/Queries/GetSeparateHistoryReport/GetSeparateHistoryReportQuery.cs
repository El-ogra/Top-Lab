using MediatR;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ReportProduction.Common;

namespace TopLab.Application.Features.ReportProduction.Queries.GetSeparateHistoryReport;

/// <summary>
/// Assembles the standalone P-04 history report DTO (FR-M07-007) consumed by
/// <c>PrintHistoryReportCommand</c>. Fraternal twin of
/// <c>GetPatientTestHistoryQuery</c> that keeps the print path wired to the
/// separate-report model without touching the combined/history readers.
/// </summary>
public sealed record GetSeparateHistoryReportQuery(int PatientId)
    : IRequest<Result<PatientHistoryDto>>;