using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ResultsEntry.Common;
using TopLab.Application.Features.TestCatalogAndReferenceRanges.Common;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.ResultsEntry.Queries.GetResultEntry;

public sealed class GetResultEntryQueryHandler : IRequestHandler<GetResultEntryQuery, Result<ResultEntryDto>>
{
    private readonly IApplicationDbContext _db;

    public GetResultEntryQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Result<ResultEntryDto>> Handle(GetResultEntryQuery request, CancellationToken cancellationToken)
    {
        var pt = _db.Set<PatientTest>().FirstOrDefault(t => t.Id.Value == request.PatientTestId);
        if (pt is null)
        {
            return Task.FromResult(Result<ResultEntryDto>.Failure(Error.NotFound("التحليل غير موجود")));
        }

        var patient = _db.Set<Patient>().FirstOrDefault(p => p.Id.Value == pt.PatientId.Value);
        if (patient is null || patient.IsDeleted)
        {
            return Task.FromResult(Result<ResultEntryDto>.Failure(Error.NotFound("المريض غير موجود.")));
        }

        var test = _db.Set<Test>().FirstOrDefault(t => t.Id.Value == pt.TestId.Value);

        var analyzerBands = test is null ? null : ResultReferenceRangeSource.LoadAnalyteBands(_db, test);

        var ranges = analyzerBands is not null
            ? analyzerBands
                .OrderBy(b => b.AgeMin)
                .ThenBy(b => b.AgeMax)
                .ThenBy(b => b.Id.Value)
                .Select(b => new ReferenceRangeDto(
                    b.Id.Value,
                    pt.TestId.Value,
                    b.Sex,
                    b.AgeUnit,
                    b.AgeMin,
                    b.AgeMax,
                    b.MinValue,
                    b.MaxValue,
                    b.LowComment,
                    b.HighComment))
                .ToList()
            : _db.Set<ReferenceRange>()
                .Where(r => r.TestId.Value == pt.TestId.Value)
                .OrderBy(r => r.AgeMin)
                .ThenBy(r => r.AgeMax)
                .ToList()
                .Select(r => new ReferenceRangeDto(
                    r.Id.Value,
                    r.TestId.Value,
                    r.Sex,
                    r.AgeUnit,
                    r.AgeMin,
                    r.AgeMax,
                    r.MinValue,
                    r.MaxValue,
                    r.LowComment,
                    r.HighComment))
                .ToList();

        var snapshot = _db.Set<PatientTestReferenceRangeSnapshot>()
            .FirstOrDefault(s => s.PatientTestId.Value == pt.Id.Value);

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

        var dto = new ResultEntryDto(
            pt.Id.Value,
            patient.Id.Value,
            test?.Name ?? string.Empty,
            test?.TestCode ?? string.Empty,
            test == null ? 0 : (int)test.ResultKind,
            pt.ResultValue,
            pt.ResultFlag == null ? null : (int)pt.ResultFlag.Value,
            pt.Notes,
            pt.IsReviewed,
            pt.IsPrinted,
            pt.IsDelivered,
            patient.AgeValue,
            patient.AgeUnit.ToString(),
            patient.Sex.ToString(),
            ranges,
            frozen);

        return Task.FromResult(Result<ResultEntryDto>.Success(dto));
    }
}
