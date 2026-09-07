using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientRegistration.Common;

namespace TopLab.Application.Features.PatientRegistration.Commands.AddCustomGroupToVisit;

public sealed record AddCustomGroupToVisitCommand(int PatientId, int CustomGroupId)
    : IRequest<Result<IReadOnlyList<int>>>, IAuthorizedRequest
{
    public string RequiredPermissionCode => PatientRegistrationAccessPolicy.AddEditPatient;
}