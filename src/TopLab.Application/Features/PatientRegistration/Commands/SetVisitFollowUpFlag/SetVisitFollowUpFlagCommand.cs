using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientRegistration.Common;
using TopLab.Domain.Common.Ids;

namespace TopLab.Application.Features.PatientRegistration.Commands.SetVisitFollowUpFlag;

public sealed record SetVisitFollowUpFlagCommand(int PatientId, bool IsFollowUpRequired)
    : IRequest<Result<bool>>, IAuthorizedRequest
{
    public string RequiredPermissionCode => PatientRegistrationAccessPolicy.AddEditPatient;
}
