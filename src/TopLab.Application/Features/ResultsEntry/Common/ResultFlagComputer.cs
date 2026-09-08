using System.Globalization;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Tests;

namespace TopLab.Application.Features.ResultsEntry.Common;

/// <summary>
/// Pure flag computer for simple results. <c>SelectMatch</c> implements the
/// owner-settled overlapping-range rule: sex-matched preferred over sex-null,
/// then narrowest age band, then lowest id (oldest row).
/// </summary>
internal static class ResultFlagComputer
{
    internal static ReferenceRange? SelectMatch(
        Sex sex,
        AgeUnit ageUnit,
        int ageValue,
        IReadOnlyList<ReferenceRange> ranges)
    {
        var candidates = new List<ReferenceRange>();
        foreach (var r in ranges)
        {
            if (r.Matches(sex, ageUnit, ageValue))
            {
                candidates.Add(r);
            }
        }

        if (candidates.Count == 0)
        {
            return null;
        }

        return candidates
            .OrderBy(r => r.Sex is null ? 1 : 0)
            .ThenBy(r => r.AgeMax - r.AgeMin)
            .ThenBy(r => r.Id.Value)
            .First();
    }

    internal static ResultFlag? Compute(
        string? resultValue,
        Sex sex,
        AgeUnit ageUnit,
        int ageValue,
        IReadOnlyList<ReferenceRange> ranges)
    {
        if (string.IsNullOrWhiteSpace(resultValue))
        {
            return null;
        }

        if (!decimal.TryParse(resultValue.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
        {
            return null;
        }

        var match = SelectMatch(sex, ageUnit, ageValue, ranges);
        if (match is null)
        {
            return null;
        }

        if (value < match.MinValue)
        {
            return ResultFlag.Low;
        }

        if (value > match.MaxValue)
        {
            return ResultFlag.High;
        }

        return ResultFlag.Normal;
    }
}
