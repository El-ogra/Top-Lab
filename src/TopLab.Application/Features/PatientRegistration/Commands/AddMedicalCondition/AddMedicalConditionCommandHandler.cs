using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;

namespace TopLab.Application.Features.PatientRegistration.Commands.AddMedicalCondition;

public sealed class AddMedicalConditionCommandHandler : IRequestHandler<AddMedicalConditionCommand, Result<bool>>
{
    private readonly IApplicationDbContext _db;

    public AddMedicalConditionCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result<bool>> Handle(AddMedicalConditionCommand request, CancellationToken cancellationToken)
    {
        var patient = _db.Set<Patient>().FirstOrDefault(p => p.Id.Value == request.PatientId);
        if (patient is null)
        {
            return Result<bool>.Failure(Error.NotFound("المريض غير موجود."));
        }

        if (patient.IsDeleted)
        {
            return Result<bool>.Failure(Error.Conflict("المريض محذوف."));
        }

        if (!_db.Set<MedicalConditionType>().Any(m => m.Id.Value == request.MedicalConditionTypeId))
        {
            return Result<bool>.Failure(Error.NotFound("نوع الحالة الصحية غير موجود."));
        }

        patient.AddMedicalCondition(MedicalConditionTypeId.Create(request.MedicalConditionTypeId));
        await _db.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}