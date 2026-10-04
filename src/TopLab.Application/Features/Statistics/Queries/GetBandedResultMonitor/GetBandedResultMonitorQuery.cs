using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.Statistics.Common;

namespace TopLab.Application.Features.Statistics.Queries.GetBandedResultMonitor;

/// <summary>
/// Banded result monitor (R-F05). Lists every result of one test, in one period, whose
/// numeric value lies inside the inclusive [MinValue, MaxValue] band (BR-F05-4).
/// </summary>
public sealed record GetBandedResultMonitorQuery(
    int TestId,
    DateOnly? From,
    DateOnly? To,
    decimal MinValue,
    decimal MaxValue)
    : IRequest<Result<BandedResultMonitorDto>>, IAuthorizedRequest
{
    public string RequiredPermissionCode => StatisticsAccessPolicy.Statistics;
}