using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ReportProduction.Common;
using TopLab.Domain.Patients;
using TopLab.Domain.Settings;

namespace TopLab.Application.Features.ReportProduction.Queries.GetPatientTestHistory;

public sealed class GetPatientTestHistoryQueryHandler
    : IRequestHandler<GetPatientTestHistoryQuery, Result<PatientHistoryDto>>
{
    private readonly IApplicationDbContext _db;

    public GetPatientTestHistoryQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Result<PatientHistoryDto>> Handle(
        GetPatientTestHistoryQuery request, CancellationToken cancellationToken)
    {
        var patient = _db.Set<Patient>().FirstOrDefault(p => p.Id.Value == request.PatientId);
        if (patient is null || patient.IsDeleted)
        {
            return Task.FromResult(Result<PatientHistoryDto>.Failure(
                Error.NotFound("المريض غير موجود.")));
        }

        var settings = _db.Set<ReportSettings>().SingleOrDefault(s => s.Id == 1);
        if (settings is null)
        {
            return Task.FromResult(Result<PatientHistoryDto>.Failure(
                Error.Unexpected("سجل إعدادات التقرير مفقود.")));
        }

        IReadOnlyList<Patient> visits;
        try
        {
            visits = PatientHistoryReader.ResolveVisitPatients(_db, patient, settings);
        }
        catch (ArgumentException ex)
        {
            return Task.FromResult(Result<PatientHistoryDto>.Failure(
                Error.Conflict(DomainFailureTranslator.Translate(ex))));
        }

        var entries = PatientHistoryReader.BuildEntries(_db, visits);

        var dto = new PatientHistoryDto(
            patient.Id.Value,
            patient.FullName,
            patient.LabId?.Value,
            settings.HistorySortMode.ToString(),
            settings.HistoryAutoDisplayEnabled,
            entries);

        return Task.FromResult(Result<PatientHistoryDto>.Success(dto));
    }
}