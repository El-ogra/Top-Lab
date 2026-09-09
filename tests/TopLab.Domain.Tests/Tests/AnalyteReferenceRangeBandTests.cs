using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Domain.Tests.Tests;

public class AnalyteReferenceRangeBandTests
{
    private static AnalyteReferenceRangeBand CreateBand(
        AnalyteReferenceRangeBandId id,
        Sex? sex = null,
        AgeUnit ageUnit = AgeUnit.Day,
        int ageMin = 1,
        int ageMax = 60)
    {
        return AnalyteReferenceRangeBand.Create(id, AnalyteReferenceRangeId.Create(1), ageUnit, ageMin, ageMax, 0m, 100m, sex);
    }

    [Theory]
    [InlineData(15)]
    [InlineData(35)]
    public void Matches_AgeWithinRange_ReturnsTrue(int ageValue)
    {
        var band = CreateBand(AnalyteReferenceRangeBandId.Create(1), ageMin: 1, ageMax: 60);

        Assert.True(band.Matches(Sex.Male, AgeUnit.Day, ageValue));
    }

    [Fact]
    public void Matches_Boundaries_ReturnsTrue()
    {
        var band = CreateBand(AnalyteReferenceRangeBandId.Create(1), ageMin: 1, ageMax: 60);

        Assert.True(band.Matches(Sex.Male, AgeUnit.Day, 1));
        Assert.True(band.Matches(Sex.Male, AgeUnit.Day, 60));
    }

    [Fact]
    public void Matches_UnitMismatch_ReturnsFalse()
    {
        var band = CreateBand(AnalyteReferenceRangeBandId.Create(1), ageUnit: AgeUnit.Day);

        Assert.False(band.Matches(Sex.Male, AgeUnit.Year, 30));
    }

    [Fact]
    public void Matches_SexMismatch_ReturnsFalse()
    {
        var band = CreateBand(AnalyteReferenceRangeBandId.Create(1), sex: Sex.Female);

        Assert.False(band.Matches(Sex.Male, AgeUnit.Day, 30));
    }

    [Fact]
    public void Matches_NullSexBand_MatchesAnySex()
    {
        var band = CreateBand(AnalyteReferenceRangeBandId.Create(1), sex: null);

        Assert.True(band.Matches(Sex.Male, AgeUnit.Day, 30));
        Assert.True(band.Matches(Sex.Female, AgeUnit.Day, 30));
    }

    [Fact]
    public void Guard_Rejects_InvalidAges()
    {
        Assert.Throws<ArgumentException>(() => CreateBand(AnalyteReferenceRangeBandId.Create(1), ageMin: -1));
        Assert.Throws<ArgumentException>(() => CreateBand(AnalyteReferenceRangeBandId.Create(1), ageMin: 10, ageMax: 5));
    }

    [Fact]
    public void Guard_Rejects_MinGreaterThanMax()
    {
        Assert.Throws<ArgumentException>(() => AnalyteReferenceRangeBand.Create(
            AnalyteReferenceRangeBandId.Create(1),
            AnalyteReferenceRangeId.Create(1),
            AgeUnit.Year,
            0,
            100,
            10m,
            1m));
    }

    [Fact]
    public void Guard_Rejects_OverlongComments()
    {
        var longComment = new string('x', 501);

        Assert.Throws<ArgumentException>(() => AnalyteReferenceRangeBand.Create(
            AnalyteReferenceRangeBandId.Create(1),
            AnalyteReferenceRangeId.Create(1),
            AgeUnit.Year,
            0,
            100,
            0m,
            10m,
            null,
            longComment));
    }
}