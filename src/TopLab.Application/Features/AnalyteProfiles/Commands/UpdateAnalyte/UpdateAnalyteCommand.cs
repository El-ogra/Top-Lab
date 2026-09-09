using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;

namespace TopLab.Application.Features.AnalyteProfiles.Commands.UpdateAnalyte;

public sealed record UpdateAnalyteCommand(
    int AnalyteId,
    string Name,
    string ReportName)
    : IRequest<Result>, IAuthorizedRequest
{
    public string RequiredPermissionCode => "EDIT_SYSTEM_SETTINGS";
}