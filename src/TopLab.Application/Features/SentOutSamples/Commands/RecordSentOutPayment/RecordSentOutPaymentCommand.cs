using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.SentOutSamples.Common;

namespace TopLab.Application.Features.SentOutSamples.Commands.RecordSentOutPayment;

public sealed record RecordSentOutPaymentCommand(
    int SentOutSampleId,
    decimal AmountPaid)
    : IRequest<Result>, IAuthorizedRequest
{
    public string RequiredPermissionCode => SentOutSamplesAccessPolicy.CashDisburseDeposit;
}
