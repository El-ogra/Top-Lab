using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;

namespace TopLab.Application.Features.SampleCollection.Commands.MarkSampleDrawn;

public sealed class MarkSampleDrawnCommandHandler : IRequestHandler<MarkSampleDrawnCommand, Result<bool>>
{
    private readonly IApplicationDbContext _db;
    private readonly IDateTimeProvider _clock;

    public MarkSampleDrawnCommandHandler(IApplicationDbContext db, IDateTimeProvider clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<Result<bool>> Handle(MarkSampleDrawnCommand request, CancellationToken cancellationToken)
    {
        var pt = _db.Set<PatientTest>().FirstOrDefault(t => t.Id.Value == request.PatientTestId);
        if (pt is null)
        {
            return Result<bool>.Failure(Error.NotFound("التحليل غير موجود."));
        }

        // PatientTest carries no IsDeleted flag (recorded M02 deviation in Handoff_M02):
        // the soft-delete guard is enforced here via the owning Patient.
        var patient = _db.Set<Patient>().FirstOrDefault(p => p.Id.Value == pt.PatientId.Value);
        if (patient is null)
        {
            return Result<bool>.Failure(Error.NotFound("المريض غير موجود."));
        }

        if (patient.IsDeleted)
        {
            return Result<bool>.Failure(Error.Conflict("المريض محذوف."));
        }

        if (pt.IsTakenOutsideLab)
        {
            return Result<bool>.Failure(Error.Conflict("تم تسجيل العينة كمسحوبة خارج المعمل؛ لا يمكن تعديلها من شاشة السحب"));
        }

        if (pt.IsSampleDrawn)
        {
            return Result<bool>.Success(true);
        }

        pt.MarkSampleDrawn(_clock.UtcNow);

        await _db.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(true);
    }
}
