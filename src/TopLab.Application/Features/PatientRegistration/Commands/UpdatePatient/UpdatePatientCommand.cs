using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientRegistration.Common;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Patients;

namespace TopLab.Application.Features.PatientRegistration.Commands.UpdatePatient;

public sealed record UpdatePatientCommand(
    int PatientId,
    string FullName,
    Sex Sex,
    int AgeValue,
    AgeUnit AgeUnit,
    string? NationalId,
    string? Address,
    string? Title,
    bool IsVip,
    AccountType AccountType,
    string? Notes,
    bool IsFastingIndicated,
    int? FastingHours,
    bool RecentContrastImaging,
    IReadOnlyList<PatientNumberInput> PhoneNumbers)
    : IRequest<Result<bool>>, IAuthorizedRequest
{
    public string RequiredPermissionCode => PatientRegistrationAccessPolicy.AddEditPatient;
}