using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;

namespace TopLab.Application.Features.PatientRegistration.Commands.AddMedicalConditionType;

public sealed class AddMedicalConditionTypeCommandHandler
    : IRequestHandler<AddMedicalConditionTypeCommand, Result<int>>
{
    private readonly IApplicationDbContext _db;

    public AddMedicalConditionTypeCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result<int>> Handle(AddMedicalConditionTypeCommand request, CancellationToken cancellationToken)
    {
        var name = request.Name.Trim();

        if (_db.Set<MedicalConditionType>().Any(m => m.Name == name))
        {
            return Result<int>.Failure(Error.Conflict("نوع الحالة الصحية موجود بالفعل."));
        }

        var type = MedicalConditionType.Create(MedicalConditionTypeId.Create(0), name, request.Category);
        _db.Add(type);
        await _db.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(type.Id.Value);
    }
}
