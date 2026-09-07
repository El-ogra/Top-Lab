using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;

namespace TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.RenameCustomGroup;

public sealed record RenameCustomGroupCommand(int Id, string Name)
    : IRequest<Result>, IAuthorizedRequest
{
    public string RequiredPermissionCode => "EDIT_SYSTEM_SETTINGS";
}
