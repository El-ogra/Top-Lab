using MediatR;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientRegistration.Common;
using TopLab.Domain.ExternalEntities;
using TopLab.Domain.Patients;

namespace TopLab.Application.Features.PatientRegistration.Queries.GetPatientById;

public sealed class GetPatientByIdQueryHandler
    : IRequestHandler<GetPatientByIdQuery, Result<PatientDetailDto>>
{
    private readonly IApplicationDbContext _db;

    public GetPatientByIdQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Result<PatientDetailDto>> Handle(
        GetPatientByIdQuery request, CancellationToken cancellationToken)
    {
        var patient = _db.Set<Patient>()
            .FirstOrDefault(p => p.Id.Value == request.PatientId);

        if (patient is null)
        {
            return Task.FromResult(Result<PatientDetailDto>.Failure(Error.NotFound("المريض غير موجود.")));
        }

        if (patient.IsDeleted)
        {
            return Task.FromResult(Result<PatientDetailDto>.Failure(Error.NotFound("المريض محذوف.")));
        }

        var phones = _db.Set<PatientPhoneNumber>()
            .Where(ph => ph.PatientId.Equals(patient.Id))
            .OrderBy(ph => ph.SortOrder)
            .ToList();

        var conditionJoin = _db.Set<PatientMedicalCondition>()
            .Where(mc => mc.PatientId.Equals(patient.Id))
            .ToList();

        var conditionIds = conditionJoin.Select(c => c.MedicalConditionTypeId).ToList();

        var conditions = _db.Set<MedicalConditionType>()
            .Where(mct => conditionIds.Contains(mct.Id))
            .ToList();

        var treatingDoctor = patient.TreatingDoctorId is null
            ? null
            : _db.Set<ExternalEntity>().FirstOrDefault(e => e.Id.Equals(patient.TreatingDoctorId));

        var referral = patient.ReferralEntityId is null
            ? null
            : _db.Set<ExternalEntity>().FirstOrDefault(e => e.Id.Equals(patient.ReferralEntityId));

        var dto = new PatientDetailDto(
            patient.Id.Value,
            patient.LabId?.Value,
            patient.Title,
            patient.FullName,
            patient.Sex,
            patient.AgeValue,
            patient.AgeUnit,
            patient.NationalId,
            patient.Address,
            patient.AccountType,
            patient.IsVip,
            patient.RegistrationDateUtc,
            patient.PickupDateUtc,
            patient.IsFastingIndicated,
            patient.FastingHours,
            patient.RecentContrastImaging,
            patient.Notes,
            patient.TreatingDoctorId?.Value,
            treatingDoctor?.Name,
            patient.ReferralEntityId?.Value,
            referral?.Name,
            phones.Select(ph => new PatientPhoneNumberDto(ph.Id.Value, ph.PhoneNumber, ph.SortOrder)).ToList(),
            conditions.Select(mct => new PatientMedicalConditionDto(mct.Id.Value, mct.Name)).ToList(),
            patient.IsDeleted);

        return Task.FromResult(Result<PatientDetailDto>.Success(dto));
    }
}