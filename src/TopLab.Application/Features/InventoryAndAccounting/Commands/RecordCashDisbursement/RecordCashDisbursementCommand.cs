using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.InventoryAndAccounting.Common;

namespace TopLab.Application.Features.InventoryAndAccounting.Commands.RecordCashDisbursement;

public sealed record RecordCashDisbursementCommand(
    decimal Amount,
    int? RelatedExternalEntityId,
    string? Notes)
    : IRequest<Result<int>>, IAuthorizedRequest
{
    public string RequiredPermissionCode => InventoryAndAccountingAccessPolicy.CashDisburseDeposit;
}
