using TopLab.Domain.Billing;
using Xunit;

namespace TopLab.Domain.Tests.Billing;

/// <summary>
/// R1 Round 3 B3: the two-press <c>Enter</c> state machine (canonical reference 1.3.1.4.2:
/// "the first press performs the accounting operation for the patient; the second press saves
/// the operation"). <c>ShouldPersist</c> is the only gate a ViewModel may use, so proving it
/// false on the first press proves the first press cannot write to the database.
/// </summary>
public class AccountEntrySessionTests
{
    [Fact]
    public void FirstPress_ComputesAndDisplays_ButNeverPersists()
    {
        var session = new AccountEntrySession { AmountText = "100" };

        var step = session.PressEnter(totalCharged: 200m, alreadyPaid: 0m);

        Assert.True(step.IsValid);
        Assert.False(step.ShouldPersist);
        Assert.Equal(AccountEntryStage.Previewed, step.Stage);
        Assert.NotNull(step.Preview);
        Assert.Equal(100m, step.Preview!.Amount);
        Assert.False(string.IsNullOrWhiteSpace(step.StatusMessage));
    }

    [Fact]
    public void SecondPress_WithUnchangedInputs_Persists()
    {
        var session = new AccountEntrySession { AmountText = "100" };

        var first = session.PressEnter(200m, 0m);
        var second = session.PressEnter(200m, 0m);

        Assert.False(first.ShouldPersist);
        Assert.True(second.ShouldPersist);
        Assert.Equal(AccountEntryStage.ReadyToPersist, second.Stage);
    }

    [Fact]
    public void SecondPress_AfterAnEdit_ReturnsToTheFirstPressState()
    {
        var session = new AccountEntrySession { AmountText = "100" };
        session.PressEnter(200m, 0m);

        session.AmountText = "120";
        var afterEdit = session.PressEnter(200m, 0m);

        Assert.False(afterEdit.ShouldPersist);
        Assert.Equal(AccountEntryStage.Previewed, afterEdit.Stage);
    }

    [Fact]
    public void SecondPress_AfterADiscountEdit_ReturnsToTheFirstPressState()
    {
        var session = new AccountEntrySession { AmountText = "100" };
        session.PressEnter(200m, 0m);

        session.DiscountText = "10";
        var afterEdit = session.PressEnter(200m, 0m);

        Assert.False(afterEdit.ShouldPersist);
    }

    [Fact]
    public void ThirdPress_AfterPersisting_IsAFirstPressAgain()
    {
        var session = new AccountEntrySession { AmountText = "100" };
        session.PressEnter(200m, 0m);
        Assert.True(session.PressEnter(200m, 0m).ShouldPersist);

        session.MarkPersisted();
        var next = session.PressEnter(200m, 0m);

        Assert.False(next.ShouldPersist);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("0")]
    [InlineData("+")]
    public void InvalidInput_ReportsAnErrorAndStaysInTheFirstPressState(string text)
    {
        var session = new AccountEntrySession { AmountText = text };

        var step = session.PressEnter(200m, 0m);

        Assert.False(step.IsValid);
        Assert.False(step.ShouldPersist);
        Assert.False(string.IsNullOrWhiteSpace(step.ErrorMessage));
        Assert.Equal(AccountEntryStage.Idle, step.Stage);
    }

    [Fact]
    public void InvalidInput_ThenValidInput_StillRequiresTwoPresses()
    {
        var session = new AccountEntrySession { AmountText = "abc" };
        session.PressEnter(200m, 0m);

        session.AmountText = "100";
        var first = session.PressEnter(200m, 0m);
        var second = session.PressEnter(200m, 0m);

        Assert.False(first.ShouldPersist);
        Assert.True(second.ShouldPersist);
    }

    // ============================================================
    // Surcharge (1.3.1.4.1 / OD-11)
    // ============================================================

    [Fact]
    public void PlusPrefix_IsReportedAsASurcharge()
    {
        var session = new AccountEntrySession { AmountText = "+30" };

        var step = session.PressEnter(200m, 50m);

        Assert.True(step.IsValid);
        Assert.Equal(AccountEntryKind.Surcharge, step.Kind);
        Assert.Equal(30m, step.Amount);
        Assert.Equal(230m, step.Preview!.TotalChargedAfter);
    }

    [Fact]
    public void SurchargeWithDiscount_IsRejected()
    {
        var session = new AccountEntrySession { AmountText = "+30", DiscountText = "5" };

        var step = session.PressEnter(200m, 50m);

        Assert.False(step.IsValid);
        Assert.Equal(AccountEntryParser.SurchargeWithDiscountMessage, step.ErrorMessage);
    }

    // ============================================================
    // Discount rules preserved
    // ============================================================

    [Fact]
    public void DiscountGreaterThanTheAmount_IsRejected_TheDomainRuleIsUnchanged()
    {
        var session = new AccountEntrySession { AmountText = "100", DiscountText = "150" };

        var step = session.PressEnter(200m, 0m);

        Assert.False(step.IsValid);
    }

    // ============================================================
    // Percentage (1.3.1.3)
    // ============================================================

    [Fact]
    public void ApplyPercentage_WritesTheConvertedAmountIntoTheValueField()
    {
        var session = new AccountEntrySession { PercentageText = "10" };

        var error = session.ApplyPercentage(totalAccountAmount: 200m);

        Assert.Null(error);
        Assert.Equal("20", session.DiscountText);
    }

    [Fact]
    public void ApplyPercentage_OutOfRange_ReturnsTheArabicMessage()
    {
        var session = new AccountEntrySession { PercentageText = "101" };

        var error = session.ApplyPercentage(200m);

        Assert.Equal(AccountEntryParser.InvalidPercentageMessage, error);
        Assert.Null(session.DiscountText);
    }

    [Fact]
    public void EditingTheValueField_ClearsThePercentage()
    {
        var session = new AccountEntrySession { PercentageText = "10" };

        session.DiscountText = "20";

        Assert.Null(session.PercentageText);
    }

    [Fact]
    public void EditingThePercentageField_ClearsTheValue()
    {
        var session = new AccountEntrySession { DiscountText = "20" };

        session.PercentageText = "10";

        Assert.Null(session.DiscountText);
    }
}
