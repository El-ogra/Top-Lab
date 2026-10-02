using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;

namespace TopLab.Application.Features.PatientRegistration.Commands.ApplyConditionDeltas;

/// <summary>
/// W-02 S14 (WP-29): condition adds and removals share one transaction and one
/// save. Checks run before any write, so a failure leaves the patient untouched.
/// </summary>
public sealed class ApplyConditionDeltasCommandHandler : IRequestHandler<ApplyConditionDeltasCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly IAppUnitOfWork _uow;

    public ApplyConditionDeltasCommandHandler(IApplicationDbContext db, IAppUnitOfWork uow)
    {
        _db = db;
        _uow = uow;
    }

    public Task<Result> Handle(ApplyConditionDeltasCommand request, CancellationToken cancellationToken) =>
        _uow.ExecuteAsync(async ct =>
        {
            var patient = _db.Set<Patient>().FirstOrDefault(p => p.Id.Value == request.PatientId);
            if (patient is null)
            {
                return Result.Failure(Error.NotFound("المريض غير موجود."));
            }

            if (patient.IsDeleted)
            {
                return Result.Failure(Error.Conflict("المريض محذوف."));
            }

            var current = patient.MedicalConditions.Select(m => m.MedicalConditionTypeId.Value).ToHashSet();
            var target = request.WantedMedicalConditionTypeIds.Distinct().ToList();

            foreach (var typeId in target.Where(t => !current.Contains(t)))
            {
                if (!_db.Set<MedicalConditionType>().Any(m => m.Id.Value == typeId))
                {
                    return Result.Failure(Error.NotFound("نوع الحالة الصحية غير موجود."));
                }
            }

            foreach (var typeId in target.Where(t => !current.Contains(t)))
            {
                patient.AddMedicalCondition(MedicalConditionTypeId.Create(typeId));
            }

            foreach (var typeId in current.Where(t => !target.Contains(t)))
            {
                patient.RemoveMedicalCondition(MedicalConditionTypeId.Create(typeId));
            }

            await _db.SaveChangesAsync(ct);
            return Result.Success();
        }, cancellationToken);
}
