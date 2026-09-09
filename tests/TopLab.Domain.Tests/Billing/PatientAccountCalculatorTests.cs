using TopLab.Domain.Billing;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using Xunit;

namespace TopLab.Domain.Tests.Billing;

public class PatientAccountCalculatorTests
{
    private static int _nextId = 1000;

    private static PaymentOperation Op(
        decimal amount,
        decimal? discountAmount = null,
        bool isExtraCharge = false,
        OperationType operationType = OperationType.Payment,
        bool voided = false)
    {
        var op = PaymentOperation.Create(
            PaymentOperationId.Create(_nextId++),
            PatientId.Create(1),
            amount,
            1,
            DateTime.UtcNow,
            discountAmount,
            isExtraCharge,
            operationType);
        if (voided)
            op.Void();
        return op;
    }

    [Fact]
    public void Empty_ReturnsZeros()
    {
        Assert.Equal(0m, PatientAccountCalculator.TotalCharged(Array.Empty<decimal>(), Array.Empty<PaymentOperation>()));
        Assert.Equal(0m, PatientAccountCalculator.TotalPaid(Array.Empty<PaymentOperation>()));
        Assert.Equal(0m, PatientAccountCalculator.Balance(Array.Empty<decimal>(), Array.Empty<PaymentOperation>()));
    }

    [Fact]
    public void PricesOnly_AllCharged()
    {
        var prices = new List<decimal> { 100m, 50m };
        var ops = new List<PaymentOperation>();

        Assert.Equal(150m, PatientAccountCalculator.TotalCharged(prices, ops));
        Assert.Equal(0m, PatientAccountCalculator.TotalPaid(ops));
        Assert.Equal(150m, PatientAccountCalculator.Balance(prices, ops));
    }

    [Fact]
    public void ExtraCharge_AddsToChargedSide()
    {
        var prices = new List<decimal> { 100m, 50m };
        var ops = new List<PaymentOperation> { Op(20m, isExtraCharge: true) };

        Assert.Equal(170m, PatientAccountCalculator.TotalCharged(prices, ops));
        Assert.Equal(0m, PatientAccountCalculator.TotalPaid(ops));
        Assert.Equal(170m, PatientAccountCalculator.Balance(prices, ops));
    }

    [Fact]
    public void VoidedRows_IgnoredEverywhere()
    {
        var prices = new List<decimal> { 100m };
        var ops = new List<PaymentOperation>
        {
            Op(999m, voided: true),
            Op(500m, isExtraCharge: true, voided: true),
            Op(50m, discountAmount: 25m, voided: true),
        };

        Assert.Equal(100m, PatientAccountCalculator.TotalCharged(prices, ops));
        Assert.Equal(0m, PatientAccountCalculator.TotalPaid(ops));
        Assert.Equal(100m, PatientAccountCalculator.Balance(prices, ops));
    }

    [Fact]
    public void Discount_CountsTowardPaidSide()
    {
        var prices = new List<decimal> { 100m };
        var ops = new List<PaymentOperation> { Op(80m, discountAmount: 10m) };

        Assert.Equal(100m, PatientAccountCalculator.TotalCharged(prices, ops));
        Assert.Equal(90m, PatientAccountCalculator.TotalPaid(ops));
        Assert.Equal(10m, PatientAccountCalculator.Balance(prices, ops));
    }

    [Fact]
    public void Correction_ReducesBalance()
    {
        var prices = new List<decimal> { 100m };
        var ops = new List<PaymentOperation>
        {
            Op(60m),
            Op(30m, operationType: OperationType.Correction),
        };

        Assert.Equal(100m, PatientAccountCalculator.TotalCharged(prices, ops));
        Assert.Equal(90m, PatientAccountCalculator.TotalPaid(ops));
        Assert.Equal(10m, PatientAccountCalculator.Balance(prices, ops));
    }

    [Fact]
    public void FullSettlement_NetsToZero_WhenAmountEqualsPriorBalance()
    {
        var prices = new List<decimal> { 100m, 50m };
        var prior = new List<PaymentOperation> { Op(80m, discountAmount: 10m) };
        var priorBalance = PatientAccountCalculator.Balance(prices, prior);
        Assert.Equal(60m, priorBalance);

        var ops = new List<PaymentOperation>(prior) { Op(60m, operationType: OperationType.FullSettlement) };

        Assert.Equal(150m, PatientAccountCalculator.TotalCharged(prices, ops));
        Assert.Equal(150m, PatientAccountCalculator.TotalPaid(ops));
        Assert.Equal(0m, PatientAccountCalculator.Balance(prices, ops));
    }

    [Fact]
    public void Overpayment_AllowsNegativeBalance_Credit()
    {
        var prices = new List<decimal> { 100m };
        var ops = new List<PaymentOperation> { Op(150m) };

        Assert.Equal(100m, PatientAccountCalculator.TotalCharged(prices, ops));
        Assert.Equal(150m, PatientAccountCalculator.TotalPaid(ops));
        Assert.Equal(-50m, PatientAccountCalculator.Balance(prices, ops));
    }

    [Fact]
    public void WorkedExample_MatchesSettledFormula()
    {
        // Plan §2.8 shared worked example: prices 100+50, extra charge 20,
        // payment 80 with discount 10, voided payment 999
        // ⇒ Charged 170, Paid 90, Balance 80.
        var prices = new List<decimal> { 100m, 50m };
        var ops = new List<PaymentOperation>
        {
            Op(20m, isExtraCharge: true),
            Op(80m, discountAmount: 10m),
            Op(999m, voided: true),
        };

        Assert.Equal(170m, PatientAccountCalculator.TotalCharged(prices, ops));
        Assert.Equal(90m, PatientAccountCalculator.TotalPaid(ops));
        Assert.Equal(80m, PatientAccountCalculator.Balance(prices, ops));
    }

    [Fact]
    public void ManualSelectionCharge_SumsIndividualPrices()
    {
        Assert.Equal(120m, PatientAccountCalculator.ManualSelectionCharge(new[] { 100m, 20m }));
        Assert.Equal(0m, PatientAccountCalculator.ManualSelectionCharge(Array.Empty<decimal>()));
    }

    [Fact]
    public void ManualSelectionCharge_RejectsNegativePrice()
    {
        Assert.Throws<ArgumentException>(() => PatientAccountCalculator.ManualSelectionCharge(new[] { 100m, -5m }));
    }

    [Fact]
    public void ProfileSelectionCharge_ReturnsFixedPrice_Only()
    {
        Assert.Equal(200m, PatientAccountCalculator.ProfileSelectionCharge(200m));
        Assert.Equal(0m, PatientAccountCalculator.ProfileSelectionCharge(0m));
    }

    [Fact]
    public void ProfileSelectionCharge_RejectsNegative()
    {
        Assert.Throws<ArgumentException>(() => PatientAccountCalculator.ProfileSelectionCharge(-1m));
    }

    [Fact]
    public void Balance_RemainsTheOnlyAggregateCalculator()
    {
        // The new selection entry points never aggregate payment operations; they feed
        // an order's PriceAtOrderTime so Balance is the only aggregate formula.
        var prices = new List<decimal>
        {
            PatientAccountCalculator.ManualSelectionCharge(new[] { 100m, 50m }),
            PatientAccountCalculator.ProfileSelectionCharge(200m),
        };

        Assert.Equal(350m, PatientAccountCalculator.Balance(prices, Array.Empty<PaymentOperation>()));
    }
}
