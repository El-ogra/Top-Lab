using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientBilling.Common;

namespace TopLab.Application.Features.PatientBilling.Queries.GetPatientReceipt;

public sealed record GetPatientReceiptQuery(
    int PatientId) : IRequest<Result<ReceiptDto>>, IAuthorizedRequest
{
    public string RequiredPermissionCode => "CASH_DISBURSE_DEPOSIT";
}
