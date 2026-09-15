using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ResultDelivery.Common;

namespace TopLab.Application.Features.ResultDelivery.Queries.GetUndeliveredResults;

public sealed record GetUndeliveredResultsQuery(
    DateOnly? From = null,
    DateOnly? To = null,
    int Page = 1,
    int PageSize = 50) : IRequest<Result<IReadOnlyList<UndeliveredPatientRowDto>>>, IAuthorizedRequest
{
    public string RequiredPermissionCode => ResultDeliveryAccessPolicy.DeliverResults;
}
