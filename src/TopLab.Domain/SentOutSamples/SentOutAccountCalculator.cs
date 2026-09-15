namespace TopLab.Domain.SentOutSamples;

/// <summary>
/// Pure static Domain service implementing the settled sent-out settlement formula
/// (Data Model §8.3): full settlement = sum of payments equals the cost price.
/// TotalCost = Σ CostPrice; TotalPaid = Σ AmountPaid;
/// Remaining = TotalCost − TotalPaid (negative = over-payment, no clamping);
/// IsFullySettled = TotalPaid >= TotalCost.
/// Single source of the formula — never restated in Application (D5 principle).
/// </summary>
public static class SentOutAccountCalculator
{
    public static decimal TotalCost(IEnumerable<SentOutSample> samples)
    {
        ArgumentNullException.ThrowIfNull(samples);

        return samples.Sum(s => s.CostPrice);
    }

    public static decimal TotalPaid(IEnumerable<SentOutSamplePayment> payments)
    {
        ArgumentNullException.ThrowIfNull(payments);

        return payments.Sum(p => p.AmountPaid);
    }

    public static decimal Remaining(decimal totalCost, decimal totalPaid)
    {
        return totalCost - totalPaid;
    }

    public static bool IsFullySettled(decimal totalCost, decimal totalPaid)
    {
        return totalPaid >= totalCost;
    }
}
