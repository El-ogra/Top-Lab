using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;

namespace TopLab.Application.Features.PatientRegistration.Commands.ClearAllTests;

public sealed class ClearAllTestsCommandHandler : IRequestHandler<ClearAllTestsCommand, Result<int>>
{
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