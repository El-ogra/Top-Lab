using TopLab.Domain.Common.Ids;
using TopLab.Domain.SentOutSamples;
using Xunit;

namespace TopLab.Domain.Tests.SentOutSamples;

public class SentOutSamplePaymentTests
{
    private static SentOutSamplePayment CreateValid(decimal amountPaid = 50m)
    {
        return SentOutSamplePayment.Create(
            SentOutSamplePaymentId.Create(1),
            SentOutSampleId.Create(1),
            amountPaid,
            DateTime.UtcNow,
            performedByUserId: 1);
    }

    [Fact]
    public void Create_PositiveAmount_SetsProperties()
    {
        var paidAt = DateTime.UtcNow;

        var p = SentOutSamplePayment.Create(
            SentOutSamplePaymentId.Create(1),
            SentOutSampleId.Create(2),
            75m,
            paidAt,
            performedByUserId: 7);

        Assert.Equal(75m, p.AmountPaid);
        Assert.Equal(paidAt, p.PaidAtUtc);
        Assert.Equal(7, p.PerformedByUserId);
        Assert.Equal(2, p.SentOutSampleId.Value);
    }

    [Fact]
    public void Create_ZeroAmount_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(() => CreateValid(amountPaid: 0m));

        Assert.Equal("amountPaid", ex.ParamName);
    }

    [Fact]
    public void Create_NegativeAmount_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(() => CreateValid(amountPaid: -10m));

        Assert.Equal("amountPaid", ex.ParamName);
    }
}
