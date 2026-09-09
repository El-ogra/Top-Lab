using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ResultsEntry.Common;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.ResultsEntry.Commands.RefreshResultReferenceRange;

/// <summary>
/// Explicit user-triggered refresh of a result's frozen reference values after the
/// test's reference ranges changed (FR-M04-008 — report-side «تحديث» then «موافق»).
/// Old values persist until refresh; refresh replaces the snapshot and recomputes
/// the flag from the stored value.
/// </summary>
public sealed class RefreshResultReferenceRangeCommandHandler
    : IRequestHandler<RefreshResultReferenceRangeCommand, Result>
{
    private readonly IApplicationDbContext _db;

    public RefreshResultReferenceRangeCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result> Handle(RefreshResultReferenceRangeCommand request, CancellationToken cancellationToken)
    {
        var pt = _db.Set<PatientTest>().FirstOrDefault(t => t.Id.Value == request.PatientTestId);
        if (pt is null)
        {
            return Result.Failure(Error.NotFound("التحليل غير موجود"));
        }

        var patient = _db.Set<Patient>().FirstOrDefault(p => p.Id.Value == pt.PatientId.Value);
        if (patient is null || patient.IsDeleted)
        {
            return Result.Failure(Error.NotFound("المريض غير موجود."));
        }

        if (pt.IsReviewed)
        {
            return Result.Failure(Error.Conflict("النتيجة معتمدة؛ ألغِ الاعتماد أولاً."));
        }

        var test = _db.Set<Test>().FirstOrDefault(t => t.Id.Value == pt.TestId.Value);
        var analyteBands = test is null
            ? null
            : ResultReferenceRangeSource.LoadAnalyteBands(_db, test);
        IReadOnlyList<ReferenceRange> currentRanges = analyteBands is null
            ? _db.Set<ReferenceRange>()
                .Where(r => r.TestId.Value == pt.TestId.Value)
                .ToList()
            : Array.Empty<ReferenceRange>();

        var snapshotEntity = ResultReferenceRangeSource.Capture(
            pt.Id, pt.TestId.Value, patient.Sex, patient.AgeUnit, patient.AgeValue, analyteBands, currentRanges);
        var existing = _db.Set<PatientTestReferenceRangeSnapshot>()
            .FirstOrDefault(s => s.PatientTestId.Value == pt.Id.Value);

        if (snapshotEntity is not null)
        {
            if (existing is not null)
            {
                _db.Remove(existing);
            }

            _db.Add(snapshotEntity);
        }
        else if (existing is not null)
        {
            _db.Remove(existing);
        }

        var recomputed = analyteBands is not null
            ? ResultFlagComputer.Compute(pt.ResultValue, patient.Sex, patient.AgeUnit, patient.AgeValue, analyteBands)
            : ResultFlagComputer.Compute(pt.ResultValue, patient.Sex, patient.AgeUnit, patient.AgeValue, currentRanges);
        try
        {
            pt.EnterResult(pt.ResultValue, recomputed, pt.EnteredByUserId, pt.EnteredAtUtc, pt.Notes);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(Error.Conflict(DomainFailureTranslator.Translate(ex)));
        }

        await _db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
