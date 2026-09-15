using MediatR;
using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.InventoryAndAccounting.Common;

namespace TopLab.Application.Features.InventoryAndAccounting.Queries.GetCompanyDelegateAccounts;

public sealed record GetCompanyDelegateAccountsQuery(
    DateOnly? From,
    DateOnly? To,
    int? ExternalEntityId)
    : IRequest<Result<IReadOnlyList<CompanyDelegateAccountDto>>>, IAuthorizedRequest
{
    public string RequiredPermissionCode => InventoryAndAccountingAccessPolicy.CashDisburseDeposit;
}
