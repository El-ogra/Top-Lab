using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.Statistics.Common;

namespace TopLab.Application.Features.Statistics.Queries.GetUserProductivityStatistics;

public sealed record GetUserProductivityStatisticsQuery(
    DateOnly? From,
    DateOnly? To,
    int? UserId)
    : IRequest<Result<UserProductivityStatisticsDto>>, IAuthorizedRequest
{
    public string RequiredPermissionCode => StatisticsAccessPolicy.Statistics;
}
