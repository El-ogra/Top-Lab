using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ReportProduction.Common;
using TopLab.Domain.Patients;
using TopLab.Domain.Settings;

namespace TopLab.Application.Features.ReportProduction.Queries.GetMultiPatientHistory;

public sealed class GetMultiPatientHistoryQueryHandler
    : IRequestHandler<GetMultiPatientHistoryQuery, Result<MultiPatientHistoryDto>>
{
    private readonly IApplicationDbContext _db;

    public GetMultiPatientHistoryQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Result<MultiPatientHistoryDto>> Handle(
        GetMultiPatientHistoryQuery request, CancellationToken cancellationToken)
    {
        var settings = _db.Set<ReportSettings>().SingleOrDefault(s => s.Id == 1);
        if (settings is null)
        {
            return Task.FromResult(Result<MultiPatientHistoryDto>.Failure(
                Error.Unexpected("سجل إعدادات التقرير مفقود.")));
        }

        var distinctIds = request.PatientIds.Distinct().ToList();
        var patients = _db.Set<Patient>()
            .Where(p => distinctIds.Contains(p.Id.Value) && !p.IsDeleted)
            .ToList();

        if (patients.Count != distinctIds.Count)
        {
            return Task.FromResult(Result<MultiPatientHistoryDto>.Failure(
                Error.NotFound("المريض غير موجود.")));
        }

        var union = new Dictionary<int, Patient>();
        foreach (var patient in patients)
        {
            IReadOnlyList<Patient> visits;
            try
            {
                visits = PatientHistoryReader.ResolveVisitPatients(_db, patient, settings);
            }
            catch (ArgumentException ex)
            {
                return Task.FromResult(Result<MultiPatientHistoryDto>.Failure(
                    Error.Conflict(DomainFailureTranslator.Translate(ex))));
            }

            foreach (var visit in visits)
            {
                union[visit.Id.Value] = visit;
            }
        }

        var ordered = PatientHistoryReader.OrderByIdentity(union.Values.ToList(), settings.HistorySortMode);
        var entries = PatientHistoryReader.BuildEntries(_db, ordered);

        var dto = new MultiPatientHistoryDto(
            settings.HistorySortMode.ToString(),
            settings.HistoryAutoDisplayEnabled,
            entries);

        return Task.FromResult(Result<MultiPatientHistoryDto>.Success(dto));
    }
}