using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;

namespace TopLab.Application.Features.PriceListsCommentsAndCustomGroups.Commands.CreateTestComment;

public sealed record CreateTestCommentCommand(int TestId, string CommentText)
    : IRequest<Result<int>>, IAuthorizedRequest
{
    public string RequiredPermissionCode => "EDIT_SYSTEM_SETTINGS";
}
