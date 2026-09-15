using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.InventoryAndAccounting.Common;

namespace TopLab.Application.Features.InventoryAndAccounting.Queries.GetCashDrawerInventory;

public sealed record GetCashDrawerInventoryQuery(
    DateOnly? From,
    DateOnly? To)
    : IRequest<Result<CashDrawerInventoryDto>>, IAuthorizedRequest
{
    public string RequiredPermissionCode => InventoryAndAccountingAccessPolicy.CashDisburseDeposit;
}
