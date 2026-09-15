using TopLab.Domain.Common.Ids;
using TopLab.Domain.SentOutSamples;
using Xunit;

namespace TopLab.Domain.Tests.SentOutSamples;

public class SentOutSampleTests
{
    private static SentOutSample CreateValid(
        decimal costPrice = 50m,
        decimal patientPrice = 100m)
    {
        return SentOutSample.Create(
            SentOutSampleId.Create(1),
            PatientTestId.Create(1),
            ExternalEntityId.Create(1),
            costPrice,
            patientPrice,
            DateTime.UtcNow);
    }

    [Fact]
    public void Create_Valid_SetsProperties()
    {
        var sentAt = DateTime.UtcNow;

        var s = SentOutSample.Create(
            SentOutSampleId.Create(1),
            PatientTestId.Create(2),
            ExternalEntityId.Create(3),
            50m,
            100m,
            sentAt);

        Assert.Equal(50m, s.CostPrice);
        Assert.Equal(100m, s.PatientPrice);
        Assert.Equal(sentAt, s.SentAtUtc);
        Assert.Equal(2, s.PatientTestId.Value);
        Assert.Equal(3, s.ExternalLabEntityId.Value);
    }

    [Fact]
    public void Create_ZeroPrices_Accepted()
    {
        var s = CreateValid(costPrice: 0m, patientPrice: 0m);

        Assert.Equal(0m, s.CostPrice);
        Assert.Equal(0m, s.PatientPrice);
    }

    [Fact]
    public void Create_NegativeCostPrice_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(() => CreateValid(costPrice: -1m));

        Assert.Equal("costPrice", ex.ParamName);
    }

    [Fact]
    public void Create_NegativePatientPrice_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(() => CreateValid(patientPrice: -0.01m));

        Assert.Equal("patientPrice", ex.ParamName);
    }
}
