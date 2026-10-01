using TopLab.Domain.Common.Enums;

namespace TopLab.Domain.Common;

/// <summary>
/// Age classification for antibiotic display. NOT a reference-range matcher:
/// BR-04 ("no conversion between age units") governs
/// AnalyteReferenceRangeBand.Matches, which compares like-for-like units. This type
/// converts a stored (value, unit) into a whole-year age purely to classify a
/// patient as under twelve. The two rules are deliberately different.
/// </summary>
public static class AgeRules
{
    public const int MonthsPerYear = 12;
    public const int DaysPerYear = 365;

    /// <summary>
    /// The threshold lives in Domain so this type has no dependency on Application. The
    /// Application-side <c>CultureAntibioticDisplay.ChildAgeThresholdYears</c> constant is kept
    /// (plan VG-03) and pinned by a test so the two can never drift apart silently.
    /// </summary>
    public const int ChildAgeThresholdYears = 12;

    /// <summary>Age in whole years, rounded down. Day/Month values are normalised.</summary>
    public static int ToWholeYears(int ageValue, AgeUnit ageUnit) => ageUnit switch
    {
        AgeUnit.Year => ageValue,
        AgeUnit.Month => ageValue / MonthsPerYear,
        AgeUnit.Day => ageValue / DaysPerYear,
        _ => ageValue
    };

    public static bool IsUnderTwelve(int ageValue, AgeUnit ageUnit)
        => ToWholeYears(ageValue, ageUnit) < ChildAgeThresholdYears;
}
