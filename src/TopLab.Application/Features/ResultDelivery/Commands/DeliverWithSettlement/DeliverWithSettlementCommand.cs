using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ResultDelivery.Common;

namespace TopLab.Application.Features.ResultDelivery.Commands.DeliverWithSettlement;

public sealed record DeliverWithSettlementCommand(
    int PatientId,
    IReadOnlyList<int> PatientTestIds,
    decimal? SettleAmount = null,
    bool SettleInFull = false) : IRequest<Result>, IAuthorizedRequest
{
    public string RequiredPermissionCode => ResultDeliveryAccessPolicy.DeliverResults;
}
