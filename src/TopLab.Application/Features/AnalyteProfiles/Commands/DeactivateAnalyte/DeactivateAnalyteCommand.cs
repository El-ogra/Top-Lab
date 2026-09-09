using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;

namespace TopLab.Application.Features.AnalyteProfiles.Commands.DeactivateAnalyte;

public sealed record DeactivateAnalyteCommand(
    int AnalyteId)
    : IRequest<Result>, IAuthorizedRequest
{
    public string RequiredPermissionCode => "EDIT_SYSTEM_SETTINGS";
}