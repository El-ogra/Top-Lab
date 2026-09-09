using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.AnalyteProfiles.Common;

namespace TopLab.Application.Features.AnalyteProfiles.Queries.GetAnalyteDefinitions;

public sealed record GetAnalyteDefinitionsQuery()
    : IRequest<Result<IReadOnlyList<AnalyteDefinitionDto>>>, IAuthorizedRequest
{
    public string RequiredPermissionCode => "EDIT_SYSTEM_SETTINGS";
}