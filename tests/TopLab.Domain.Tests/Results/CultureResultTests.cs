using TopLab.Domain.Common.Ids;
using TopLab.Domain.Results;
using Xunit;

namespace TopLab.Domain.Tests.Results;

public class CultureResultTests
{
    [Fact]
    public void Update_RoundTrips_AllFields()
    {
        var result = new CultureResult(PatientTestId.Create(1));

        result.Update("Sample", "A", "B", "C", "Aerobic", "100 CFU");

        Assert.Equal("Sample", result.Sample);
        Assert.Equal("A", result.OrganismA);
        Assert.Equal("B", result.OrganismB);
        Assert.Equal("C", result.OrganismC);
        Assert.Equal("Aerobic", result.CultureCondition);
        Assert.Equal("100 CFU", result.ColonyCount);
    }

    [Fact]
    public void Update_WhitespaceOnlyValues_NormalizesToNull()
    {
        var result = new CultureResult(PatientTestId.Create(1));

        result.Update(" ", "\t", "\r\n", null, "  ", "");

        Assert.Null(result.Sample);
        Assert.Null(result.OrganismA);
        Assert.Null(result.OrganismB);
        Assert.Null(result.OrganismC);
        Assert.Null(result.CultureCondition);
        Assert.Null(result.ColonyCount);
    }

    [Fact]
    public void Update_TrimsValues()
    {
        var result = new CultureResult(PatientTestId.Create(1));

        result.Update("  urine  ", "  E. coli  ", null, null, "  aerobic  ", "  10^5  ");

        Assert.Equal("urine", result.Sample);
        Assert.Equal("E. coli", result.OrganismA);
        Assert.Equal("aerobic", result.CultureCondition);
        Assert.Equal("10^5", result.ColonyCount);
    }
}
