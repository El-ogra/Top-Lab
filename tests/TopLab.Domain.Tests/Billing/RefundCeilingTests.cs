using TopLab.Domain.Billing;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using Xunit;

namespace TopLab.Domain.Tests.Billing;

/// <summary>
/// R1 Round 5 Part B — the refund ceiling is the net CASH the patient actually paid.
/// <para>
/// Before the fix, <c>MaxRefundable</c> delegated to <c>PatientAccountCalculator.TotalPaid</c>,
/// which sums <c>Amount + DiscountAmount</c>. A discount is money the patient no longer OWES, not
/// money they PAID, so a discounted payment made the laboratory refund cash it never received.
/// </para>
/// <para>
/// <c>TotalPaid</c> and <c>Balance</c> are deliberately unchanged: a discount still reduces the
/// balance. Only the refund ceiling changed.
/// </para>
/// </summary>
public class RefundCeilingTests
{
    private static PaymentOperation Payment(decimal amount, decimal? discount = null, bool voided = false)
    {
        var op = PaymentOperation.Create(
            PaymentOperationId.Create(0), PatientId.Create(1), amount, 7, DateTime.UtcNow, discount);
        if (voided)
        {
            op.Void();
        }

        return op;
    }

    private static PaymentOperation Surcharge(decimal amount, bool voided = false)
    {
        var op = PaymentOperation.Create(
            PaymentOperationId.Create(0), PatientId.Create(1), amount, 7, DateTime.UtcNow, null, true);
        if (voided)
        {
            op.Void();
        }

        return op;
    }

    private static PaymentOperation Refund(decimal amount, bool voided = false)
    {
        var op = PaymentOperation.Create(
            PaymentOperationId.Create(0), PatientId.Create(1), 0m, 7, DateTime.UtcNow,
            null, false, OperationType.Refund, amount);
        if (voided)
        {
            op.Void();
        }

        return op;
    }

    private static PaymentOperation Correction(decimal amount, bool voided = false)
    {
        var op = PaymentOperation.Create(
            PaymentOperationId.Create(0), PatientId.Create(1), amount, 7, DateTime.UtcNow,
            null, false, OperationType.Correction);
        if (voided)
        {
            op.Void();
        }

        return op;
    }

    // ================================================================
    // The defect itself
    // ================================================================

    [Fact]
    public void Discount_IsExcludedFromTheCeiling_ButStillCountsInTotalPaid()
    {
        // Payment of 800 cash with a 200 discount: the patient handed over 800.
        var operations = new[] { Payment(800m, 200m) };

        Assert.Equal(1000m, PatientAccountCalculator.TotalPaid(operations));  // unchanged
        Assert.Equal(800m, AccountEntryCalculator.MaxRefundable(operations)); // cash only
    }

    [Fact]
    public void DiscountedPayment_OneUnitAboveTheCashPaid_IsAboveTheCeiling()
    {
        var operations = new[] { Payment(800m, 200m) };

        Assert.Equal(800m, AccountEntryCalculator.MaxRefundable(operations));
        Assert.True(800.01m > AccountEntryCalculator.MaxRefundable(operations));
    }

    [Fact]
    public void Balance_IsUnchangedByTheFix()
    {
        var operations = new[] { Payment(800m, 200m) };
        var prices = new[] { 1000m };

        // The discount still reduces the balance: 1000 - (800 + 200) = 0.
        Assert.Equal(0m, PatientAccountCalculator.Balance(prices, operations));
        // ...while only 800 is refundable.
        Assert.Equal(800m, AccountEntryCalculator.MaxRefundable(operations));
    }

    // ================================================================
    // The other operation types
    // ================================================================

    [Fact]
    public void Surcharge_IsNotRefundable_ItIsMoneyOwedNotMoneyPaid()
    {
        var operations = new[] { Payment(500m), Surcharge(300m) };

        Assert.Equal(500m, AccountEntryCalculator.MaxRefundable(operations));
    }

    [Fact]
    public void Correction_CountsAsPaidCash_ExactlyAsTotalPaidTreatsIt()
    {
        var operations = new[] { Payment(500m), Correction(50m) };

        Assert.Equal(550m, PatientAccountCalculator.TotalPaid(operations));
        Assert.Equal(550m, AccountEntryCalculator.MaxRefundable(operations));
    }

    [Fact]
    public void VoidedOperations_ContributeNothing_OfAnyType()
    {
        var operations = new[]
        {
            Payment(500m),
            Payment(400m, voided: true),
            Surcharge(300m, voided: true),
            Correction(70m, voided: true),
            Refund(60m, voided: true)
        };

        Assert.Equal(500m, AccountEntryCalculator.MaxRefundable(operations));
    }

    // ================================================================
    // Mixed scenarios
    // ================================================================

    [Fact]
    public void Mixed_DiscountsVoidsEarlierRefundsAndCorrections()
    {
        // Cash: 800 (disc 200) + 300 (disc 50) + correction 100 = 1200.
        // Surcharge 400 is not cash. Voided payment 999 is ignored.
        // Earlier refund 250 is subtracted.  => 950 refundable.
        var operations = new[]
        {
            Payment(800m, 200m),
            Payment(300m, 50m),
            Correction(100m),
            Surcharge(400m),
            Payment(999m, voided: true),
            Refund(250m)
        };

        Assert.Equal(950m, AccountEntryCalculator.MaxRefundable(operations));
        // TotalPaid still includes both discounts: 800+200+300+50+100 - 250 = 1200.
        Assert.Equal(1200m, PatientAccountCalculator.TotalPaid(operations));
    }

    [Fact]
    public void RefundEqualToTheCashPaid_LeavesZero()
    {
        var operations = new[] { Payment(800m, 200m), Refund(800m) };

        Assert.Equal(0m, AccountEntryCalculator.MaxRefundable(operations));
    }

    [Fact]
    public void NeverNegative_WhenRefundsExceedCash()
    {
        var operations = new[] { Payment(100m), Refund(150m) };

        Assert.Equal(0m, AccountEntryCalculator.MaxRefundable(operations));
    }

    [Fact]
    public void NoOperationsAtAll_YieldsZero()
    {
        Assert.Equal(0m, AccountEntryCalculator.MaxRefundable(Array.Empty<PaymentOperation>()));
    }

    [Fact]
    public void FullSettlement_CountsAsPaidCash()
    {
        var settlement = PaymentOperation.Create(
            PaymentOperationId.Create(0), PatientId.Create(1), 1000m, 7, DateTime.UtcNow,
            null, false, OperationType.FullSettlement);

        Assert.Equal(1000m, AccountEntryCalculator.MaxRefundable(new[] { settlement }));
    }
}
