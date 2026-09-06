using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;

namespace TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.SaveWorkGroupLogItems;

public sealed record SaveWorkGroupLogItemsCommand(int Id, IReadOnlyList<int> TestIds)
    : IRequest<Result>, IAuthorizedRequest
{
    public string RequiredPermissionCode => "EDIT_SYSTEM_SETTINGS";
}