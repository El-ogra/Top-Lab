using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;

namespace TopLab.Application.Features.PatientRegistration.Commands.RemoveMedicalCondition;

public sealed class RemoveMedicalConditionCommandHandler : IRequestHandler<RemoveMedicalConditionCommand, Result<bool>>
{
    private readonly IApplicationDbContext _db;

    public RemoveMedicalConditionCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result<bool>> Handle(RemoveMedicalConditionCommand request, CancellationToken cancellationToken)
    {
        var patient = _db.Set<Patient>().FirstOrDefault(p => p.Id.Value == request.PatientId);
        if (patient is null)
        {
            return Result<bool>.Failure(Error.NotFound("المريض غير موجود."));
        }

        patient.RemoveMedicalCondition(MedicalConditionTypeId.Create(request.MedicalConditionTypeId));
        await _db.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}