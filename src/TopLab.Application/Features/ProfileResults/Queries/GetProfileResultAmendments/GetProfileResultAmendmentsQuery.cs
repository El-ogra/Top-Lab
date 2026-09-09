using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ProfileResults.Common;

namespace TopLab.Application.Features.ProfileResults.Queries.GetProfileResultAmendments;

public sealed record GetProfileResultAmendmentsQuery(
    int ProfileResultItemId)
    : IRequest<Result<IReadOnlyList<ProfileAmendmentDto>>>, IAuthorizedRequest
{
    public string RequiredPermissionCode => "PT_AUDIT_ACCESS";
}