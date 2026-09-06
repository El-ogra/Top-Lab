using TopLab.Domain.Common;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;

namespace TopLab.Domain.Tests;

public sealed class ReferenceRange : Entity<ReferenceRangeId>
{
    public const int MaxCommentLength = 500;

    public TestId TestId { get; private set; } = default!;

    public Sex? Sex { get; private set; }

    public AgeUnit AgeUnit { get; private set; }

    public int AgeMin { get; private set; }

    public int AgeMax { get; private set; }

    public decimal MinValue { get; private set; }

    public decimal MaxValue { get; private set; }

    public string? LowComment { get; private set; }

    public string? HighComment { get; private set; }

    private ReferenceRange()
    {
    }

    private ReferenceRange(
        ReferenceRangeId id,
        TestId testId,
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
        TestId = testId;
        Sex = sex;
        AgeUnit = ageUnit;
        AgeMin = ageMin;
        AgeMax = ageMax;
        MinValue = minValue;
        MaxValue = maxValue;
        LowComment = lowComment;
        HighComment = highComment;
    }

    public static ReferenceRange Create(
        ReferenceRangeId id,
        TestId testId,
        AgeUnit ageUnit,
        int ageMin,
        int ageMax,
        decimal minValue,
        decimal maxValue,
        Sex? sex = null,
        string? lowComment = null,
        string? highComment = null)
    {
        Guard(ageUnit, ageMin, ageMax, minValue, maxValue, lowComment, highComment);

        return new ReferenceRange(id, testId, sex, ageUnit, ageMin, ageMax, minValue, maxValue, lowComment, highComment);
    }

    public void Update(
        Sex? sex,
        AgeUnit ageUnit,
        int ageMin,
        int ageMax,
        decimal minValue,
        decimal maxValue,
        string? lowComment,
        string? highComment)
    {
        Guard(ageUnit, ageMin, ageMax, minValue, maxValue, lowComment, highComment);

        Sex = sex;
        AgeUnit = ageUnit;
        AgeMin = ageMin;
        AgeMax = ageMax;
        MinValue = minValue;
        MaxValue = maxValue;
        LowComment = lowComment;
        HighComment = highComment;
    }

    /// <summary>Age-unit sensitive matching — no conversion between units.</summary>
    public bool Matches(Sex sex, AgeUnit ageUnit, int ageValue)
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

    public ReferenceRangeSnapshot CaptureSnapshot()
    {
        return new ReferenceRangeSnapshot(
            TestId.Value,
            Sex,
            AgeUnit,
            AgeMin,
            AgeMax,
            MinValue,
            MaxValue,
            LowComment,
            HighComment,
            DateTimeOffset.UtcNow);
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