using TopLab.Domain.Billing;

namespace TopLab.Domain.Billing;

/// <summary>
/// Pure static Domain service implementing the settled patient balance formula
/// (Data Model §7.1 + ADR-0016 + FR-M03-001/003/004):
/// TotalCharged = Σ PriceAtOrderTime + Σ Amount of non-voided extra-charge ops;
/// TotalPaid = Σ (Amount + coalesce(DiscountAmount, 0)) of non-voided non-extra-charge ops;
/// Balance = TotalCharged − TotalPaid (negative = credit, no clamping).
/// Correction rows contribute to TotalPaid (positive Amount = credit);
/// FullSettlement rows contribute as ordinary payments whose Amount equals the
/// balance at settlement time; voided rows contribute nothing.
/// </summary>
public static class PatientAccountCalculator
{
    public static decimal TotalCharged(
        IReadOnlyList<decimal> pricesAtOrderTime,
        IReadOnlyList<PaymentOperation> operations)
    {
        var testsTotal = pricesAtOrderTime.Sum();
        var extraCharges = operations
            .Where(o => !o.IsVoided && o.IsExtraCharge)
            .Sum(o => o.Amount);
        return testsTotal + extraCharges;
    }

    public static decimal TotalPaid(IReadOnlyList<PaymentOperation> operations)
    {
        return operations
            .Where(o => !o.IsVoided && !o.IsExtraCharge)
            .Sum(o => o.Amount + (o.DiscountAmount ?? 0m));
    }

    public static decimal Balance(
        IReadOnlyList<decimal> pricesAtOrderTime,
        IReadOnlyList<PaymentOperation> operations)
    {
        return TotalCharged(pricesAtOrderTime, operations) - TotalPaid(operations);
    }
}
