using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.AuditAndTraceability.Common;
using TopLab.Application.Features.UsersAndPermissions.Common;

namespace TopLab.Application.Features.AuditAndTraceability.Queries.GetAuditActivity;

/// <summary>
/// GAP-19 (4.5.1.2.2 / 4.5.1.2.3): audit activity rows across the three
/// audit-bearing sources, filtered by an inclusive period and (optionally) by
/// a set of users. <c>UserIds</c> null or empty means "show all".
/// </summary>
public sealed record GetAuditActivityQuery(
    DateOnly From,
    DateOnly To,
    IReadOnlyList<int>? UserIds)
    : IRequest<Result<IReadOnlyList<AuditActivityRowDto>>>, IAuthorizedRequest
{
    public string RequiredPermissionCode => AuditAccessPolicy.PtAuditAccess;
}