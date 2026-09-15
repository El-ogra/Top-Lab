using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.Statistics.Common;

namespace TopLab.Application.Features.Statistics.Queries.GetPatientCountStatistics;

public sealed record GetPatientCountStatisticsQuery(
    DateOnly? From,
    DateOnly? To,
    bool BySex,
    bool ByReferralEntity,
    bool ByAccountType,
    bool GroupByMonth)
    : IRequest<Result<PatientCountStatisticsDto>>, IAuthorizedRequest
{
    public string RequiredPermissionCode => StatisticsAccessPolicy.Statistics;
}
