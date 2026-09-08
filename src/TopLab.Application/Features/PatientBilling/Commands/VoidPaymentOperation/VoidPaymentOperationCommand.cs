using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientBilling.Common;

namespace TopLab.Application.Features.PatientBilling.Commands.VoidPaymentOperation;

public sealed record VoidPaymentOperationCommand(
    int PaymentOperationId) : IRequest<Result>, IAuthorizedRequest
{
    public string RequiredPermissionCode => PatientBillingAccessPolicy.CashDisburseDeposit;
}
