using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.InventoryAndAccounting.Common;
using TopLab.Domain.Common.Enums;

namespace TopLab.Application.Features.InventoryAndAccounting.Queries.GetElementInventory;

public sealed record GetElementInventoryQuery(
    DateOnly? From,
    DateOnly? To,
    InventoryElementKind Element,
    int? ElementId,
    AccountType? AccountType,
    InventoryReportType ReportType)
    : IRequest<Result<ElementInventoryDto>>, IAuthorizedRequest
{
    public string RequiredPermissionCode => InventoryAndAccountingAccessPolicy.CashDisburseDeposit;
}
