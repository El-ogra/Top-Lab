using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientSearch.Common;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Settings;

namespace TopLab.Application.Features.PatientSearch.Queries.GetVisitHistory;

/// <summary>
/// Visit history for one registration: when the patient's LabId is null the history is
/// the single visit; otherwise it loads all non-deleted Patient rows sharing the LabId
/// (the one-row-per-visit grouping rule) and rolls them up in RegistrationDateUtc-desc
/// order. HistorySortMode / HistoryAutoDisplayEnabled are echoed (FR-M22-015); their real
/// cross-patient effect applies to merged multi-patient views outside this query.
/// </summary>
public sealed class GetVisitHistoryQueryHandler
    : IRequestHandler<GetVisitHistoryQuery, Result<VisitHistoryDto>>
{
    private readonly IApplicationDbContext _db;

    public GetVisitHistoryQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Result<VisitHistoryDto>> Handle(
        GetVisitHistoryQuery request, CancellationToken cancellationToken)
    {
        var patient = _db.Set<Patient>()
            .FirstOrDefault(p => p.Id.Value == request.PatientId);

        if (patient is null || patient.IsDeleted)
        {
            return Task.FromResult(Result<VisitHistoryDto>.Failure(
                Error.NotFound("المريض غير موجود.")));
        }

        var visits = string.IsNullOrWhiteSpace(patient.LabId?.Value)
            ? new List<Patient> { patient }
            : _db.Set<Patient>()
                .Where(p => !p.IsDeleted && p.LabId != null && p.LabId.Value == patient.LabId!.Value)
                .ToList();

        var settings = _db.Set<ReportSettings>().SingleOrDefault(s => s.Id == 1);
        if (settings is null)
        {
            return Task.FromResult(Result<VisitHistoryDto>.Failure(
                Error.Unexpected("سجل إعدادات التقرير مفقود.")));
        }

        var patientIds = visits.Select(v => v.Id).ToList();
        var tests = _db.Set<PatientTest>()
            .Where(pt => patientIds.Contains(pt.PatientId))
            .ToList();

        var history = VisitRollup.BuildHistory(
            patient.LabId?.Value ?? string.Empty,
            patient.FullName,
            settings,
            visits,
            tests,
            _db);

        return Task.FromResult(Result<VisitHistoryDto>.Success(history));
    }
}