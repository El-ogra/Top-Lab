using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientSearch.Common;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Settings;

namespace TopLab.Application.Features.PatientSearch.Queries.GetPatientByLabId;

/// <summary>
/// Lab-ID lookup: returns the VisitHistoryDto rollup of every non-deleted visit sharing
/// the LabId (the latest visit by RegistrationDateUtc heads the list — FR-M08-006:
/// searching by Lab ID displays all of the patient's visits with their dates).
/// </summary>
public sealed class GetPatientByLabIdQueryHandler
    : IRequestHandler<GetPatientByLabIdQuery, Result<VisitHistoryDto>>
{
    private readonly IApplicationDbContext _db;

    public GetPatientByLabIdQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Result<VisitHistoryDto>> Handle(
        GetPatientByLabIdQuery request, CancellationToken cancellationToken)
    {
        var term = request.LabId.Trim();

        var matches = _db.Set<Patient>()
            .Where(p => !p.IsDeleted && p.LabId != null && p.LabId.Value.Trim() == term)
            .ToList();

        if (matches.Count == 0)
        {
            return Task.FromResult(Result<VisitHistoryDto>.Failure(
                Error.NotFound("لا يوجد مريض بهذا الكود.")));
        }

        var settings = _db.Set<ReportSettings>().SingleOrDefault(s => s.Id == 1);
        if (settings is null)
        {
            return Task.FromResult(Result<VisitHistoryDto>.Failure(
                Error.Unexpected("سجل إعدادات التقرير مفقود.")));
        }

        var latest = matches.OrderBy(p => p.RegistrationDateUtc).Last();

        var patientIds = matches.Select(m => m.Id).ToList();
        var tests = _db.Set<PatientTest>()
            .Where(pt => patientIds.Contains(pt.PatientId))
            .ToList();

        var history = VisitRollup.BuildHistory(
            latest.LabId!.Value, latest.FullName, settings, matches, tests, _db);

        return Task.FromResult(Result<VisitHistoryDto>.Success(history));
    }
}