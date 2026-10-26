using TopLab.Domain.Common.Enums;

namespace TopLab.Domain.Billing;

/// <summary>
/// What a parsed paid-amount entry means (canonical reference 1.3.1.4.1).
/// </summary>
public enum AccountEntryKind
{
    /// <summary>An unprefixed amount is the payment itself.</summary>
    Payment = 0,

    /// <summary>An amount prefixed with <c>+</c> is an addition to the total of the tests.</summary>
    Surcharge = 1
}

/// <summary>
/// Result of parsing one account-entry field. A failure carries the Arabic message the
/// screens show, so the wording lives in exactly one place (SD-17).
/// </summary>
public sealed record AccountEntryParseResult(bool IsValid, decimal Value, AccountEntryKind Kind, string? ErrorMessage)
{
    public static AccountEntryParseResult Ok(decimal value, AccountEntryKind kind = AccountEntryKind.Payment)
        => new(true, value, kind, null);

    public static AccountEntryParseResult Fail(string message)
        => new(false, 0m, AccountEntryKind.Payment, message);
}

/// <summary>
/// Pure parsing for the account-panel entry fields (canonical reference 1.3.1.2, 1.3.1.3,
/// 1.3.1.4, 1.3.1.4.1; owner decisions OD-11 and the Round 3 percentage decision).
/// <para>
/// This type exists so that every payment-entry screen parses identically. It performs no
/// I/O, holds no state, and introduces no schema change and no stored percentage.
/// </para>
/// </summary>
public static class AccountEntryParser
{
    /// <summary>Arabic message shown when a paid amount cannot be read as a number.</summary>
    public const string InvalidAmountMessage = "قيمة المدفوع غير صالحة.";

    /// <summary>Arabic message for a bare <c>+</c>, <c>+0</c> or a negative paid amount.</summary>
    public const string InvalidSurchargeMessage = "قيمة المبلغ الإضافي غير صالحة.";

    /// <summary>Arabic message for a <c>+</c> amount combined with a discount (1.3.1.4.1).</summary>
    public const string SurchargeWithDiscountMessage = "لا يمكن إضافة مبلغ إضافي مع خصم.";

    /// <summary>Arabic message for a discount value that cannot be read or is negative.</summary>
    public const string InvalidDiscountMessage = "قيمة الخصم غير صالحة.";

    /// <summary>Arabic message for a percentage outside the open range (0, 100].</summary>
    public const string InvalidPercentageMessage = "نسبة الخصم يجب أن تكون أكبر من صفر ولا تتجاوز 100.";

    /// <summary>
    /// D-01 (owner decision): the percentage is available only when the payment settles the whole
    /// remaining balance. This is the message shown when `amount + percentage discount` does not
    /// equal the remaining balance; `{0}` is the remaining balance.
    /// </summary>
    public const string PercentageRequiresFullSettlementMessage =
        "النسبة متاحة فقط عند سداد كامل المتبقي: يجب أن يكون المدفوع + الخصم = {0}.";

    /// <summary>D-01: there is nothing left to settle, so a percentage cannot be applied at all.</summary>
    public const string PercentageWithoutRemainingBalanceMessage =
        "لا يوجد مبلغ متبقٍ في الحساب لتطبيق نسبة عليه.";

    private const char SurchargePrefix = '+';
    private const char MinusSign = '-';
    private const char ArabicDecimalSeparator = '\u066B';

    /// <summary>
    /// Parses the paid-amount field. A leading <c>+</c> (after trimming) selects the surcharge
    /// convention (1.3.1.4.1 / OD-11); anything else is an ordinary payment.
    /// </summary>
    public static AccountEntryParseResult ParsePaidAmount(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return AccountEntryParseResult.Fail(InvalidAmountMessage);
        }

        var trimmed = text.Trim();
        var kind = AccountEntryKind.Payment;
        if (trimmed.Length > 0 && trimmed[0] == SurchargePrefix)
        {
            kind = AccountEntryKind.Surcharge;
            trimmed = trimmed[1..].Trim();
        }

        if (!TryParseDecimal(trimmed, out var value))
        {
            return AccountEntryParseResult.Fail(kind == AccountEntryKind.Surcharge ? InvalidSurchargeMessage : InvalidAmountMessage);
        }

        if (value <= 0)
        {
            return AccountEntryParseResult.Fail(kind == AccountEntryKind.Surcharge ? InvalidSurchargeMessage : InvalidAmountMessage);
        }

