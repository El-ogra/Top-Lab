using TopLab.Domain.Common.Enums;

namespace TopLab.Domain.Billing;

/// <summary>
/// What the current <c>Enter</c> press did (canonical reference 1.3.1.4.2: "the first press
/// performs the accounting operation for the patient; the second press saves the operation").
/// </summary>
public enum AccountEntryStage
{
    /// <summary>Nothing entered yet, or an edit reset the sequence.</summary>
    Idle = 0,

    /// <summary>The first press computed and displayed the figures; nothing was recorded.</summary>
    Previewed = 1,

    /// <summary>The second press may persist; the caller performs the write.</summary>
    ReadyToPersist = 2
}

/// <summary>
/// Outcome of one <c>Enter</c> press. <see cref="ShouldPersist"/> is the single gate the
/// ViewModels use to decide whether the record-payment command may be sent — the first press
/// can never persist, by construction.
/// </summary>
public sealed record AccountEntryStep(
    AccountEntryStage Stage,
    bool ShouldPersist,
    bool IsValid,
    string? ErrorMessage,
    string? StatusMessage,
    AccountEntryPreview? Preview,
    AccountEntryKind Kind,
    decimal Amount,
    decimal Discount);

/// <summary>
/// The account panel's entry state machine (canonical reference 1.3.1.2, 1.3.1.3, 1.3.1.4,
/// 1.3.1.4.1, 1.3.1.4.2; owner decisions OD-11, OD-12 and the Round 3 percentage decision).
/// <para>
/// Pure and unit-testable: it performs no I/O and knows nothing about WPF. Both payment-entry
/// screens drive one instance, so they cannot diverge. Behaviour:
/// </para>
/// <list type="bullet">
/// <item>the first <c>Enter</c> validates the inputs and returns the resulting figures, and
/// <see cref="ShouldPersist"/> is false;</item>
/// <item>a second <c>Enter</c> with unchanged inputs returns <see cref="ShouldPersist"/> true;</item>
/// <item>any edit to an input resets the sequence to the first-press state;</item>
/// <item>invalid input reports an error and stays in the first-press state.</item>
/// </list>
/// </summary>
public sealed class AccountEntrySession
{
    private string? _amountText;
    private string? _discountText;
    private string? _percentageText;

    private string? _validatedAmountText;
    private string? _validatedDiscountText;
    private string? _validatedPercentageText;

    /// <summary>
    /// The percentage actually converted into the discount field by <see cref="ApplyPercentage"/>.
    /// The percentage text is cleared once it has been converted, so this value is what the D-01
    /// availability check reads on that same Enter press and on the second (persisting) press.
    /// </summary>
    private decimal _appliedPercentage;

    /// <summary>Paid-amount text. Editing it resets the two-press sequence.</summary>
    public string? AmountText
    {
        get => _amountText;
        set => SetField(ref _amountText, value);
    }

    /// <summary>Value-discount text. Editing it resets the sequence and clears the percentage.</summary>
    public string? DiscountText
    {
        get => _discountText;
        set
        {
            if (SetField(ref _discountText, value))
            {
                // 1.3.1.2 / 1.3.1.3: the value and the percentage are never applied together.
                _percentageText = null;
                _appliedPercentage = 0m;
            }
        }
    }

    /// <summary>Percentage-discount text. Editing it resets the sequence and clears the value.</summary>
    public string? PercentageText
    {
        get => _percentageText;
        set
        {
            if (SetField(ref _percentageText, value))
            {
                _discountText = null;
            }
        }
    }

    /// <summary>Current stage of the two-press sequence.</summary>
    public AccountEntryStage Stage { get; private set; } = AccountEntryStage.Idle;

    /// <summary>Resets everything, including the stage (called after a successful save).</summary>
    public void Reset()
    {
        _amountText = null;
        _discountText = null;
        _percentageText = null;
        _validatedAmountText = null;
        _validatedDiscountText = null;
        _validatedPercentageText = null;
        _appliedPercentage = 0m;
        Stage = AccountEntryStage.Idle;
    }

