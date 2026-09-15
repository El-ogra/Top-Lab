using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.InventoryAndAccounting.Common;

namespace TopLab.Application.Features.InventoryAndAccounting.Commands.RecordCashDeposit;

public sealed record RecordCashDepositCommand(
    decimal Amount,
    int? RelatedExternalEntityId,
    string? Notes)
    : IRequest<Result<int>>, IAuthorizedRequest
{
    public string RequiredPermissionCode => InventoryAndAccountingAccessPolicy.CashDisburseDeposit;
}
