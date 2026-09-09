using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;

namespace TopLab.Application.Features.AnalyteProfiles.Commands.RemoveProfileAnalyte;

public sealed record RemoveProfileAnalyteCommand(
    int ProfileId,
    int AnalyteId)
    : IRequest<Result>, IAuthorizedRequest
{
    public string RequiredPermissionCode => "EDIT_SYSTEM_SETTINGS";
}