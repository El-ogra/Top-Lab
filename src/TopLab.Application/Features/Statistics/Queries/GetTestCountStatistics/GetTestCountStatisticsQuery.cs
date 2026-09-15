using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.Statistics.Common;

namespace TopLab.Application.Features.Statistics.Queries.GetTestCountStatistics;

public sealed record GetTestCountStatisticsQuery(
    DateOnly? From,
    DateOnly? To,
    int? TestGroupId)
    : IRequest<Result<TestCountStatisticsDto>>, IAuthorizedRequest
{
    public string RequiredPermissionCode => StatisticsAccessPolicy.Statistics;
}
