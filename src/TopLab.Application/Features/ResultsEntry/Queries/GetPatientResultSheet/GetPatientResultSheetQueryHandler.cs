using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ResultsEntry.Common;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.ResultsEntry.Queries.GetPatientResultSheet;

public sealed class GetPatientResultSheetQueryHandler
    : IRequestHandler<GetPatientResultSheetQuery, Result<PatientResultSheetDto>>
{
    private readonly IApplicationDbContext _db;

    public GetPatientResultSheetQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Result<PatientResultSheetDto>> Handle(
        GetPatientResultSheetQuery request, CancellationToken cancellationToken)
    {
        var patient = _db.Set<Patient>().FirstOrDefault(p => p.Id.Value == request.PatientId);
        if (patient is null || patient.IsDeleted)
        {
            return Task.FromResult(Result<PatientResultSheetDto>.Failure(Error.NotFound("المريض غير موجود.")));
        }

        var tests = _db.Set<PatientTest>()
            .Where(pt => pt.PatientId.Value == patient.Id.Value)
            .OrderBy(pt => pt.Id.Value)
            .ToList();

        var catalog = _db.Set<Test>().ToDictionary(t => t.Id.Value);
        var snapshots = _db.Set<PatientTestReferenceRangeSnapshot>()
            .Where(s => tests.Select(t => t.Id.Value).Contains(s.PatientTestId.Value))
            .ToDictionary(s => s.PatientTestId.Value);

        var lines = tests.Select(pt =>
        {
            catalog.TryGetValue(pt.TestId.Value, out var test);
            snapshots.TryGetValue(pt.Id.Value, out var snapshot);
            FrozenRangeDto? frozen = snapshot == null
                ? null
                : new FrozenRangeDto(
                    snapshot.TestId,
                    snapshot.Sex?.ToString(),
                    snapshot.AgeUnit.ToString(),
                    snapshot.AgeMin,
                    snapshot.AgeMax,
                    snapshot.MinValue,
                    snapshot.MaxValue,
                    snapshot.LowComment,
                    snapshot.HighComment,
                    snapshot.CapturedAtUtc);

            return new ResultSheetLineDto(
                pt.Id.Value,
                test?.Name ?? string.Empty,
                test?.TestCode ?? string.Empty,
                pt.ResultValue,
                pt.ResultFlag == null ? null : (int)pt.ResultFlag.Value,
                pt.Notes,
                pt.IsReviewed,
                pt.IsPrinted,
                frozen);
        }).ToList();

        var dto = new PatientResultSheetDto(
            patient.Id.Value,
            patient.FullName,
            patient.LabId == null ? null : patient.LabId.Value,
            lines);

        return Task.FromResult(Result<PatientResultSheetDto>.Success(dto));
    }
}
