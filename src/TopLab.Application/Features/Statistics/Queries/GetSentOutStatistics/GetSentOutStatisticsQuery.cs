using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.Statistics.Common;

namespace TopLab.Application.Features.Statistics.Queries.GetSentOutStatistics;

public sealed record GetSentOutStatisticsQuery(
    DateOnly? From,
    DateOnly? To,
    int? ExternalLabEntityId)
    : IRequest<Result<SentOutStatisticsDto>>, IAuthorizedRequest
{
    public string RequiredPermissionCode => StatisticsAccessPolicy.Statistics;
}
