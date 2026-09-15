using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.SentOutSamples.Common;

namespace TopLab.Application.Features.SentOutSamples.Commands.SendSampleOut;

public sealed record SendSampleOutCommand(
    int PatientTestId,
    int ExternalLabEntityId,
    decimal? CostPrice,
    decimal? PatientPrice)
    : IRequest<Result<int>>, IAuthorizedRequest
{
    public string RequiredPermissionCode => SentOutSamplesAccessPolicy.CashDisburseDeposit;
}
