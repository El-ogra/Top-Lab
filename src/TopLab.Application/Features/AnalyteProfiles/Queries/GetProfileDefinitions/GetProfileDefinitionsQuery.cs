using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.AnalyteProfiles.Common;

namespace TopLab.Application.Features.AnalyteProfiles.Queries.GetProfileDefinitions;

public sealed record GetProfileDefinitionsQuery()
    : IRequest<Result<IReadOnlyList<ProfileDefinitionDto>>>, IAuthorizedRequest
{
    public string RequiredPermissionCode => "EDIT_SYSTEM_SETTINGS";
}