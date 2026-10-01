using TopLab.Domain.Common.Ids;
using TopLab.Domain.Results;
using Xunit;

namespace TopLab.Domain.Tests.Results;

/// <summary>W-02 S9 (WP-14): microscopy field rules.</summary>
public class CultureMicroscopyTests
{
    [Fact]
    public void CultureMicroscopy_PersistsAllNineFields()
    {
        var m = new CultureMicroscopy(
            PatientTestId.Create(11), "+", "++", "+++", "oxalate", "candida",
            "o1", "o2", "o3", true);

        Assert.Equal("+", m.PusCells);
        Assert.Equal("++", m.RedBloodCells);
        Assert.Equal("+++", m.EpithelialCells);
        Assert.Equal("oxalate", m.Crystals);
        Assert.Equal("candida", m.Fungi);
        Assert.Equal("o1", m.OthersOne);
        Assert.Equal("o2", m.OthersTwo);
        Assert.Equal("o3", m.OthersThree);
        Assert.True(m.IsDirect);
    }

    [Fact]
    public void CultureMicroscopy_NormalizesWhitespace()
    {
        var m = new CultureMicroscopy(PatientTestId.Create(11), pusCells: "  ++  ");

        Assert.Equal("++", m.PusCells);
    }

    [Fact]
    public void CultureMicroscopy_RejectsFieldOverTwentyChars()
    {
        Assert.Throws<ArgumentException>(() =>
            new CultureMicroscopy(PatientTestId.Create(11), pusCells: new string('x', 21)));
    }

    [Fact]
    public void CultureAntibioticResult_SetInhibitionZone_RoundTrips()
    {
        var row = CultureAntibioticResult.Create(
            CultureAntibioticResultId.Create(1), PatientTestId.Create(11), AntibioticId.Create(2), null);

        row.SetInhibitionZone(18.5m);

        Assert.Equal(18.5m, row.InhibitionZoneMm);
    }

    [Fact]
    public void CultureAntibioticResult_SetInhibitionZone_OutOfRange_IsRejected()
    {
        var row = CultureAntibioticResult.Create(
            CultureAntibioticResultId.Create(1), PatientTestId.Create(11), AntibioticId.Create(2), null);

        Assert.Throws<ArgumentException>(() => row.SetInhibitionZone(101m));
    }
}
