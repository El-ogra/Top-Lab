using TopLab.Application.Common.Authorization;
using TopLab.Application.Common.Behaviors;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.InventoryAndAccounting.Commands.RecordCashDeposit;
using TopLab.Application.Features.InventoryAndAccounting.Commands.RecordCashDisbursement;
using TopLab.Application.Features.InventoryAndAccounting.Common;
using TopLab.Application.Features.InventoryAndAccounting.Queries.GetCashDrawerInventory;
using TopLab.Application.Features.InventoryAndAccounting.Queries.GetCompanyDelegateAccounts;
using TopLab.Application.Features.InventoryAndAccounting.Queries.GetElementInventory;
using TopLab.Application.Features.InventoryAndAccounting.Queries.GetPatientSamplesDetail;
using TopLab.Application.Features.InventoryAndAccounting.Queries.ListCashMovements;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using Xunit;

namespace TopLab.Application.Tests.Features.InventoryAndAccounting;

public class InventoryAndAccountingAuthorizationTests
{
    private const string ExpectedDenial = "أنت لا تملك الصلاحية لهذا العمل راجع مدير النظام";

    public static TheoryData<IAuthorizedRequest> ModuleQueries => new()
    {
        { new GetCashDrawerInventoryQuery(null, null) },
        { new GetElementInventoryQuery(null, null, InventoryElementKind.User, 1, null, InventoryReportType.Summary) },
        { new GetPatientSamplesDetailQuery(null, null) },
        { new RecordCashDepositCommand(1m, null, null) },
        { new RecordCashDisbursementCommand(1m, null, null) },
        { new ListCashMovementsQuery(null, null) },
        { new GetCompanyDelegateAccountsQuery(null, null, null) },
    };

    [Theory]
    [MemberData(nameof(ModuleQueries))]
    public void Queries_DeclareCashDisburseDeposit(IAuthorizedRequest query)
    {
        Assert.Equal("CASH_DISBURSE_DEPOSIT", query.RequiredPermissionCode);
        Assert.Equal(InventoryAndAccountingAccessPolicy.CashDisburseDeposit, query.RequiredPermissionCode);
    }

    [Fact]
    public async Task CashDrawer_DeniedWithoutPermission_ReturnsStandardMessage()
    {
        var user = new FakeCurrentUserService { IsAbsolutePermission = false };
        var behavior = new AuthorizationBehavior<GetCashDrawerInventoryQuery, Result<CashDrawerInventoryDto>>(user);

        var response = await behavior.Handle(
            new GetCashDrawerInventoryQuery(null, null),
            _ => throw new Exception("handler must not run"),
            CancellationToken.None);

        Assert.False(response.IsSuccess);
        Assert.Equal(ExpectedDenial, response.Error!.Message);
    }

    [Fact]
    public async Task CashDrawer_AbsolutePermission_BypassesWithoutGrant()
    {
        var user = new FakeCurrentUserService { IsAbsolutePermission = true };
        var behavior = new AuthorizationBehavior<GetCashDrawerInventoryQuery, Result<CashDrawerInventoryDto>>(user);

        var response = await behavior.Handle(
            new GetCashDrawerInventoryQuery(null, null),
            _ => Task.FromResult(Result<CashDrawerInventoryDto>.Success(null!)),
            CancellationToken.None);

        Assert.True(response.IsSuccess);
    }

    [Fact]
    public async Task CashDrawer_GrantedPermission_Allows()
    {
        var user = new FakeCurrentUserService { IsAbsolutePermission = false };
        user.GrantedPermissions.Add("CASH_DISBURSE_DEPOSIT");
        var behavior = new AuthorizationBehavior<GetCashDrawerInventoryQuery, Result<CashDrawerInventoryDto>>(user);

        var response = await behavior.Handle(
            new GetCashDrawerInventoryQuery(null, null),
            _ => Task.FromResult(Result<CashDrawerInventoryDto>.Success(null!)),
            CancellationToken.None);

        Assert.True(response.IsSuccess);
    }
}
