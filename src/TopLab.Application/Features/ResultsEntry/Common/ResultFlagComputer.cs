using System.Globalization;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.ResultsEntry.Common;

/// <summary>
/// Pure flag computer for simple results. <c>SelectMatch</c> implements the
/// owner-settled overlapping-range rule: sex-matched preferred over sex-null,
/// then narrowest age band, then lowest id (oldest row). The rule is shared
/// verbatim by the legacy <see cref="ReferenceRange"/> path and the
/// analyte-owned <see cref="AnalyteReferenceRangeBand"/> path (Decision 1:
/// the analyte is the only live range source).
/// </summary>
internal static class ResultFlagComputer
{
    internal static ReferenceRange? SelectMatch(
        Sex sex,
        AgeUnit ageUnit,
        int ageValue,
        IReadOnlyList<ReferenceRange> ranges)
    {
        var candidates = ranges.Where(r => r.Matches(sex, ageUnit, ageValue)).ToList();
        return SelectBest(candidates, r => r.Sex, r => r.AgeMax - r.AgeMin, r => r.Id.Value);
    }

    internal static AnalyteReferenceRangeBand? SelectMatch(
        Sex sex,
        AgeUnit ageUnit,
        int ageValue,
        IReadOnlyList<AnalyteReferenceRangeBand> bands)
    {
        var candidates = bands.Where(b => b.Matches(sex, ageUnit, ageValue)).ToList();
        return SelectBest(candidates, b => b.Sex, b => b.AgeMax - b.AgeMin, b => b.Id.Value);
    }

    internal static ResultFlag? Compute(
        string? resultValue,
        Sex sex,
        AgeUnit ageUnit,
        int ageValue,
        IReadOnlyList<ReferenceRange> ranges)
    {
        if (!TryParse(resultValue, out var value))
        {
            return null;
        }

        var match = SelectMatch(sex, ageUnit, ageValue, ranges);
        return match is null ? null : ComputeValue(value, match.MinValue, match.MaxValue);
    }

    internal static ResultFlag? Compute(
        string? resultValue,
        Sex sex,
        AgeUnit ageUnit,
        int ageValue,
        IReadOnlyList<AnalyteReferenceRangeBand> bands)
    {
        if (!TryParse(resultValue, out var value))
        {
            return null;
        }

        var match = SelectMatch(sex, ageUnit, ageValue, bands);
        return match is null ? null : ComputeValue(value, match.MinValue, match.MaxValue);
    }

    private static bool TryParse(string? resultValue, out decimal value)
    {
        value = 0;
        if (string.IsNullOrWhiteSpace(resultValue))
        {
            return false;
        }

        return decimal.TryParse(resultValue.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out value);
    }

    private static ResultFlag? ComputeValue(decimal value, decimal minValue, decimal maxValue)
    {
        if (value < minValue)
        {
            return ResultFlag.Low;
        }

        if (value > maxValue)
        {
            return ResultFlag.High;
        }

        return ResultFlag.Normal;
    }

    private static T? SelectBest<T>(
        IReadOnlyList<T> candidates,
        Func<T, Sex?> sexSelector,
        Func<T, int> bandSpanSelector,
        Func<T, int> idSelector)
        where T : class
    {
        if (candidates.Count == 0)
        {
            return null;
        }

        return candidates
            .OrderBy(c => sexSelector(c) is null ? 1 : 0)
            .ThenBy(bandSpanSelector)
            .ThenBy(idSelector)
            .First();
    }
}