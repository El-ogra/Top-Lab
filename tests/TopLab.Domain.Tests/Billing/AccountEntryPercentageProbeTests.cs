using TopLab.Domain.Billing;
using Xunit;

namespace TopLab.Domain.Tests.Billing;

/// <summary>
/// R1 Round 5 — D-01 (owner decision, option 4 of card D-01).
/// <para>
/// The percentage discount is available ONLY when the payment settles the whole remaining
/// balance: <c>amount + percentage-discount == TotalCharged - net TotalPaid</c>. The percentage
/// keeps its base (the patient's TOTAL account amount, rounded to 2 decimals away from zero).
/// The Domain rule "discount cannot exceed the operation amount" stays untouched and is applied
/// as the SECOND check.
/// </para>
/// <para>
/// This file replaces the Round-4 characterization probe of the same name, which asserted the
/// pre-decision behaviour (percentage computed from the total and then rejected on every partial
/// payment). The Round-4 file stated in its own header that it "must be rewritten to assert the
/// chosen behaviour" once the owner decided. That is what this is.
/// </para>
/// </summary>
public class AccountEntryPercentageProbeTests
{
    private static AccountEntrySession Session(string amount, string? percentage = null, string? discount = null)
        => new() { AmountText = amount, PercentageText = percentage, DiscountText = discount };

    // ================================================================
    // The five required cases
    // ================================================================

    /// <summary>Case 1: TC 1000, nothing paid, amount 900, 10% → discount 100, accepted.</summary>
    [Fact]
    public void D01_Case1_FullSettlementByPercentage_IsAccepted()
    {
        var session = Session("900", "10");

        var conversionError = session.ApplyPercentage(totalAccountAmount: 1000m);
        var first = session.PressEnter(totalCharged: 1000m, alreadyPaid: 0m);

        Assert.Null(conversionError);
        Assert.Equal("100", session.DiscountText);   // 10% of 1000
        Assert.True(first.IsValid);
        Assert.False(first.ShouldPersist);           // first Enter previews only
        Assert.Equal(100m, first.Discount);
        Assert.Equal(900m, first.Amount);

        // second Enter with unchanged inputs persists
        var second = session.PressEnter(1000m, 0m);
        Assert.True(second.ShouldPersist);
    }

    /// <summary>Case 2: TC 1000, nothing paid, amount 100, 50% → refused as unavailable.</summary>
    [Fact]
    public void D01_Case2_PartialPaymentWithPercentage_IsRefusedAsUnavailable()
    {
        var session = Session("100", "50");

        session.ApplyPercentage(1000m);
        var step = session.PressEnter(1000m, 0m);

        Assert.False(step.IsValid);
        Assert.False(step.ShouldPersist);
        Assert.Equal(
            "النسبة متاحة فقط عند سداد كامل المتبقي: يجب أن يكون المدفوع + الخصم = 1000.",
            step.ErrorMessage);
    }

    /// <summary>Case 3: TC 1000, nothing paid, amount 1000, 10% → would create a 100 credit, refused.</summary>
    [Fact]
    public void D01_Case3_OverSettlementWithPercentage_IsRefused()
    {
        var session = Session("1000", "10");

        session.ApplyPercentage(1000m);
        var step = session.PressEnter(1000m, 0m);

        Assert.False(step.IsValid);
        Assert.Equal(
            "النسبة متاحة فقط عند سداد كامل المتبقي: يجب أن يكون المدفوع + الخصم = 1000.",
            step.ErrorMessage);
    }

    /// <summary>Case 4: TC 1000, already paid 400 (remaining 600), amount 500, 10% → accepted.</summary>
    [Fact]
    public void D01_Case4_RemainingBalanceAfterAPartPayment_IsAccepted()
    {
        var session = Session("500", "10");

        session.ApplyPercentage(1000m);
        var step = session.PressEnter(totalCharged: 1000m, alreadyPaid: 400m);

        Assert.True(step.IsValid);
        Assert.Null(step.ErrorMessage);
        Assert.Equal(100m, step.Discount);            // 10% of the TOTAL (1000), not of the 600
        Assert.Equal(500m, step.Amount);
        Assert.Equal(1000m - (400m + 500m + 100m), step.Preview!.BalanceAfter);
    }

