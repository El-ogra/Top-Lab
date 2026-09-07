namespace TopLab.Application.Features.CultureAndAntibiotics.Common;

public static class CultureAntibioticDisplay
{
    public const int ChildAgeThresholdYears = 12;

    public static bool IsDisplayable(
        bool isPregnancyFlagged,
        bool isChildrenFlagged,
        bool isPregnancyIndicated,
        bool isChildUnder12)
    {
        return (!isPregnancyFlagged && !isChildrenFlagged)
            || (isPregnancyFlagged && isPregnancyIndicated)
            || (isChildrenFlagged && isChildUnder12);
    }
}