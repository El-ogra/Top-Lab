using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;

namespace TopLab.Application.Features.SampleCollection.Commands.MarkAllSamplesDrawnForPatient;

public sealed class MarkAllSamplesDrawnForPatientCommandHandler : IRequestHandler<MarkAllSamplesDrawnForPatientCommand, Result<int>>
{
    private readonly IApplicationDbContext _db;
    private readonly IDateTimeProvider _clock;

    public MarkAllSamplesDrawnForPatientCommandHandler(IApplicationDbContext db, IDateTimeProvider clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<Result<int>> Handle(MarkAllSamplesDrawnForPatientCommand request, CancellationToken cancellationToken)
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

        var pending = _db.Set<PatientTest>()
            .Where(pt => pt.PatientId.Value == patient.Id.Value
                && !pt.IsSampleDrawn
                && !pt.IsTakenOutsideLab)
            .OrderBy(pt => pt.Id.Value)
            .ToList();

        if (pending.Count == 0)
        {
            return Result<int>.Success(0);
        }

        foreach (var pt in pending)
        {
            pt.MarkSampleDrawn(_clock.UtcNow);
        }

        await _db.SaveChangesAsync(cancellationToken);
        return Result<int>.Success(pending.Count);
    }
}
