using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ReportProduction.Common;
using TopLab.Application.Features.ReportProduction.Queries.GetPatientTestHistory;

namespace TopLab.Application.Features.ReportProduction.Queries.GetSeparateHistoryReport;

/// <summary>
/// W-02 S12 (WP-10, C-25): the old twin handler was byte-identical to
/// <see cref="GetPatientTestHistoryQueryHandler"/>. The duplicate is deleted;
/// this thin handler keeps the query type (still consumed by the history screen
/// and the history print path) and delegates to the single implementation.
/// </summary>
public sealed class GetSeparateHistoryReportQueryHandler
    : IRequestHandler<GetSeparateHistoryReportQuery, Result<PatientHistoryDto>>
{
    private readonly IApplicationDbContext _db;

    public GetSeparateHistoryReportQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Result<PatientHistoryDto>> Handle(
        GetSeparateHistoryReportQuery request, CancellationToken cancellationToken)
    {
        return new GetPatientTestHistoryQueryHandler(_db).Handle(
            new GetPatientTestHistoryQuery(request.PatientId, request.FromUtc, request.ToUtc, request.TestId),
            cancellationToken);
    }
}
