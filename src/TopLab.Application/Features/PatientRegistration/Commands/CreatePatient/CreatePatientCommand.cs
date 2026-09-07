using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientRegistration.Common;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;

namespace TopLab.Application.Features.PatientRegistration.Commands.CreatePatient;

public sealed record CreatePatientCommand(
    string FullName,
    Sex Sex,
    int AgeValue,
    AgeUnit AgeUnit,
    DateTime RegistrationDateUtc,
    AccountType AccountType,
    bool IsVip,
    string? LabId,
    string? Title,
    string? NationalId,
    string? Address,
    int? TreatingDoctorId,
    int? ReferralEntityId,
    DateTime? PickupDateUtc,
    bool IsFastingIndicated,
    int? FastingHours,
    bool RecentContrastImaging,
    string? Notes,
    IReadOnlyList<PatientNumberInput> PhoneNumbers,
    IReadOnlyList<int> MedicalConditionIds)
    : IRequest<Result<int>>, IAuthorizedRequest
{
    public string RequiredPermissionCode => PatientRegistrationAccessPolicy.AddEditPatient;
}