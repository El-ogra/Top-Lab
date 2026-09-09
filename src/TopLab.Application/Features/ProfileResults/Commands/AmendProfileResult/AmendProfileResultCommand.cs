using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ResultsEntry.Common;

namespace TopLab.Application.Features.ProfileResults.Commands.AmendProfileResult;

public sealed record AmendProfileResultCommand(
    int ProfileResultItemId,
    string ResultValue,
    string? Unit,
    int? Flag,
    string? Reason)
    : IRequest<Result>, IAuthorizedRequest
{
    public string RequiredPermissionCode => ResultsEntryAccessPolicy.EditResults;
}