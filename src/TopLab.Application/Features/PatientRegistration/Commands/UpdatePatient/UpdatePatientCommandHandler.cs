using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientRegistration.Common;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;

namespace TopLab.Application.Features.PatientRegistration.Commands.UpdatePatient;

public sealed class UpdatePatientCommandHandler : IRequestHandler<UpdatePatientCommand, Result<bool>>
{
    private readonly IApplicationDbContext _db;

    public UpdatePatientCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result<bool>> Handle(UpdatePatientCommand request, CancellationToken cancellationToken)
    {
        var patient = _db.Set<Patient>().FirstOrDefault(p => p.Id.Value == request.PatientId);
        if (patient is null)
        {
            return Result<bool>.Failure(Error.NotFound("المريض غير موجود."));
        }

        if (patient.IsDeleted)
        {
            return Result<bool>.Failure(Error.Conflict("المريض محذوف ولا يمكن تعديله."));
        }

        try
        {
            patient.Update(
                request.FullName,
                request.Sex,
                request.AgeValue,
                request.AgeUnit,
                request.NationalId,
                request.Address,
                request.Title,
                request.IsVip,
                request.AccountType,
                request.Notes,
                request.IsFastingIndicated,
                request.FastingHours,
                request.RecentContrastImaging);
        }
        catch (ArgumentException ex)
        {
            return Result<bool>.Failure(Error.Validation(CreatePatient.DomainFailureTranslator.Translate(ex)));
        }

        patient.SetPhoneNumbers(request.PhoneNumbers ?? Array.Empty<PatientNumberInput>());

        await _db.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(true);
    }
}