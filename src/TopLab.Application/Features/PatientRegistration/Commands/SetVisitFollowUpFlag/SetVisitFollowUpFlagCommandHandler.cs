using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;

namespace TopLab.Application.Features.PatientRegistration.Commands.SetVisitFollowUpFlag;

public sealed class SetVisitFollowUpFlagCommandHandler
    : IRequestHandler<SetVisitFollowUpFlagCommand, Result<bool>>
{
    private readonly IApplicationDbContext _db;

    public SetVisitFollowUpFlagCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result<bool>> Handle(SetVisitFollowUpFlagCommand request, CancellationToken cancellationToken)
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

        patient.SetFollowUpRequired(request.IsFollowUpRequired);
        await _db.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(patient.IsFollowUpRequired);
    }
}
