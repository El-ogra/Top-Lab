using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;

namespace TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.RemoveCustomGroupItem;

public sealed record RemoveCustomGroupItemCommand(int CustomGroupId, int TestId)
    : IRequest<Result>, IAuthorizedRequest
{
    public string RequiredPermissionCode => "EDIT_SYSTEM_SETTINGS";
}
