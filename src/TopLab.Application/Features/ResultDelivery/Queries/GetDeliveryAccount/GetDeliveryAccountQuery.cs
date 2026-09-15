using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ResultDelivery.Common;

namespace TopLab.Application.Features.ResultDelivery.Queries.GetDeliveryAccount;

public sealed record GetDeliveryAccountQuery(
    int PatientId) : IRequest<Result<DeliveryAccountDto>>, IAuthorizedRequest
{
    public string RequiredPermissionCode => ResultDeliveryAccessPolicy.DeliverResults;
}
