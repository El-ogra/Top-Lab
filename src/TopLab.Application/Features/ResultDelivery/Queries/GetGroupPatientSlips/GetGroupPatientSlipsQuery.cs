using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ResultDelivery.Common;

namespace TopLab.Application.Features.ResultDelivery.Queries.GetGroupPatientSlips;

/// <summary>
/// Reads one group-patient report slip per selected patient (Phase 6, GAP-013),
/// for the follow-up/delivery desk.
/// Patient selection reuses the established explicit-patient-id mechanism
/// (<c>BulkPrintPreflightQuery.PatientIds</c> precedent) — the follow-up list
/// already carries the ids the user ticked, so no second selection concept is
/// introduced.
/// Gated on the delivery desk's <c>DELIVER_RESULTS</c> permission, matching the
/// other queries on this surface.
/// </summary>
public sealed record GetGroupPatientSlipsQuery(
    IReadOnlyList<int> PatientIds) : IRequest<Result<GroupPatientSlipBatchDto>>, IAuthorizedRequest
{
    public string RequiredPermissionCode => ResultDeliveryAccessPolicy.DeliverResults;
}
