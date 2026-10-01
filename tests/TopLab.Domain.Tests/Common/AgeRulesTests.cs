using TopLab.Domain.Common;
using TopLab.Domain.Common.Enums;
using Xunit;

namespace TopLab.Domain.Tests.Common;

/// <summary>
/// W-02 Slice 3 / C-20: age classification for antibiotic display. The original defect was
/// that an 11-month-old was stored as (Month, 11) and therefore never classified as a child.
/// These tests pin the classification only — BR-04 ("no conversion between age units")
/// still governs AnalyteReferenceRangeBand.Matches, which is unchanged.
/// </summary>
public class AgeRulesTests
{
    [Theory]
    [InlineData(11, true)]
    [InlineData(12, false)]
    [InlineData(13, false)]
    public void AgeRules_Year_IsClassified(int age, bool expected)
        => Assert.Equal(expected, AgeRules.IsUnderTwelve(age, AgeUnit.Year));

    /// <summary>
    /// W-02 C-20, corrected against the plan's own test list. The plan's
    /// <c>AgeRules_Month13_IsNotChild</c> / <c>AgeRules_Day365_IsNotChild</c> /
    /// <c>AgeRules_Day2000_IsNotChild</c> expectations are medically wrong: 13 months is
    /// one year one month, 365 days is exactly one year, and 2000 days is about five and a
    /// half years — all three ARE under twelve. The plan's implementation snippet
    /// (normalise to whole years, then compare) is correct and is what is implemented here.
    /// </summary>
    [Theory]
    [InlineData(11, true)]    // 11 months — the original defect
    [InlineData(13, true)]    // 1 year 1 month — still a child
    [InlineData(143, true)]   // 11 years 11 months
    [InlineData(144, false)]  // exactly 12 years
    public void AgeRules_Month_IsClassified(int months, bool expected)
        => Assert.Equal(expected, AgeRules.IsUnderTwelve(months, AgeUnit.Month));

    [Theory]
    [InlineData(364, true)]    // the day before the first birthday
    [InlineData(365, true)]    // exactly one year
    [InlineData(2000, true)]   // about five and a half years
    [InlineData(4379, true)]   // one day short of twelve years
    [InlineData(4380, false)]  // exactly twelve years (12 x 365)
    public void AgeRules_Day_IsClassified(int days, bool expected)
        => Assert.Equal(expected, AgeRules.IsUnderTwelve(days, AgeUnit.Day));

    [Fact]
    public void AgeRules_Zero_IsChild()
        => Assert.True(AgeRules.IsUnderTwelve(0, AgeUnit.Year));

    [Theory]
    [InlineData(AgeUnit.Year, 11, 11)]
    [InlineData(AgeUnit.Month, 11, 0)]
    [InlineData(AgeUnit.Month, 24, 2)]
    [InlineData(AgeUnit.Day, 364, 0)]
    [InlineData(AgeUnit.Day, 730, 2)]
    public void AgeRules_ToWholeYears_Normalises(AgeUnit unit, int value, int expected)
        => Assert.Equal(expected, AgeRules.ToWholeYears(value, unit));
}
