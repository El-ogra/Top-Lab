using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientRegistration.Common;
using TopLab.Domain.Patients;

namespace TopLab.Application.Features.PatientRegistration.Commands.RenameMedicalConditionType;

public sealed class RenameMedicalConditionTypeCommandHandler
    : IRequestHandler<RenameMedicalConditionTypeCommand, Result>
{
    private readonly IApplicationDbContext _db;

    public RenameMedicalConditionTypeCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result> Handle(RenameMedicalConditionTypeCommand request, CancellationToken cancellationToken)
    {
        if (MedicalConditionCatalogue.IsSeeded(request.MedicalConditionTypeId))
        {
            return Result.Failure(Error.Validation(
                "لا يمكن تعديل أسماء الأنواع المدمجة في كتلة التاريخ الطبي."));
        }

        var type = _db.Set<MedicalConditionType>()
            .FirstOrDefault(m => m.Id.Value == request.MedicalConditionTypeId);

        if (type is null)
        {
            return Result.Failure(Error.NotFound("نوع الحالة الصحية غير موجود."));
        }

        var name = request.Name.Trim();

        if (_db.Set<MedicalConditionType>()
            .Any(m => m.Name == name && m.Id.Value != request.MedicalConditionTypeId))
        {
            return Result.Failure(Error.Conflict("نوع الحالة الصحية موجود بالفعل."));
        }

        type.Rename(name);
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
