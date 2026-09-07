using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientRegistration.Common;

namespace TopLab.Application.Features.PatientRegistration.Commands.UpdatePatientTestSampleFlags;

public sealed record UpdatePatientTestSampleFlagsCommand(
    int PatientTestId,
    bool IsUrine,
    bool IsStool,
    bool IsBlood,
    bool IsSemen,
    bool IsCsf,
    bool IsTakenOutsideLab)
    : IRequest<Result<bool>>, IAuthorizedRequest
{
    public string RequiredPermissionCode => PatientRegistrationAccessPolicy.AddEditPatient;
}