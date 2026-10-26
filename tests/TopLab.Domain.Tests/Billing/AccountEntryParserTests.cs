using TopLab.Domain.Billing;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using Xunit;

namespace TopLab.Domain.Tests.Billing;

/// <summary>
/// R1 Round 3 B1/B2/B3: the shared account-entry parsing and calculation code
/// (canonical reference 1.3.1.2, 1.3.1.3, 1.3.1.4, 1.3.1.4.1, 1.3.1.4.2).
/// Pure Domain tests — no I/O, no screen.
/// </summary>
public class AccountEntryParserTests
{
    // ============================================================
    // Paid amount and the "+" surcharge convention (1.3.1.4.1)
    // ============================================================

    [Theory]
    [InlineData("100", 100)]
    [InlineData(" 100 ", 100)]
    [InlineData("100.50", 100.50)]
    [InlineData("0.01", 0.01)]
    public void ParsePaidAmount_PlainAmount_IsAPayment(string text, decimal expected)
    {
        var result = AccountEntryParser.ParsePaidAmount(text);

        Assert.True(result.IsValid);
        Assert.Equal(expected, result.Value);
        Assert.Equal(AccountEntryKind.Payment, result.Kind);
    }

    [Theory]
    [InlineData("+100", 100)]
    [InlineData(" + 100 ", 100)]
    [InlineData("+0.5", 0.5)]
    public void ParsePaidAmount_PlusPrefix_IsASurcharge(string text, decimal expected)
    {
        var result = AccountEntryParser.ParsePaidAmount(text);

        Assert.True(result.IsValid);
        Assert.Equal(expected, result.Value);
        Assert.Equal(AccountEntryKind.Surcharge, result.Kind);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    [InlineData("abc")]
    [InlineData("0")]
    [InlineData("-5")]
    [InlineData("+")]
    [InlineData("+0")]
    [InlineData("+abc")]
    public void ParsePaidAmount_Invalid_IsRejectedWithArabicMessage(string? text)
    {
        var result = AccountEntryParser.ParsePaidAmount(text);

        Assert.False(result.IsValid);
        Assert.False(string.IsNullOrWhiteSpace(result.ErrorMessage));
    }

    [Fact]
    public void ParsePaidAmount_BarePlus_UsesTheSurchargeMessage()
    {
        var result = AccountEntryParser.ParsePaidAmount("+");

        Assert.False(result.IsValid);
        Assert.Equal(AccountEntryParser.InvalidSurchargeMessage, result.ErrorMessage);
    }

    [Fact]
    public void ParsePaidAmount_ZeroSurcharge_UsesTheSurchargeMessage()
    {
        var result = AccountEntryParser.ParsePaidAmount("+0");

        Assert.False(result.IsValid);
        Assert.Equal(AccountEntryParser.InvalidSurchargeMessage, result.ErrorMessage);
    }

    [Fact]
    public void ParsePaidAmount_Negative_UsesThePaymentMessage()
    {
        var result = AccountEntryParser.ParsePaidAmount("-5");

        Assert.False(result.IsValid);
        Assert.Equal(AccountEntryParser.InvalidAmountMessage, result.ErrorMessage);
    }

    // ============================================================
    // Arabic-Indic digits
    // ============================================================

    [Theory]
    [InlineData("\u0661\u0660\u0660", 100)]          // ١٠٠
    [InlineData("\u0660\u066B\u0665", 0.5)]          // ٠٫٥
    [InlineData("\u06F1\u06F2\u06F3", 123)]          // ۱۲۳ (extended)
    public void ParsePaidAmount_ArabicIndicDigits_AreAccepted(string text, decimal expected)
    {
        var result = AccountEntryParser.ParsePaidAmount(text);

        Assert.True(result.IsValid);
        Assert.Equal(expected, result.Value);
    }

    [Fact]
    public void NormalizeDigits_MapsBothArabicDigitRanges()
    {
        Assert.Equal("0123456789", AccountEntryParser.NormalizeDigits("\u0660\u0661\u0662\u0663\u0664\u0665\u0666\u0667\u0668\u0669"));
        Assert.Equal("0123456789", AccountEntryParser.NormalizeDigits("\u06F0\u06F1\u06F2\u06F3\u06F4\u06F5\u06F6\u06F7\u06F8\u06F9"));
    }

    // ============================================================
    // Discount value (1.3.1.2)
    // ============================================================

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void ParseDiscount_Empty_MeansNoDiscount(string? text)
    {
        var result = AccountEntryParser.ParseDiscount(text);

        Assert.True(result.IsValid);
        Assert.Equal(0m, result.Value);
    }

    [Fact]
    public void ParseDiscount_Value_IsAccepted()
    {
        Assert.Equal(12.5m, AccountEntryParser.ParseDiscount("12.5").Value);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("-1")]
    public void ParseDiscount_Invalid_IsRejectedWithArabicMessage(string text)
    {
        var result = AccountEntryParser.ParseDiscount(text);

        Assert.False(result.IsValid);
        Assert.Equal(AccountEntryParser.InvalidDiscountMessage, result.ErrorMessage);
    }

    // ============================================================
    // Discount percentage (1.3.1.3)
    // ============================================================

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void ParsePercentage_Empty_MeansNoPercentage(string? text)
    {
        var result = AccountEntryParser.ParsePercentage(text);

        Assert.True(result.IsValid);
        Assert.Equal(0m, result.Value);
    }

    [Theory]
    [InlineData("1", 1)]
    [InlineData("10", 10)]
    [InlineData("100", 100)]
    [InlineData("12.5", 12.5)]
    [InlineData("25%", 25)]
    public void ParsePercentage_InRange_IsAccepted(string text, decimal expected)
    {
        var result = AccountEntryParser.ParsePercentage(text);

        Assert.True(result.IsValid);
        Assert.Equal(expected, result.Value);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-5")]
    [InlineData("100.01")]
    [InlineData("101")]
    [InlineData("abc")]
    public void ParsePercentage_OutOfRange_IsRejectedWithArabicMessage(string text)
    {
        var result = AccountEntryParser.ParsePercentage(text);

        Assert.False(result.IsValid);
        Assert.Equal(AccountEntryParser.InvalidPercentageMessage, result.ErrorMessage);
    }

    // ============================================================
    // Refund amount (1.3.2.5)
    // ============================================================

    [Theory]
    [InlineData("50", 50)]
    [InlineData("0.01", 0.01)]
    public void ParseRefundAmount_Positive_IsAccepted(string text, decimal expected)
    {
        var result = AccountEntryParser.ParseRefundAmount(text);

        Assert.True(result.IsValid);
        Assert.Equal(expected, result.Value);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData(null)]
    public void ParseRefundAmount_ZeroOrNegativeOrUnreadable_IsRejected(string? text)
    {
        var result = AccountEntryParser.ParseRefundAmount(text);

        Assert.False(result.IsValid);
        Assert.Equal(AccountEntryParser.InvalidAmountMessage, result.ErrorMessage);
    }

    // ============================================================
    // Prefix helpers
    // ============================================================

    [Theory]
    [InlineData("+", true)]
    [InlineData("+5", false)]
    [InlineData("5", false)]
    [InlineData(null, false)]
    public void IsSurchargePrefixOnly_DetectsABarePlus(string? text, bool expected)
        => Assert.Equal(expected, AccountEntryParser.IsSurchargePrefixOnly(text));

    [Theory]
    [InlineData("+5", true)]
    [InlineData("+ 5", true)]
    [InlineData("5", false)]
    [InlineData(null, false)]
    public void HasSurchargePrefix_DetectsThePrefix(string? text, bool expected)
        => Assert.Equal(expected, AccountEntryParser.HasSurchargePrefix(text));
}

/// <summary>R1 Round 3 B1: the percentage-to-amount conversion and the first-press figures.</summary>
public class AccountEntryCalculatorTests
{
    [Fact]
    public void DiscountAmountFromPercentage_IsOfTheTotalAccountAmount()
    {
        // 10% of the patient's TOTAL account amount (200) is 20.
        Assert.Equal(20m, AccountEntryCalculator.DiscountAmountFromPercentage(200m, 10m));
    }

    [Fact]
    public void DiscountAmountFromPercentage_RoundsAwayFromZeroToTwoDecimals()
    {
        // 33.333% of 100 = 33.333 -> 33.33; 1% of 0.005 = 0.00005 -> 0.00
        Assert.Equal(33.33m, AccountEntryCalculator.DiscountAmountFromPercentage(100m, 33.333m));
        // 12.5% of 10.01 = 1.25125 -> 1.25
        Assert.Equal(1.25m, AccountEntryCalculator.DiscountAmountFromPercentage(10.01m, 12.5m));
        // 50% of 0.05 = 0.025 -> 0.03 (away from zero, not banker's rounding)
        Assert.Equal(0.03m, AccountEntryCalculator.DiscountAmountFromPercentage(0.05m, 50m));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(100.01)]
    public void DiscountAmountFromPercentage_OutOfRange_Throws(decimal percentage)
    {
        Assert.Throws<ArgumentException>(() => AccountEntryCalculator.DiscountAmountFromPercentage(100m, percentage));
    }

    [Fact]
    public void DiscountAmountFromPercentage_NoChargedAmount_YieldsZero()
    {
        Assert.Equal(0m, AccountEntryCalculator.DiscountAmountFromPercentage(0m, 10m));
    }

    [Fact]
    public void Preview_Payment_ShowsAmountDiscountAndRemainingBalance()
    {
        var preview = AccountEntryCalculator.Preview(200m, 50m, 100m, 20m, AccountEntryKind.Payment);

        Assert.Equal(100m, preview.Amount);
        Assert.Equal(20m, preview.Discount);
        Assert.Equal(200m - (50m + 100m + 20m), preview.BalanceAfter);
    }

    [Fact]
    public void Preview_Surcharge_RaisesTheChargedSideAndCarriesNoDiscount()
    {
        var preview = AccountEntryCalculator.Preview(200m, 50m, 30m, 0m, AccountEntryKind.Surcharge);

        Assert.Equal(0m, preview.Amount);
        Assert.Equal(0m, preview.Discount);
        Assert.Equal(230m, preview.TotalChargedAfter);
        Assert.Equal(180m, preview.BalanceAfter);
    }

    [Fact]
    public void MaxRefundable_IsNetPaidAfterEarlierRefundsAndVoids()
    {
        var operations = new List<PaymentOperation>
        {
            PaymentOperation.Create(PaymentOperationId.Create(1), PatientId.Create(1), 100m, 7, DateTime.UtcNow),
            PaymentOperation.Create(PaymentOperationId.Create(2), PatientId.Create(1), 50m, 7, DateTime.UtcNow)
        };
        var refund = PaymentOperation.Create(
            PaymentOperationId.Create(3), PatientId.Create(1), 0m, 7, DateTime.UtcNow,
            null, false, OperationType.Refund, 30m);
        operations.Add(refund);

        var voided = PaymentOperation.Create(PaymentOperationId.Create(4), PatientId.Create(1), 40m, 7, DateTime.UtcNow);
        voided.Void();
        operations.Add(voided);

        // Paid = 100 + 50 - 30 (refund) = 120; the voided 40 is ignored.
        Assert.Equal(120m, AccountEntryCalculator.MaxRefundable(operations));
    }

    [Fact]
    public void MaxRefundable_NeverNegative()
    {
        var operations = new List<PaymentOperation>
        {
            PaymentOperation.Create(
                PaymentOperationId.Create(1), PatientId.Create(1), 0m, 7, DateTime.UtcNow,
                null, false, OperationType.Refund, 100m)
        };

        Assert.Equal(0m, AccountEntryCalculator.MaxRefundable(operations));
    }
}
