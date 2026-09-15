using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.InventoryAndAccounting.Common;

namespace TopLab.Application.Features.InventoryAndAccounting.Queries.ListCashMovements;

public sealed record ListCashMovementsQuery(
    DateOnly? From,
    DateOnly? To)
    : IRequest<Result<IReadOnlyList<CashMovementDto>>>, IAuthorizedRequest
{
    public string RequiredPermissionCode => InventoryAndAccountingAccessPolicy.CashDisburseDeposit;
}
