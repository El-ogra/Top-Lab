using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientRegistration.Common;

namespace TopLab.Application.Features.PatientRegistration.Commands.AddTestGroupToVisit;

public sealed record AddTestGroupToVisitCommand(int PatientId, int TestGroupId)
    : IRequest<Result<IReadOnlyList<int>>>, IAuthorizedRequest
{
    public string RequiredPermissionCode => PatientRegistrationAccessPolicy.AddEditPatient;
}