using TopLab.Application.Features.ResultsEntry.Common;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Application.Tests.Features.ResultsEntry;

public class ResultFlagComputerBandTests
{
    private static int _nextId = 10000;

    private static AnalyteReferenceRangeBand Band(
        Sex? sex,
        AgeUnit ageUnit,
        int ageMin,
        int ageMax,
        decimal min,
        decimal max,
        int? id = null)
    {
        return AnalyteReferenceRangeBand.Create(
            AnalyteReferenceRangeBandId.Create(id ?? _nextId++),
            AnalyteReferenceRangeId.Create(1),
            ageUnit,
            ageMin,
            ageMax,
            min,
            max,
            sex);
    }

    [Theory]
    [InlineData("3", ResultFlag.Low)]
    [InlineData("5", ResultFlag.Normal)]
    [InlineData("12", ResultFlag.High)]
    public void Compute_LowNormalHigh_MatchesReferenceRangeBehavior(string value, ResultFlag expected)
    {
        var bands = new List<AnalyteReferenceRangeBand> { Band(null, AgeUnit.Year, 0, 100, 4m, 10m) };
        Assert.Equal(expected, ResultFlagComputer.Compute(value, Sex.Male, AgeUnit.Year, 30, bands));
    }

    [Theory]
    [InlineData("4", ResultFlag.Normal)]
    [InlineData("10", ResultFlag.Normal)]
    public void Compute_Boundaries_AreInclusive(string value, ResultFlag expected)
    {
        var bands = new List<AnalyteReferenceRangeBand> { Band(null, AgeUnit.Year, 0, 100, 4m, 10m) };
        Assert.Equal(expected, ResultFlagComputer.Compute(value, Sex.Female, AgeUnit.Year, 30, bands));
    }

    [Fact]
    public void Compute_NonNumeric_ReturnsNull()
    {
        var bands = new List<AnalyteReferenceRangeBand> { Band(null, AgeUnit.Year, 0, 100, 0m, 10m) };
        Assert.Null(ResultFlagComputer.Compute("abc", Sex.Male, AgeUnit.Year, 30, bands));
    }

    [Fact]
    public void Compute_NoMatchingBand_ReturnsNull()
    {
        var bands = new List<AnalyteReferenceRangeBand> { Band(Sex.Male, AgeUnit.Year, 0, 10, 0m, 10m) };
        Assert.Null(ResultFlagComputer.Compute("5", Sex.Male, AgeUnit.Year, 30, bands));
    }

    [Fact]
    public void SelectMatch_SexSpecific_Beats_SexNull()
    {
        var generic = Band(null, AgeUnit.Year, 0, 100, 0m, 10m, id: 1);
        var specific = Band(Sex.Male, AgeUnit.Year, 0, 100, 20m, 30m, id: 2);
        var bands = new List<AnalyteReferenceRangeBand> { generic, specific };

        Assert.Same(specific, ResultFlagComputer.SelectMatch(Sex.Male, AgeUnit.Year, 30, bands));

        var female = ResultFlagComputer.SelectMatch(Sex.Female, AgeUnit.Year, 30, bands);
        Assert.Same(generic, female);
    }

    [Fact]
    public void SelectMatch_NarrowerBand_Wins()
    {
        var wide = Band(null, AgeUnit.Year, 0, 100, 0m, 10m, id: 1);
        var narrow = Band(null, AgeUnit.Year, 20, 40, 0m, 10m, id: 2);
        var bands = new List<AnalyteReferenceRangeBand> { wide, narrow };

        Assert.Same(narrow, ResultFlagComputer.SelectMatch(Sex.Male, AgeUnit.Year, 30, bands));
    }

    [Fact]
    public void SelectMatch_Tie_BrokenBy_LowestId()
    {
        var first = Band(null, AgeUnit.Year, 0, 100, 0m, 10m, id: 5);
        var second = Band(null, AgeUnit.Year, 0, 100, 0m, 99m, id: 9);
        var bands = new List<AnalyteReferenceRangeBand> { second, first };

        Assert.Same(first, ResultFlagComputer.SelectMatch(Sex.Male, AgeUnit.Year, 30, bands));
    }

    [Fact]
    public void SelectMatch_NoCandidate_ReturnsNull()
    {
        var bands = new List<AnalyteReferenceRangeBand> { Band(Sex.Male, AgeUnit.Year, 0, 5, 0m, 1m, id: 1) };
        Assert.Null(ResultFlagComputer.SelectMatch(Sex.Male, AgeUnit.Year, 99, bands));
    }
}