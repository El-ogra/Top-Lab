using TopLab.Application.Common.Interfaces;
using TopLab.Domain.Billing;
using TopLab.Domain.Results;

namespace TopLab.Application.Features.PatientSearch.Common;

/// <summary>
/// Private copy of the settled balance formula (plan §6.1): TotalCharged =
/// Σ PatientTest.PriceAtOrderTime + Σ (PaymentOperation.Amount where !IsVoided &amp;&amp;
/// IsExtraCharge); TotalPaid = Σ (Amount + DiscountAmount where !IsVoided &amp;&amp;
/// !IsExtraCharge); Balance = TotalCharged − TotalPaid. Per-visit by design.
/// </summary>
internal static class BalanceProbe
{
    public static decimal Balance(IApplicationDbContext db, int patientId)
    {
        var charged = db.Set<PatientTest>().Where(x => x.PatientId.Value == patientId).Sum(x => (decimal?)x.PriceAtOrderTime) ?? 0m;
        var operations = db.Set<PaymentOperation>().Where(x => x.PatientId.Value == patientId && !x.IsVoided).ToList();
        charged += operations.Where(x => x.IsExtraCharge).Sum(x => x.Amount);
        var paid = operations.Where(x => !x.IsExtraCharge).Sum(x => x.Amount + (x.DiscountAmount ?? 0m));
        return charged - paid;
    }
}