using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ResultDelivery.Common;

namespace TopLab.Application.Features.ResultDelivery.Queries.GetDeliveryGrid;

public sealed record GetDeliveryGridQuery(
    int PatientId) : IRequest<Result<IReadOnlyList<DeliveryGridRowDto>>>, IAuthorizedRequest
{
    public string RequiredPermissionCode => ResultDeliveryAccessPolicy.DeliverResults;
}
