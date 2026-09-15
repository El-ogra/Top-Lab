using TopLab.Application.Features.PatientBilling.Common;

namespace TopLab.Application.Features.SentOutSamples.Common;

/// <summary>
/// Feature-local permission policy for sent-out samples (OD-16-C).
/// Reuses the seeded <c>CASH_DISBURSE_DEPOSIT</c> code (id=11) — no new
/// permission seed, no migration. The code value is identical to
/// <see cref="PatientBillingAccessPolicy.CashDisburseDeposit"/>.
/// </summary>
public static class SentOutSamplesAccessPolicy
{
    public const string CashDisburseDeposit = PatientBillingAccessPolicy.CashDisburseDeposit;
}
