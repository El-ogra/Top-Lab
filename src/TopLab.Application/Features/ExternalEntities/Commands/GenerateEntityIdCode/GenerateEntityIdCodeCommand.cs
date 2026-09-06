using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;

namespace TopLab.Application.Features.ExternalEntities.Commands.GenerateEntityIdCode;

public sealed record GenerateEntityIdCodeCommand(int Id) : IRequest<Result<string>>, IAuthorizedRequest
{
    public string RequiredPermissionCode => "EDIT_SYSTEM_SETTINGS";
}
