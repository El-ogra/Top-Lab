using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ReportProduction.Common;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Settings;

namespace TopLab.Application.Features.ReportProduction.Commands.InsertHistoryResult;

public sealed class InsertHistoryResultCommandHandler
    : IRequestHandler<InsertHistoryResultCommand, Result<CombinedReportDto>>
{
    private readonly IApplicationDbContext _db;

    public InsertHistoryResultCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Result<CombinedReportDto>> Handle(
        InsertHistoryResultCommand request, CancellationToken cancellationToken)
    {
        var current = _db.Set<PatientTest>().FirstOrDefault(pt => pt.Id.Value == request.PatientTestId);
        if (current is null)
        {
            return Task.FromResult(Result<CombinedReportDto>.Failure(
                Error.NotFound("التحليل غير موجود")));
        }

        var patient = _db.Set<Patient>().FirstOrDefault(p => p.Id.Value == current.PatientId.Value);
        if (patient is null || patient.IsDeleted)
        {
            return Task.FromResult(Result<CombinedReportDto>.Failure(
                Error.NotFound("المريض غير موجود.")));
        }

        var settings = _db.Set<ReportSettings>().SingleOrDefault(s => s.Id == 1);
        if (settings is null)
        {
            return Task.FromResult(Result<CombinedReportDto>.Failure(
                Error.Unexpected("سجل إعدادات التقرير مفقود.")));
        }

        IReadOnlyList<Patient> visits;
        try
        {
            visits = PatientHistoryReader.ResolveVisitPatients(_db, patient, settings);
        }
        catch (ArgumentException ex)
        {
            return Task.FromResult(Result<CombinedReportDto>.Failure(
                Error.Conflict(DomainFailureTranslator.Translate(ex))));
        }

        var source = _db.Set<PatientTest>().FirstOrDefault(pt => pt.Id.Value == request.SourcePatientTestId);
        if (source is null)
        {
            return Task.FromResult(Result<CombinedReportDto>.Failure(
                Error.NotFound("التحليل غير موجود")));
        }

        // FR-M07-006: manual insertion works regardless of the switch, but the
        // chosen prior result must belong to the resolved patient identity
        // (EC-10).
        if (!visits.Any(p => p.Id.Value == source.PatientId.Value))
        {
            return Task.FromResult(Result<CombinedReportDto>.Failure(
                Error.Conflict("النتيجة المحددة لا تنتمي لهذا المريض.")));
        }

        var sourcePatient = _db.Set<Patient>().First(p => p.Id.Value == source.PatientId.Value);
        var entry = PatientHistoryReader.BuildEntries(_db, new[] { sourcePatient })
            .First(e => e.PatientTestId == source.Id.Value);

        var line = HistoryInsertion.LineFromEntry(entry);
        return Task.FromResult(Result<CombinedReportDto>.Success(
            new CombinedReportDto(patient.Id.Value, patient.FullName, patient.LabId?.Value,
                new[] { line })));
    }
}