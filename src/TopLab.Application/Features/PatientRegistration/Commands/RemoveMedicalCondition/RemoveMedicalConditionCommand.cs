using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientRegistration.Common;

namespace TopLab.Application.Features.PatientRegistration.Commands.RemoveMedicalCondition;

public sealed record RemoveMedicalConditionCommand(int PatientId, int MedicalConditionTypeId)
    : IRequest<Result<bool>>, IAuthorizedRequest
{
    public string RequiredPermissionCode => PatientRegistrationAccessPolicy.AddEditPatient;
}