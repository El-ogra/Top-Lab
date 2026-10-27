using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.SentOutSamples.Common;

namespace TopLab.Application.Features.SentOutSamples.Commands.SetSentOutDefaultLab;

public sealed record SetSentOutDefaultLabCommand(int? ExternalLabEntityId)
    : IRequest<Result>, IAuthorizedRequest
{
    public string RequiredPermissionCode => SentOutSamplesAccessPolicy.CashDisburseDeposit;
}