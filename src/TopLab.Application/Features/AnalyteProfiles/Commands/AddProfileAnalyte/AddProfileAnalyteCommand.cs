using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;

namespace TopLab.Application.Features.AnalyteProfiles.Commands.AddProfileAnalyte;

public sealed record AddProfileAnalyteCommand(
    int ProfileId,
    int AnalyteId)
    : IRequest<Result>, IAuthorizedRequest
{
    public string RequiredPermissionCode => "EDIT_SYSTEM_SETTINGS";
}