    /// <summary>
    /// Applies a percentage to the patient's TOTAL account amount and writes the resulting
    /// amount into the value field, as the Round 3 decision requires. Returns the Arabic error
    /// message when the percentage is out of range.
    /// </summary>
    public string? ApplyPercentage(decimal totalAccountAmount)
    {
        var parsed = AccountEntryParser.ParsePercentage(_percentageText);
        if (!parsed.IsValid)
        {
            return parsed.ErrorMessage;
        }

        if (parsed.Value <= 0m)
        {
            return null;
        }

        var amount = AccountEntryCalculator.DiscountAmountFromPercentage(totalAccountAmount, parsed.Value);

        // Round 4 fix: the converted amount is written first, then the percentage field is
        // cleared through the BACKING FIELD. Going through the PercentageText setter would
        // clear _discountText as well (the mutual-exclusion rule for USER edits) and silently
        // throw the converted amount away — which is exactly what the Round-3 build did.
        _discountText = amount.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);
        _percentageText = null;

        // The field is now empty, so PressEnter cannot read the percentage from the text any
        // more. Keep the value that was actually applied so the D-01 availability check still
        // runs on the same Enter press (and on the second press, which must persist).
        _appliedPercentage = parsed.Value;
        Stage = AccountEntryStage.Idle;
        return null;
    }

    /// <summary>
    /// U1-GAP-004: derives the percentage that the entered discount amount represents,
    /// against the patient's total account amount, and writes it into the percentage field
    /// as a read-only projection (amount is authoritative). Returns null on success.
    /// </summary>
    public string? ApplyDiscountAmount(decimal totalAccountAmount)
    {
        var discount = AccountEntryParser.ParseDiscount(_discountText);
        if (!discount.IsValid)
        {
            return discount.ErrorMessage;
        }

        if (discount.Value <= 0m)
        {
            _percentageText = null;
            _appliedPercentage = 0m;
            Stage = AccountEntryStage.Idle;
            return null;
        }

        var percentage = AccountEntryCalculator.DiscountPercentageFromAmount(totalAccountAmount, discount.Value);

        // Write through the backing field so the mutual-exclusion rule for USER edits
        // (which would clear _discountText) does not discard the derived value.
        _percentageText = percentage.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);
        _appliedPercentage = percentage;
        Stage = AccountEntryStage.Idle;
        return null;
    }
    /// <summary>
    /// One <c>Enter</c> press. <paramref name="totalCharged"/> and
    /// <paramref name="alreadyPaid"/> come from the loaded account (never from client-side
    /// arithmetic on other figures) and are used only to display the resulting figures.
    /// </summary>
    public AccountEntryStep PressEnter(decimal totalCharged, decimal alreadyPaid)
    {
        var amount = AccountEntryParser.ParsePaidAmount(_amountText);
        if (!amount.IsValid)
        {
            Stage = AccountEntryStage.Idle;
            return Invalid(amount.ErrorMessage!, AccountEntryKind.Payment, 0m, 0m);
        }

        var discount = AccountEntryParser.ParseDiscount(_discountText);
        if (!discount.IsValid)
        {
            Stage = AccountEntryStage.Idle;
            return Invalid(discount.ErrorMessage!, amount.Kind, amount.Value, 0m);
        }

        var percentage = AccountEntryParser.ParsePercentage(_percentageText);
        if (!percentage.IsValid)
        {
            Stage = AccountEntryStage.Idle;
            return Invalid(percentage.ErrorMessage!, amount.Kind, amount.Value, 0m);
        }

        // A percentage that was already converted into the discount field no longer has any text
        // to parse, so the effective percentage is whichever of the two is set.
        var effectivePercentage = percentage.Value > 0m ? percentage.Value : _appliedPercentage;

        // 1.3.1.4.1: a surcharge carries no discount.
        if (amount.Kind == AccountEntryKind.Surcharge && discount.Value > 0m)
        {
            Stage = AccountEntryStage.Idle;
            return Invalid(AccountEntryParser.SurchargeWithDiscountMessage, amount.Kind, amount.Value, 0m);
        }

        // D-01 (owner decision, option 4 of card D-01): the percentage discount is available
        // ONLY when the payment settles the whole remaining balance — that is, when
        // amount + percentage-discount EQUALS (TotalCharged - net TotalPaid). This availability
        // check runs BEFORE the Domain rule below, so a percentage that fails it is reported as
        // unavailable rather than as an over-discount.
        if (effectivePercentage > 0m)
        {
            var remaining = AccountEntryCalculator.Round2(totalCharged - alreadyPaid);
            if (remaining <= 0m)
            {
                Stage = AccountEntryStage.Idle;
                return Invalid(
                    AccountEntryParser.PercentageWithoutRemainingBalanceMessage,
                    amount.Kind, amount.Value, discount.Value);
            }

            var settled = AccountEntryCalculator.Round2(amount.Value + discount.Value);
            if (settled != remaining)
            {
                Stage = AccountEntryStage.Idle;
                return Invalid(
                    string.Format(
                        System.Globalization.CultureInfo.InvariantCulture,
                        AccountEntryParser.PercentageRequiresFullSettlementMessage,
                        remaining.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture)),
                    amount.Kind, amount.Value, discount.Value);
            }
        }

        // The domain rule stays untouched: a discount can never exceed the operation amount.
        // It is the SECOND check, exactly as the owner required.
        if (discount.Value > amount.Value)
        {
            Stage = AccountEntryStage.Idle;
            return Invalid("الخصم لا يمكن أن يتجاوز مبلغ الدفع.", amount.Kind, amount.Value, discount.Value);
        }

        var preview = AccountEntryCalculator.Preview(
            totalCharged, alreadyPaid, amount.Value, discount.Value, amount.Kind);

        var isSecondPress = Stage == AccountEntryStage.Previewed
            && _validatedAmountText == _amountText
            && _validatedDiscountText == _discountText
            && _validatedPercentageText == _percentageText;

        if (isSecondPress)
        {
            Stage = AccountEntryStage.ReadyToPersist;
            return new AccountEntryStep(
                AccountEntryStage.ReadyToPersist,
                ShouldPersist: true,
                IsValid: true,
                ErrorMessage: null,
                StatusMessage: null,
                Preview: preview,
                Kind: amount.Kind,
                Amount: amount.Value,
                Discount: discount.Value);
        }

        _validatedAmountText = _amountText;
        _validatedDiscountText = _discountText;
        _validatedPercentageText = _percentageText;
        Stage = AccountEntryStage.Previewed;

        var status = amount.Kind == AccountEntryKind.Surcharge
            ? $"مبلغ إضافي: {preview.Amount:0.##} — إجمالي التحاليل بعد الإضافة: {preview.TotalChargedAfter:0.##} — الباقي: {preview.BalanceAfter:0.##}. اضغط Enter مرة أخرى للحفظ."
            : $"المدفوع: {preview.Amount:0.##} — الخصم: {preview.Discount:0.##} — الباقي بعد العملية: {preview.BalanceAfter:0.##}. اضغط Enter مرة أخرى للحفظ.";

        return new AccountEntryStep(
            AccountEntryStage.Previewed,
            ShouldPersist: false,
            IsValid: true,
            ErrorMessage: null,
            StatusMessage: status,
            Preview: preview,
            Kind: amount.Kind,
            Amount: amount.Value,
            Discount: discount.Value);
    }

    /// <summary>After a successful save the panel returns to its idle state.</summary>
    public void MarkPersisted() => Reset();

    private static AccountEntryStep Invalid(string message, AccountEntryKind kind, decimal amount, decimal discount)
        => new(
            AccountEntryStage.Idle,
            ShouldPersist: false,
            IsValid: false,
            ErrorMessage: message,
            StatusMessage: null,
            Preview: null,
            Kind: kind,
            Amount: amount,
            Discount: discount);

    private bool SetField(ref string? field, string? value)
    {
        if (string.Equals(field, value, StringComparison.Ordinal))
        {
            return false;
        }

        field = value;
        // Any edit returns the sequence to its first-press state (1.3.1.4.2).
        Stage = AccountEntryStage.Idle;
        _validatedAmountText = null;
        _validatedDiscountText = null;
        _validatedPercentageText = null;
        return true;
    }
}
