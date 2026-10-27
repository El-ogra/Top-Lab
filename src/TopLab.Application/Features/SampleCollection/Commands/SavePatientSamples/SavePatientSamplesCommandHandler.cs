using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;

namespace TopLab.Application.Features.SampleCollection.Commands.SavePatientSamples;

public sealed class SavePatientSamplesCommandHandler : IRequestHandler<SavePatientSamplesCommand, Result<int>>
{
    private readonly IApplicationDbContext _db;

    public SavePatientSamplesCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result<int>> Handle(SavePatientSamplesCommand request, CancellationToken cancellationToken)
    {
        var patient = _db.Set<Patient>().SingleOrDefault(p => p.Id.Value == request.PatientId);
        if (patient is null || patient.IsDeleted)
        {
            return Result<int>.Failure(Error.Conflict("المريض محذوف."));
        }

        var tests = _db.Set<PatientTest>().Where(t => t.PatientId.Value == request.PatientId).ToList();
        var requested = new HashSet<int>(request.DrawnPatientTestIds ?? Array.Empty<int>());

        var changed = 0;
        foreach (var t in tests)
        {
            var shouldBeDrawn = requested.Contains(t.Id.Value);
            if (shouldBeDrawn && !t.IsSampleDrawn)
            {
                t.MarkSampleDrawn(DateTime.UtcNow);
                changed++;
            }
            else if (!shouldBeDrawn && t.IsSampleDrawn && !t.IsTakenOutsideLab)
            {
                t.ClearSampleDrawn();
                changed++;
            }
        }

        if (changed == 0)
        {
            return Result<int>.Success(0);
        }

        await _db.SaveChangesAsync(cancellationToken);
        return Result<int>.Success(changed);
    }
}