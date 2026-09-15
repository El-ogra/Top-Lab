using TopLab.Domain.Common.Ids;
using TopLab.Domain.SentOutSamples;
using Xunit;

namespace TopLab.Domain.Tests.SentOutSamples;

public class SentOutAccountCalculatorTests
{
    private static int _nextId = 5000;

    private static SentOutSample Sample(decimal costPrice)
    {
        return SentOutSample.Create(
            SentOutSampleId.Create(_nextId++),
            PatientTestId.Create(_nextId++),
            ExternalEntityId.Create(1),
            costPrice,
            patientPrice: 100m,
            DateTime.UtcNow);
    }

    private static SentOutSamplePayment Pay(SentOutSampleId sampleId, decimal amount)
    {
        return SentOutSamplePayment.Create(
            SentOutSamplePaymentId.Create(_nextId++),
            sampleId,
            amount,
            DateTime.UtcNow,
            performedByUserId: 1);
    }

    [Fact]
    public void Empty_ReturnsZeros_NotSettled()
    {
        Assert.Equal(0m, SentOutAccountCalculator.TotalCost(Array.Empty<SentOutSample>()));
        Assert.Equal(0m, SentOutAccountCalculator.TotalPaid(Array.Empty<SentOutSamplePayment>()));
        Assert.Equal(0m, SentOutAccountCalculator.Remaining(0m, 0m));
        Assert.True(SentOutAccountCalculator.IsFullySettled(0m, 0m));
    }

    [Fact]
    public void PartialSettlement_RemainingPositive_NotSettled()
    {
        var s = Sample(100m);
        var payments = new List<SentOutSamplePayment> { Pay(s.Id, 40m) };

        var cost = SentOutAccountCalculator.TotalCost(new[] { s });
        var paid = SentOutAccountCalculator.TotalPaid(payments);

        Assert.Equal(100m, cost);
        Assert.Equal(40m, paid);
        Assert.Equal(60m, SentOutAccountCalculator.Remaining(cost, paid));
        Assert.False(SentOutAccountCalculator.IsFullySettled(cost, paid));
    }

    [Fact]
    public void ExactSettlement_RemainingZero_Settled()
    {
        var s = Sample(100m);
        var payments = new List<SentOutSamplePayment> { Pay(s.Id, 60m), Pay(s.Id, 40m) };

        var cost = SentOutAccountCalculator.TotalCost(new[] { s });
        var paid = SentOutAccountCalculator.TotalPaid(payments);

        Assert.Equal(100m, paid);
        Assert.Equal(0m, SentOutAccountCalculator.Remaining(cost, paid));
        Assert.True(SentOutAccountCalculator.IsFullySettled(cost, paid));
    }

    [Fact]
    public void OverPayment_RemainingNegative_Settled()
    {
        var s = Sample(100m);
        var payments = new List<SentOutSamplePayment> { Pay(s.Id, 120m) };

        var cost = SentOutAccountCalculator.TotalCost(new[] { s });
        var paid = SentOutAccountCalculator.TotalPaid(payments);

        Assert.Equal(-20m, SentOutAccountCalculator.Remaining(cost, paid));
        Assert.True(SentOutAccountCalculator.IsFullySettled(cost, paid));
    }

    [Fact]
    public void MultipleSamples_SumsCost()
    {
        var samples = new List<SentOutSample> { Sample(100m), Sample(50m) };

        Assert.Equal(150m, SentOutAccountCalculator.TotalCost(samples));
    }
}
