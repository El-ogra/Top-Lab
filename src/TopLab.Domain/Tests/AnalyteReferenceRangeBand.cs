using TopLab.Domain.Common;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;

namespace TopLab.Domain.Tests;

/// <summary>
/// A typed age/sex band beneath an <see cref="AnalyteReferenceRange"/> aggregate.
/// Mirrors the historic <see cref="ReferenceRange"/> clinical shape so existing
/// matching semantics are preserved while the analyte owns the current range.
/// </summary>
public sealed class AnalyteReferenceRangeBand : Entity<AnalyteReferenceRangeBandId>
{
    public const int MaxCommentLength = 500;

    public AnalyteReferenceRangeId AnalyteReferenceRangeId { get; private set; } = default!;

    public Sex? Sex { get; private set; }

    public AgeUnit AgeUnit { get; private set; }

    public int AgeMin { get; private set; }

    public int AgeMax { get; private set; }

    public decimal MinValue { get; private set; }

    public decimal MaxValue { get; private set; }

    public string? LowComment { get; private set; }

    public string? HighComment { get; private set; }

    private AnalyteReferenceRangeBand()
    {
    }

    private AnalyteReferenceRangeBand(
        AnalyteReferenceRangeBandId id,
        AnalyteReferenceRangeId analyteReferenceRangeId,
        Sex? sex,
        AgeUnit ageUnit,
        int ageMin,
        int ageMax,
        decimal minValue,
        decimal maxValue,
        string? lowComment,
        string? highComment)
        : base(id)
    {
        AnalyteReferenceRangeId = analyteReferenceRangeId;
        Sex = sex;
        AgeUnit = ageUnit;
        AgeMin = ageMin;
        AgeMax = ageMax;
        MinValue = minValue;
        MaxValue = maxValue;
        LowComment = lowComment;
        HighComment = highComment;
    }

    public static AnalyteReferenceRangeBand Create(
        AnalyteReferenceRangeBandId id,
        AnalyteReferenceRangeId analyteReferenceRangeId,
        AgeUnit ageUnit,
        int ageMin,
        int ageMax,
        decimal minValue,
        decimal maxValue,
        Sex? sex = null,
        string? lowComment = null,
        string? highComment = null)
    {
        ArgumentNullException.ThrowIfNull(analyteReferenceRangeId);

        Guard(ageUnit, ageMin, ageMax, minValue, maxValue, lowComment, highComment);

        return new AnalyteReferenceRangeBand(
            id,
            analyteReferenceRangeId,
            sex,
            ageUnit,
            ageMin,
            ageMax,
            minValue,
            maxValue,
            lowComment,
            highComment);
    }

    /// <summary>Age-unit sensitive matching — no conversion between units (BR-04).</summary>
    public bool Matches(Sex? sex, AgeUnit ageUnit, int ageValue)
    {
        if (AgeUnit != ageUnit)
        {
            return false;
        }

        if (Sex is not null && Sex != sex)
        {
            return false;
        }

        return ageValue >= AgeMin && ageValue <= AgeMax;
    }

    private static void Guard(
        AgeUnit ageUnit,
        int ageMin,
        int ageMax,
        decimal minValue,
        decimal maxValue,
        string? lowComment,
        string? highComment)
    {
        if (ageMin < 0)
        {
            throw new ArgumentException("AgeMin must be >= 0.", nameof(ageMin));
        }

        if (ageMin > ageMax)
        {
            throw new ArgumentException("AgeMin must be <= AgeMax.");
        }

        if (minValue > maxValue)
        {
            throw new ArgumentException("MinValue must be <= MaxValue.");
        }

        if (lowComment?.Length > MaxCommentLength || highComment?.Length > MaxCommentLength)
        {
            throw new ArgumentException($"Comment must be at most {MaxCommentLength} characters.");
        }
    }
}