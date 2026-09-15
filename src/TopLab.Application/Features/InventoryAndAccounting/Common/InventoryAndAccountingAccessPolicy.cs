namespace TopLab.Application.Features.InventoryAndAccounting.Common;

/// <summary>
/// Permission code consumed by the M-20 Accounts surface.
/// All commands and queries are gated on CASH_DISBURSE_DEPOSIT (seeded id=11);
/// absolute-permission users bypass via the existing pipeline.
/// </summary>
public static class InventoryAndAccountingAccessPolicy
{
    public const string CashDisburseDeposit = "CASH_DISBURSE_DEPOSIT";
}
