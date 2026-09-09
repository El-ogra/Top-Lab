using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ResultsEntry.Common;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Settings;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.ResultsEntry.Commands.EnterResult;

public sealed class EnterResultCommandHandler : IRequestHandler<EnterResultCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;

    public EnterResultCommandHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IDateTimeProvider clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<Result> Handle(EnterResultCommand request, CancellationToken cancellationToken)
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
        if (test is null)
        {
            return Result.Failure(Error.NotFound("التحليل غير موجود"));
        }

        if (test.ResultKind != ResultKind.Simple)
        {
            return Result.Failure(Error.Conflict("لا يمكن إدخال نتيجة إلا لتحليل بسيط."));
        }

        if (string.IsNullOrWhiteSpace(request.ResultValue))
        {
            return Result.Failure(Error.Validation("الرجاء إدخال قيمة النتيجة قبل الحفظ"));
        }

        var analyteBands = ResultReferenceRangeSource.LoadAnalyteBands(_db, test);
        IReadOnlyList<ReferenceRange> currentRanges = analyteBands is null
            ? _db.Set<ReferenceRange>()
                .Where(r => r.TestId.Value == pt.TestId.Value)
                .ToList()
            : Array.Empty<ReferenceRange>();

        ResultFlag? flag;
        if (request.ResultFlag.HasValue)
        {
            flag = (ResultFlag)request.ResultFlag.Value;
        }
        else if (analyteBands is not null)
        {
            flag = ResultFlagComputer.Compute(
                request.ResultValue, patient.Sex, patient.AgeUnit, patient.AgeValue, analyteBands);
        }
        else
        {
            flag = ResultFlagComputer.Compute(
                request.ResultValue, patient.Sex, patient.AgeUnit, patient.AgeValue, currentRanges);
        }

        try
        {
            pt.EnterResult(request.ResultValue, flag, _currentUser.UserId, _clock.UtcNow, request.Notes);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(Error.Conflict(DomainFailureTranslator.Translate(ex)));
        }

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

        var settings = _db.Set<SystemSettings>().SingleOrDefault(s => s.Id == 1);
        if (settings?.AutoReviewAndComplete == true)
        {
            try
            {
                pt.MarkReviewed(_currentUser.UserId, _clock.UtcNow);
            }
            catch (InvalidOperationException ex)
            {
                return Result.Failure(Error.Conflict(DomainFailureTranslator.Translate(ex)));
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}