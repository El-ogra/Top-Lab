using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientBilling.Common;

namespace TopLab.Application.Features.PatientBilling.Commands.RecordCorrection;

public sealed record RecordCorrectionCommand(
    int PatientId,
    decimal Amount) : IRequest<Result<int>>, IAuthorizedRequest
{
    public string RequiredPermissionCode => PatientBillingAccessPolicy.CashDisburseDeposit;
}
