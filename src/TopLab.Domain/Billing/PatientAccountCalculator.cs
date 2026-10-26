using TopLab.Domain.Common.Enums;
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
        var payments = operations
            .Where(o => !o.IsVoided && !o.IsExtraCharge && o.OperationType != OperationType.Refund)
            .Sum(o => o.Amount + (o.DiscountAmount ?? 0m));
        var refunds = operations
            .Where(o => !o.IsVoided && o.OperationType == OperationType.Refund)
            .Sum(o => o.RefundAmount ?? 0m);
        return payments - refunds;
    }

    /// <summary>
    /// The cash the patient actually handed over, and therefore the only money that can be
    /// refunded (owner decision, R1 Round 5 Part B).
    /// <para>
    /// This is deliberately NOT <see cref="TotalPaid"/>: <c>TotalPaid</c> adds
    /// <c>DiscountAmount</c> because a discount is money the patient no longer owes, which is
    /// correct for the balance but wrong for a refund — a discount was never cash. Here the
    /// discount is excluded, and so are extra charges (money the patient owes, not money they
    /// paid) and refunds themselves.
    /// </para>
    /// <para>
    /// Correction operations are treated exactly as <see cref="TotalPaid"/> treats them: a
    /// correction is a non-voided, non-extra-charge, non-refund row, so its <c>Amount</c> counts
    /// as paid cash and is therefore refundable. FullSettlement rows likewise count as ordinary
    /// payments. Voided rows of any type contribute nothing. The result is floored at zero.
    /// </para>
    /// </summary>
    public static decimal CashRefundable(IReadOnlyList<PaymentOperation> operations)
    {
        var cashPaid = operations
            .Where(o => !o.IsVoided && !o.IsExtraCharge && o.OperationType != OperationType.Refund)
            .Sum(o => o.Amount);
        var refunds = operations
            .Where(o => !o.IsVoided && o.OperationType == OperationType.Refund)
            .Sum(o => o.RefundAmount ?? 0m);

        var net = cashPaid - refunds;
        return net > 0m ? net : 0m;
    }

    public static decimal Balance(
        IReadOnlyList<decimal> pricesAtOrderTime,
        IReadOnlyList<PaymentOperation> operations)
    {
        return TotalCharged(pricesAtOrderTime, operations) - TotalPaid(operations);
    }

    /// <summary>
    /// Central entry point for manual (ad-hoc) test selection (Decision 2): the order
    /// charges the sum of the individually resolved unit prices.
    /// </summary>
    public static decimal ManualSelectionCharge(IReadOnlyList<decimal> individualPrices)
    {
        ArgumentNullException.ThrowIfNull(individualPrices);

        if (individualPrices.Any(p => p < 0))
        {
            throw new ArgumentException("Individual prices must be >= 0.", nameof(individualPrices));
        }

        return individualPrices.Sum();
    }

    /// <summary>
    /// Central entry point for profile selection (Decision 2): the order charges the
    /// profile's immutable FixedPrice and never enumerates its constituent
    /// analytes/tests and is never overridden by price lists/custom groups.
    /// </summary>
    public static decimal ProfileSelectionCharge(decimal fixedPrice)
    {
        if (fixedPrice < 0)
        {
            throw new ArgumentException("FixedPrice must be >= 0.", nameof(fixedPrice));
        }

        return fixedPrice;
    }
}
