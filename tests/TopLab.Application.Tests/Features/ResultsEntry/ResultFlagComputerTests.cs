using TopLab.Application.Features.ResultsEntry.Common;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Application.Tests.Features.ResultsEntry;

public class ResultFlagComputerTests
{
    private static int _nextId = 9000;

    private static ReferenceRange Range(
        Sex? sex,
        AgeUnit ageUnit,
        int ageMin,
        int ageMax,
        decimal min,
        decimal max,
        int? id = null)
    {
        return ReferenceRange.Create(
            ReferenceRangeId.Create(id ?? _nextId++),
            TestId.Create(1),
            ageUnit,
            ageMin,
            ageMax,
            min,
            max,
            sex);
    }

    [Fact]
    public void Compute_NonNumeric_ReturnsNull()
    {
        var ranges = new List<ReferenceRange> { Range(null, AgeUnit.Year, 0, 100, 0m, 10m) };
        Assert.Null(ResultFlagComputer.Compute("abc", Sex.Male, AgeUnit.Year, 30, ranges));
    }

    [Fact]
    public void Compute_NullOrWhitespace_ReturnsNull()
    {
        var ranges = new List<ReferenceRange> { Range(null, AgeUnit.Year, 0, 100, 0m, 10m) };
        Assert.Null(ResultFlagComputer.Compute(null, Sex.Male, AgeUnit.Year, 30, ranges));
        Assert.Null(ResultFlagComputer.Compute("   ", Sex.Male, AgeUnit.Year, 30, ranges));
    }

    [Fact]
    public void Compute_NoMatchingRange_ReturnsNull()
    {
        var ranges = new List<ReferenceRange> { Range(Sex.Male, AgeUnit.Year, 0, 10, 0m, 10m) };
        Assert.Null(ResultFlagComputer.Compute("5", Sex.Male, AgeUnit.Year, 30, ranges));
    }

    [Fact]
    public void Compute_AgeUnitMismatch_ReturnsNull()
    {
        var ranges = new List<ReferenceRange> { Range(null, AgeUnit.Month, 0, 100, 0m, 10m) };
        Assert.Null(ResultFlagComputer.Compute("5", Sex.Male, AgeUnit.Year, 30, ranges));
    }

    [Theory]
    [InlineData("3", ResultFlag.Low)]
    [InlineData("5", ResultFlag.Normal)]
    [InlineData("12", ResultFlag.High)]
    public void Compute_LowNormalHigh(string value, ResultFlag expected)
    {
        var ranges = new List<ReferenceRange> { Range(null, AgeUnit.Year, 0, 100, 4m, 10m) };
        Assert.Equal(expected, ResultFlagComputer.Compute(value, Sex.Male, AgeUnit.Year, 30, ranges));
    }

    [Theory]
    [InlineData("4", ResultFlag.Normal)]
    [InlineData("10", ResultFlag.Normal)]
    public void Compute_Boundaries_AreInclusive(string value, ResultFlag expected)
    {
        var ranges = new List<ReferenceRange> { Range(null, AgeUnit.Year, 0, 100, 4m, 10m) };
        Assert.Equal(expected, ResultFlagComputer.Compute(value, Sex.Female, AgeUnit.Year, 30, ranges));
    }

    [Fact]
    public void SelectMatch_SexSpecific_Beats_SexNull()
    {
        var generic = Range(null, AgeUnit.Year, 0, 100, 0m, 10m, id: 1);
        var specific = Range(Sex.Male, AgeUnit.Year, 0, 100, 20m, 30m, id: 2);
        var ranges = new List<ReferenceRange> { generic, specific };

        var match = ResultFlagComputer.SelectMatch(Sex.Male, AgeUnit.Year, 30, ranges);
        Assert.Same(specific, match);

        // Female patient falls back to the generic row.
        var female = ResultFlagComputer.SelectMatch(Sex.Female, AgeUnit.Year, 30, ranges);
        Assert.Same(generic, female);
    }

    [Fact]
    public void SelectMatch_NarrowerBand_Wins()
    {
        var wide = Range(null, AgeUnit.Year, 0, 100, 0m, 10m, id: 1);
        var narrow = Range(null, AgeUnit.Year, 20, 40, 0m, 10m, id: 2);
        var ranges = new List<ReferenceRange> { wide, narrow };

        Assert.Same(narrow, ResultFlagComputer.SelectMatch(Sex.Male, AgeUnit.Year, 30, ranges));
    }

    [Fact]
    public void SelectMatch_Tie_BrokenBy_LowestId()
    {
        var first = Range(null, AgeUnit.Year, 0, 100, 0m, 10m, id: 5);
        var second = Range(null, AgeUnit.Year, 0, 100, 0m, 99m, id: 9);
        var ranges = new List<ReferenceRange> { second, first };

        Assert.Same(first, ResultFlagComputer.SelectMatch(Sex.Male, AgeUnit.Year, 30, ranges));
    }

    [Fact]
    public void SelectMatch_NoCandidate_ReturnsNull()
    {
        var ranges = new List<ReferenceRange> { Range(Sex.Male, AgeUnit.Year, 0, 5, 0m, 1m, id: 1) };
        Assert.Null(ResultFlagComputer.SelectMatch(Sex.Male, AgeUnit.Year, 99, ranges));
    }
}
