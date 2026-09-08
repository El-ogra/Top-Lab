using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.SampleCollection.Common;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.SampleCollection.Queries.GetPatientTestsForDraw;

public sealed class GetPatientTestsForDrawQueryHandler
    : IRequestHandler<GetPatientTestsForDrawQuery, Result<SampleDrawBoardDto>>
{
    private readonly IApplicationDbContext _db;

    public GetPatientTestsForDrawQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Result<SampleDrawBoardDto>> Handle(
        GetPatientTestsForDrawQuery request, CancellationToken cancellationToken)
    {
        var patient = _db.Set<Patient>().FirstOrDefault(p => p.Id.Value == request.PatientId);
        if (patient is null)
        {
            var empty = new SampleDrawBoardDto(request.PatientId, null, string.Empty, [], []);
            return Task.FromResult(Result<SampleDrawBoardDto>.Success(empty));
        }

        var names = _db.Set<Test>().ToDictionary(t => t.Id.Value, t => t.Name);

        PatientTestDrawDto Map(PatientTest pt) => new(
            pt.Id.Value,
            pt.PatientId.Value,
            pt.TestId.Value,
            names.TryGetValue(pt.TestId.Value, out var name) ? name : string.Empty,
            pt.IsUrine,
            pt.IsStool,
            pt.IsBlood,
            pt.IsSemen,
            pt.IsCsf,
            pt.IsTakenOutsideLab,
            pt.IsSampleDrawn,
            pt.SampleDrawnAtUtc);

        var rows = _db.Set<PatientTest>()
            .Where(pt => pt.PatientId.Value == patient.Id.Value)
            .OrderBy(pt => pt.Id.Value)
            .ToList();

        var board = new SampleDrawBoardDto(
            patient.Id.Value,
            patient.LabId == null ? null : patient.LabId.Value,
            patient.FullName,
            rows.Where(pt => pt.IsSampleDrawn).Select(Map).ToList(),
            rows.Where(pt => !pt.IsSampleDrawn).Select(Map).ToList());

        return Task.FromResult(Result<SampleDrawBoardDto>.Success(board));
    }
}
