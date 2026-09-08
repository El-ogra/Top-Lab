using TopLab.Application.Common.Interfaces;
using TopLab.Domain.Billing;
using TopLab.Domain.Results;

namespace TopLab.Application.Features.ResultsEntry.Common;

/// <summary>
/// Thin ResultsEntry data loader only (D5). Loads the patient's ordered prices
/// and payment operations, then delegates exclusively to
/// <see cref="PatientAccountCalculator.Balance"/> — the balance formula is never
/// duplicated here. Application already depends on Domain, so no cross-feature
/// dependency or adapter is required (inward-safe).
/// </summary>
internal static class BalanceProbe
{
    internal static decimal Balance(IApplicationDbContext db, int patientId)
    {
        var prices = db.Set<PatientTest>()
            .Where(pt => pt.PatientId.Value == patientId)
            .Select(pt => pt.PriceAtOrderTime)
            .ToList();

        var operations = db.Set<PaymentOperation>()
            .Where(o => o.PatientId.Value == patientId)
            .ToList();

        return PatientAccountCalculator.Balance(prices, operations);
    }
}