        return AccountEntryParseResult.Ok(value, kind);
    }

    /// <summary>
    /// Parses the value-discount field. An empty or whitespace entry means "no discount"
    /// and is reported as a valid zero.
    /// </summary>
    public static AccountEntryParseResult ParseDiscount(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return AccountEntryParseResult.Ok(0m);
        }

        if (!TryParseDecimal(text.Trim(), out var value) || value < 0)
        {
            return AccountEntryParseResult.Fail(InvalidDiscountMessage);
        }

        return AccountEntryParseResult.Ok(value);
    }

    /// <summary>
    /// Parses the percentage-discount field (1.3.1.3). An empty entry means "no percentage".
    /// The valid range is greater than 0 and at most 100.
    /// </summary>
    public static AccountEntryParseResult ParsePercentage(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return AccountEntryParseResult.Ok(0m);
        }

        if (!TryParseDecimal(text.Trim().TrimEnd('%'), out var value) || value <= 0 || value > 100m)
        {
            return AccountEntryParseResult.Fail(InvalidPercentageMessage);
        }

        return AccountEntryParseResult.Ok(value);
    }

    /// <summary>
    /// Parses a refund amount (1.3.2.5). Must be greater than zero; the upper bound against
    /// the patient's net paid total is enforced by the refund handler, not here.
    /// </summary>
    public static AccountEntryParseResult ParseRefundAmount(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return AccountEntryParseResult.Fail(InvalidAmountMessage);
        }

        if (!TryParseDecimal(text.Trim(), out var value) || value <= 0)
        {
            return AccountEntryParseResult.Fail(InvalidAmountMessage);
        }

        return AccountEntryParseResult.Ok(value);
    }

    /// <summary>
    /// Reads a decimal from user text. Latin digits, Arabic-Indic digits (U+0660–U+0669) and
    /// their extended form (U+06F0–U+06F9) are all accepted, as is the Arabic decimal
    /// separator, because the operator may be typing on an Arabic keyboard.
    /// </summary>
    public static bool TryParseDecimal(string? text, out decimal value)
    {
        value = 0m;
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        var normalized = NormalizeDigits(text.Trim()).Replace(ArabicDecimalSeparator, '.');
        if (normalized.Length == 0)
        {
            return false;
        }

        return decimal.TryParse(
            normalized,
            System.Globalization.NumberStyles.Number,
            System.Globalization.CultureInfo.InvariantCulture,
            out value);
    }

    /// <summary>
    /// Maps Arabic-Indic and extended Arabic-Indic digits onto ASCII digits so a single
    /// invariant parser can read every keyboard.
    /// </summary>
    public static string NormalizeDigits(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        var buffer = new char[text.Length];
        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];
            buffer[i] = c switch
            {
                >= '\u0660' and <= '\u0669' => (char)(c - '\u0660' + '0'),
                >= '\u06F0' and <= '\u06F9' => (char)(c - '\u06F0' + '0'),
                _ => c
            };
        }

        return new string(buffer);
    }

    /// <summary>True when the entry carries the surcharge prefix and nothing else.</summary>
    public static bool IsSurchargePrefixOnly(string? text)
        => !string.IsNullOrWhiteSpace(text)
           && text.Trim().Length == 1
           && text.Trim()[0] == SurchargePrefix;

    /// <summary>True when the entry starts with the surcharge prefix.</summary>
    public static bool HasSurchargePrefix(string? text)
        => !string.IsNullOrWhiteSpace(text) && text.Trim()[0] == SurchargePrefix;

    /// <summary>True when the entry starts with a minus sign (always rejected).</summary>
    public static bool HasMinusSign(string? text)
        => !string.IsNullOrWhiteSpace(text) && text.Trim()[0] == MinusSign;
}

/// <summary>
/// Pure calculations behind the account panel's first <c>Enter</c> press (1.3.1.4.2) and the
/// percentage field (1.3.1.3). No I/O, no state, no schema change.
/// </summary>
public static class AccountEntryCalculator
{
    /// <summary>
    /// Converts a discount percentage into an amount. The percentage is always calculated
    /// from the patient's TOTAL account amount (the total charged value of all the patient's
    /// tests), per the owner's Round 3 decision, and rounded to two decimals away from zero.
    /// </summary>
    public static decimal DiscountAmountFromPercentage(decimal totalAccountAmount, decimal percentage)
    {
        if (percentage <= 0m || percentage > 100m)
        {
            throw new ArgumentException("Percentage must be greater than 0 and at most 100.", nameof(percentage));
        }

        if (totalAccountAmount <= 0m)
        {
            return 0m;
        }

        return Round2(totalAccountAmount * percentage / 100m);
    }

    /// <summary>Rounds to two decimals away from zero (the money convention of the panel).</summary>
    public static decimal Round2(decimal value)
        => Math.Round(value, 2, MidpointRounding.AwayFromZero);

    /// <summary>
    /// The figures the first <c>Enter</c> press shows in the status line, before anything is
    /// recorded: the amount about to be paid, the discount about to be applied, and the
    /// balance that would remain.
    /// </summary>
    public static AccountEntryPreview Preview(
        decimal totalCharged,
        decimal alreadyPaid,
        decimal amount,
        decimal discount,
        AccountEntryKind kind)
    {
        if (kind == AccountEntryKind.Surcharge)
        {
            // A surcharge raises the charged side (1.3.1.4.1) and carries no discount.
            var chargedAfter = totalCharged + amount;
            return new AccountEntryPreview(0m, 0m, chargedAfter - alreadyPaid, chargedAfter);
        }

        var paidAfter = alreadyPaid + amount + discount;
        return new AccountEntryPreview(amount, discount, totalCharged - paidAfter, totalCharged);
    }

    /// <summary>
    /// The highest refund the patient may still receive: the net CASH actually paid after
    /// earlier refunds and voided operations, floored at zero.
    /// <para>
    /// Round 5 Part B: this used to delegate to <c>PatientAccountCalculator.TotalPaid</c>, which
    /// adds <c>DiscountAmount</c> — so a discount counted as refundable money. The ceiling is now
    /// <see cref="PatientAccountCalculator.CashRefundable"/>, which sums only the cash
    /// <c>Amount</c> of non-voided, non-extra-charge, non-refund operations.
    /// </para>
    /// </summary>
    public static decimal MaxRefundable(IReadOnlyList<PaymentOperation> operations)
        => PatientAccountCalculator.CashRefundable(operations);

    /// <summary>Human-readable type name used by the confirmation/status lines.</summary>
    public static string Describe(OperationType operationType) => operationType switch
    {
        OperationType.Refund => "استرداد",
        OperationType.Correction => "تعديل",
        OperationType.FullSettlement => "تسوية كاملة",
        _ => "دفع"
    };
}

/// <summary>Figures shown by the first <c>Enter</c> press; nothing is persisted to produce them.</summary>
public sealed record AccountEntryPreview(
    decimal Amount,
    decimal Discount,
    decimal BalanceAfter,
    decimal TotalChargedAfter);