    /// <summary>
    /// Case 5: TC 1000, nothing paid, amount 400, 60% → 400 + 600 = 1000 passes availability,
    /// then the Domain rule refuses because 600 &gt; 400. Proves the ORDER of the two checks.
    /// </summary>
    [Fact]
    public void D01_Case5_AvailabilityPassesThenTheDomainRuleRefuses()
    {
        var session = Session("400", "60");

        session.ApplyPercentage(1000m);
        var step = session.PressEnter(1000m, 0m);

        Assert.False(step.IsValid);
        Assert.Equal("الخصم لا يمكن أن يتجاوز مبلغ الدفع.", step.ErrorMessage);
    }

    /// <summary>Case 6: nothing remaining (or a credit) → refused with its own message.</summary>
    [Theory]
    [InlineData(1000, 1000)]   // remaining 0
    [InlineData(1000, 1200)]   // remaining negative (credit)
    public void D01_Case6_NoRemainingBalance_IsRefused(decimal totalCharged, decimal alreadyPaid)
    {
        var session = Session("100", "10");

        session.ApplyPercentage(totalCharged);
        var step = session.PressEnter(totalCharged, alreadyPaid);

        Assert.False(step.IsValid);
        Assert.Equal("لا يوجد مبلغ متبقٍ في الحساب لتطبيق نسبة عليه.", step.ErrorMessage);
    }

    /// <summary>Case 7: a value discount is untouched by D-01 — same results as before.</summary>
    [Fact]
    public void D01_Case7_ValueDiscount_IsUnchanged()
    {
        var session = Session("900", discount: "100");

        var step = session.PressEnter(totalCharged: 1000m, alreadyPaid: 0m);

        Assert.True(step.IsValid);
        Assert.Equal(100m, step.Discount);
        Assert.Equal(900m, step.Amount);
        Assert.False(step.ShouldPersist);
    }

    /// <summary>A value discount on a partial payment still works, as it did before D-01.</summary>
    [Fact]
    public void D01_Case7b_ValueDiscountOnAPartialPayment_StillWorks()
    {
        var session = Session("100", discount: "20");

        var step = session.PressEnter(totalCharged: 1000m, alreadyPaid: 0m);

        Assert.True(step.IsValid);
        Assert.Equal(20m, step.Discount);
    }

    /// <summary>
    /// Case 8: the Round-4 fix is preserved — converting a percentage never silently discards
    /// the discount it produced.
    /// </summary>
    [Fact]
    public void D01_Case8_PercentageIsNeverSilentlyCleared()
    {
        var session = Session("900", "10");

        var error = session.ApplyPercentage(1000m);

        Assert.Null(error);
        Assert.Equal("100", session.DiscountText);   // the amount survives the conversion
        Assert.Null(session.PercentageText);         // the percentage field is cleared (never both)
    }

    // ================================================================
    // The boundary and the ordering
    // ================================================================

    /// <summary>The availability rule is an equality, so both sides of it are refused.</summary>
    [Theory]
    [InlineData(1000, 0, 900, 9.99, false)]    // 900 + 99.9 = 999.9 -> refused
    [InlineData(1000, 0, 900, 10, true)]       // 900 + 100 = 1000 -> accepted
    [InlineData(1000, 0, 900, 10.01, false)]   // 900 + 100.1 = 1000.1 -> refused
    public void D01_TheAvailabilityRuleIsAnEquality(
        decimal totalCharged, decimal alreadyPaid, decimal amount, decimal percentage, bool accepted)
    {
        var session = Session(
            amount.ToString(System.Globalization.CultureInfo.InvariantCulture),
            percentage.ToString(System.Globalization.CultureInfo.InvariantCulture));

        session.ApplyPercentage(totalCharged);
        var step = session.PressEnter(totalCharged, alreadyPaid);

        Assert.Equal(accepted, step.IsValid);
    }

    /// <summary>
    /// With no percentage at all the availability rule never runs, so a partial payment is
    /// unaffected — the value-discount path is exactly as it was.
    /// </summary>
    [Fact]
    public void D01_WithoutAPercentage_TheAvailabilityRuleDoesNotRun()
    {
        var session = Session("100");

        var step = session.PressEnter(1000m, 0m);

        Assert.True(step.IsValid);
        Assert.Equal(100m, step.Amount);
    }

    /// <summary>Rounding follows the panel convention: 2 decimals, away from zero.</summary>
    [Fact]
    public void D01_PercentageIsRoundedToTwoDecimalsAwayFromZero()
    {
        // 33.333% of 100 = 33.333 -> 33.33, so 66.67 + 33.33 = 100 exactly.
        var session = Session("66.67", "33.333");

        session.ApplyPercentage(100m);
        var step = session.PressEnter(100m, 0m);

        Assert.Equal("33.33", session.DiscountText);
        Assert.True(step.IsValid);
    }
}
