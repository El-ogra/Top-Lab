namespace TopLab.Application.Features.PatientBilling.Common;

/// <summary>
/// Permission codes consumed by M03 write commands. The single code is already
/// seeded in the permission catalog; M03 reuses it rather than adding a new row.
/// Note (settled): there is no discount-limit permission code — the discount
/// limit is User.DiscountLimitPercent enforced in the RecordPayment handler
/// (Data Model §13 BR-06), not a permission-code gate.
/// </summary>
public static class PatientBillingAccessPolicy
{
    public const string CashDisburseDeposit = "CASH_DISBURSE_DEPOSIT";
}
