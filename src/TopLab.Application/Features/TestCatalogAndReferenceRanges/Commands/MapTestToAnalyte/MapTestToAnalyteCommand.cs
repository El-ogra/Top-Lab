using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;

namespace TopLab.Application.Features.TestCatalogAndReferenceRanges.Commands.MapTestToAnalyte;

public sealed record MapTestToAnalyteCommand(
    int TestId,
    int AnalyteId)
    : IRequest<Result>, IAuthorizedRequest
{
    public string RequiredPermissionCode => "EDIT_SYSTEM_SETTINGS";
}