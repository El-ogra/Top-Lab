using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;

namespace TopLab.Application.Features.AnalyteProfiles.Commands.CreateAnalyte;

public sealed record CreateAnalyteCommand(
    string Name,
    string ReportName)
    : IRequest<Result<int>>, IAuthorizedRequest
{
    public string RequiredPermissionCode => "EDIT_SYSTEM_SETTINGS";
}