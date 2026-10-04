using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;

namespace TopLab.Application.Features.PatientRegistration.Commands.ClearAllTests;

public sealed class ClearAllTestsCommandHandler : IRequestHandler<ClearAllTestsCommand, Result<int>>
{
    /// <summary>
    /// BR-A04-2 / SD-17: ONE Arabic literal, reused byte-for-byte in the handler, in the
    /// Presentation short-circuit and in the tests. Never reworded.
    /// </summary>
    public const string FirstRegistrationOnlyMessage = "لا يمكن مسح التحاليل إلا عند إضافة المريض أول مرة.";

    private readonly IApplicationDbContext _db;
    private readonly IDateTimeProvider _clock;

    public ClearAllTestsCommandHandler(IApplicationDbContext db, IDateTimeProvider clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<Result<int>> Handle(ClearAllTestsCommand request, CancellationToken cancellationToken)
    {
        var patient = _db.Set<Patient>().FirstOrDefault(p => p.Id.Value == request.PatientId);
        if (patient is null)
        {
            return Result<int>.Failure(Error.NotFound("المريض غير موجود."));
        }

        if (patient.IsDeleted)
        {
            return Result<int>.Failure(Error.Conflict("المريض محذوف."));
        }

        // OD-3 / BR-A04-1: the visit must BE the patient's first registration — the earliest
        // non-deleted Patient row in its LabId group (or the only row when LabId is null),
        // compared on RegistrationDateUtc with PatientId ascending as the tie-breaker.
        // BR-A04-3: this runs AFTER the existence and soft-delete checks and BEFORE the
        // 24-hour check. BR-A04-4: the two pre-existing guards below are untouched.
        // LabId is a strongly-typed string wrapper (LabId : StronglyTypedId<string>), so the
        // group is matched on Value, exactly as GetVisitHistoryQueryHandler does. The null
        // test is written as `!= null` rather than `is not null` because this predicate is an
        // EF expression tree, where pattern-matching operators are not allowed (CS8122).
        var labId = patient.LabId?.Value;
        var registeredAt = patient.RegistrationDateUtc;
        var patientId = patient.Id.Value;

        // "Is the first visit" == no OTHER non-deleted row in the same group sorts before
        // this one. Ordering is RegistrationDateUtc then PatientId, so the test for "before"
        // is the same two-key comparison — an exact tie on PatientId cannot occur.
        var isFirstRegistration = labId is null
            || !_db.Set<Patient>()
                .Where(p => !p.IsDeleted
                    && p.Id.Value != patientId
                    && p.LabId != null
                    && p.LabId.Value == labId)
                .Any(p => p.RegistrationDateUtc < registeredAt
                    || (p.RegistrationDateUtc == registeredAt && p.Id.Value < patientId));

        if (!isFirstRegistration)
        {
            return Result<int>.Failure(Error.Conflict(FirstRegistrationOnlyMessage));
        }

        if ((_clock.UtcNow - patient.CreatedAtUtc).TotalHours > 24)
        {
            return Result<int>.Failure(Error.Conflict("لا يمكن مسح التحاليل من مريض أضيف قبل أكثر من 24 ساعة."));
        }

        var tests = _db.Set<PatientTest>().Where(pt => pt.PatientId.Equals(patient.Id)).ToList();
        if (tests.Count == 0)
        {
            return Result<int>.Success(0);
        }

        if (tests.Any(t => t.EnteredAtUtc.HasValue || t.ResultValue is not null || t.ResultFlag.HasValue))
        {
            return Result<int>.Failure(Error.Conflict("لا يمكن مسح تحاليل تم تسجيل نتائج لها."));
        }

        foreach (var t in tests)
        {
            _db.Remove(t);
        }

        await _db.SaveChangesAsync(cancellationToken);
        return Result<int>.Success(tests.Count);
    }
}