using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ResultDelivery.Common;

namespace TopLab.Application.Features.ResultDelivery.Queries.GetUndeliveredResults;

public sealed record GetUndeliveredResultsCountQuery(
    DateOnly? From = null,
    DateOnly? To = null) : IRequest<Result<int>>, IAuthorizedRequest
{
    public string RequiredPermissionCode => ResultDeliveryAccessPolicy.DeliverResults;
}