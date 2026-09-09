using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientRegistration.Common;

namespace TopLab.Application.Features.PatientRegistration.Commands.AddProfileToVisit;

public sealed record AddProfileToVisitCommand(
    int PatientId,
    int ProfileId,
    bool IsUrine = false,
    bool IsStool = false,
    bool IsBlood = false,
    bool IsSemen = false,
    bool IsCsf = false,
    bool IsTakenOutsideLab = false)
    : IRequest<Result<int>>, IAuthorizedRequest
{
    public string RequiredPermissionCode => PatientRegistrationAccessPolicy.AddEditPatient;
}