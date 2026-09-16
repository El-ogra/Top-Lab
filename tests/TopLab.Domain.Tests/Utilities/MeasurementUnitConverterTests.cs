using TopLab.Domain.Utilities;
using Xunit;

namespace TopLab.Domain.Tests.Utilities;

public class MeasurementUnitConverterTests
{
    [Theory]
    [InlineData(1, "g", "mg", 1000)]
    [InlineData(1000, "mg", "g", 1)]
    [InlineData(1, "g", "µg", 1000000)]
    [InlineData(1000000, "µg", "g", 1)]
    [InlineData(1, "mg", "µg", 1000)]
    [InlineData(1000, "µg", "mg", 1)]
    [InlineData(2, "kg", "g", 2000)]
    [InlineData(2000, "g", "kg", 2)]
    [InlineData(1, "g/L", "mg/dL", 100)]
    [InlineData(100, "mg/dL", "g/L", 1)]
    [InlineData(1, "mmol/L", "µmol/L", 1000)]
    [InlineData(1000, "µmol/L", "mmol/L", 1)]
    [InlineData(5, "ng/mL", "µg/L", 5)]
    [InlineData(5, "µg/L", "ng/mL", 5)]
    [InlineData(1, "L", "mL", 1000)]
    [InlineData(1000, "mL", "L", 1)]
    [InlineData(2, "mL", "µL", 2000)]
    [InlineData(2000, "µL", "mL", 2)]
    public void EveryPair_NumberForNumber(decimal value, string from, string to, decimal expected)
    {
        Assert.Equal(expected, MeasurementUnitConverter.Convert(value, from, to));
    }

    [Fact]
    public void SameUnit_ReturnsValueUnchanged()
    {
        Assert.Equal(12.5m, MeasurementUnitConverter.Convert(12.5m, "mg", "mg"));
    }

    [Fact]
    public void UnknownPair_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(() => MeasurementUnitConverter.Convert(1m, "mg", "IU"));
        Assert.Contains("Unsupported unit pair", ex.Message);
    }

    [Fact]
    public void BlankUnit_Throws()
    {
        Assert.Throws<ArgumentException>(() => MeasurementUnitConverter.Convert(1m, "", "mg"));
        Assert.Throws<ArgumentException>(() => MeasurementUnitConverter.Convert(1m, "mg", "  "));
    }
}
