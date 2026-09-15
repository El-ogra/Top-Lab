using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ReportProduction.Common;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Settings;

namespace TopLab.Application.Features.ReportProduction.Commands.AutoInsertHistory;

public sealed class AutoInsertHistoryCommandHandler : IRequestHandler<AutoInsertHistoryCommand, Result<CombinedReportDto>>
{
    private readonly IApplicationDbContext _db;

    public AutoInsertHistoryCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Result<CombinedReportDto>> Handle(
        AutoInsertHistoryCommand request, CancellationToken cancellationToken)
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

        // FR-M07-005: the auto-display master switch. When off this is a no-op
        // with an empty insertion set (EC-09) — nothing fires at all.
        if (!settings.HistoryAutoDisplayEnabled)
        {
            return Task.FromResult(Result<CombinedReportDto>.Success(
                EmptyModel(patient)));
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

        // FR-M07-003: prior results for the same test, under the resolved patient
        // identity, copied into the report model DTO — the triggering row itself
        // is excluded.
        var lines = PatientHistoryReader.BuildEntries(_db, visits)
            .Where(e => e.TestId == current.TestId.Value && e.PatientTestId != current.Id.Value)
            .OrderByDescending(e => e.EnteredAtUtc)
            .ThenByDescending(e => e.PatientTestId)
            .Select(HistoryInsertion.LineFromEntry)
            .ToList();

        return Task.FromResult(Result<CombinedReportDto>.Success(
            new CombinedReportDto(patient.Id.Value, patient.FullName, patient.LabId?.Value, lines)));
    }

    private static CombinedReportDto EmptyModel(Patient patient)
    {
        return new CombinedReportDto(
            patient.Id.Value,
            patient.FullName,
            patient.LabId?.Value,
            Array.Empty<CombinedReportLineDto>());
    }
}