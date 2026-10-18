using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ResultDelivery.Common;

namespace TopLab.Application.Features.ResultDelivery.Commands.PrintGroupPatientSlips;

/// <summary>
/// Prints one group-patient report slip per selected patient (Phase 6, GAP-013)
/// from the follow-up/delivery area. Gated on the delivery desk's
/// <c>DELIVER_RESULTS</c> permission, matching the other actions on this surface.
/// </summary>
public sealed record PrintGroupPatientSlipsCommand(
    IReadOnlyList<int> PatientIds) : IRequest<Result>, IAuthorizedRequest
{
    public string RequiredPermissionCode => ResultDeliveryAccessPolicy.DeliverResults;
}
