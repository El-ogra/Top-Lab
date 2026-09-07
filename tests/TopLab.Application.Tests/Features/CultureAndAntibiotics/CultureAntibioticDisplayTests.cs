using TopLab.Application.Features.CultureAndAntibiotics.Common;
using Xunit;

namespace TopLab.Application.Tests.Features.CultureAndAntibiotics;

public class CultureAntibioticDisplayTests
{
    [Fact]
    public void ChildAgeThresholdYears_Is12()
    {
        Assert.Equal(12, CultureAntibioticDisplay.ChildAgeThresholdYears);
    }

    [Theory]
    // -- Flags both false: unflagged = universal (displayable regardless of patient context) --
    [InlineData(false, false, false, false, true)]
    [InlineData(false, false, false, true,  true)]
    [InlineData(false, false, true,  false, true)]
    [InlineData(false, false, true,  true,  true)]

    // -- PregnancyFlagged only: requires isPregnancyIndicated --
    [InlineData(true,  false, false, false, false)]
    [InlineData(true,  false, false, true,  false)]
    [InlineData(true,  false, true,  false, true)]
    [InlineData(true,  false, true,  true,  true)]

    // -- ChildrenFlagged only: requires isChildUnder12 --
    [InlineData(false, true,  false, false, false)]
    [InlineData(false, true,  false, true,  true)]
    [InlineData(false, true,  true,  false, false)]
    [InlineData(false, true,  true,  true,  true)]

    // -- Both flags set: union semantics (either condition holds) --
    [InlineData(true,  true,  false, false, false)]
    [InlineData(true,  true,  false, true,  true)]
    [InlineData(true,  true,  true,  false, true)]
    [InlineData(true,  true,  true,  true,  true)]
    public void IsDisplayable_FullTruthTable(
        bool isPregnancyFlagged,
        bool isChildrenFlagged,
        bool isPregnancyIndicated,
        bool isChildUnder12,
        bool expected)
    {
        var actual = CultureAntibioticDisplay.IsDisplayable(
            isPregnancyFlagged,
            isChildrenFlagged,
            isPregnancyIndicated,
            isChildUnder12);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void IsDisplayable_BothFlagsSet_AndEitherContextHolds_ReturnsTrue()
    {
        Assert.True(CultureAntibioticDisplay.IsDisplayable(
            isPregnancyFlagged: true,
            isChildrenFlagged: true,
            isPregnancyIndicated: true,
            isChildUnder12: false));

        Assert.True(CultureAntibioticDisplay.IsDisplayable(
            isPregnancyFlagged: true,
            isChildrenFlagged: true,
            isPregnancyIndicated: false,
            isChildUnder12: true));
    }

    [Fact]
    public void IsDisplayable_BothFlagsSet_NoContextHolds_ReturnsFalse()
    {
        Assert.False(CultureAntibioticDisplay.IsDisplayable(
            isPregnancyFlagged: true,
            isChildrenFlagged: true,
            isPregnancyIndicated: false,
            isChildUnder12: false));
    }

    [Fact]
    public void IsDisplayable_NoFlags_AlwaysTrue()
    {
        Assert.True(CultureAntibioticDisplay.IsDisplayable(
            isPregnancyFlagged: false,
            isChildrenFlagged: false,
            isPregnancyIndicated: false,
            isChildUnder12: false));
    }
}