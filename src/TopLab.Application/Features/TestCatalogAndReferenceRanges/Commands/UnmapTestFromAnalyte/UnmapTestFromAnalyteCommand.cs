using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;

namespace TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.UnmapTestFromAnalyte;

public sealed record UnmapTestFromAnalyteCommand(
    int TestId)
    : IRequest<Result>, IAuthorizedRequest
{
    public string RequiredPermissionCode => "EDIT_SYSTEM_SETTINGS";
}