using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Domain.Patients;

namespace TopLab.Application.Features.PatientRegistration.Commands.SoftDeletePatient;

public sealed class SoftDeletePatientCommandHandler : IRequestHandler<SoftDeletePatientCommand, Result<bool>>
{
    private readonly IApplicationDbContext _db;

    public SoftDeletePatientCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result<bool>> Handle(SoftDeletePatientCommand request, CancellationToken cancellationToken)
    {
        var patient = _db.Set<Patient>().FirstOrDefault(p => p.Id.Value == request.PatientId);
        if (patient is null)
        {
            return Result<bool>.Failure(Error.NotFound("المريض غير موجود."));
        }

        patient.SoftDelete();
        await _db.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}