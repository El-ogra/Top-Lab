using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;

namespace TopLab.Domain.Results;

/// <summary>
/// Per-item historical freeze captured at profile-result entry time. Keyed 1:1 by
/// <see cref="ProfileResultItemId"/>, retaining the configured <c>AnalyteId</c> plus
/// the selected band fields and capture time. Reports/reprints read only this
/// snapshot — never the live/current analyte range, no matter how much later they
/// are viewed (Decision 1). <c>AnalyteId</c> is a historical identity, not a live
/// FK, mirroring the M-04 <see cref="PatientTestReferenceRangeSnapshot"/> design.
/// </summary>
public sealed class ProfileResultItemReferenceRangeSnapshot
{
    public ProfileResultItemId ProfileResultItemId { get; private set; } = default!;

    public AnalyteId AnalyteId { get; private set; } = default!;

    public Sex? Sex { get; private set; }

    public AgeUnit AgeUnit { get; private set; }

    public int AgeMin { get; private set; }

    public int AgeMax { get; private set; }

    public decimal MinValue { get; private set; }

    public decimal MaxValue { get; private set; }

    public string? LowComment { get; private set; }

    public string? HighComment { get; private set; }

    public DateTimeOffset CapturedAtUtc { get; private set; }

    private ProfileResultItemReferenceRangeSnapshot()
    {
    }

    private ProfileResultItemReferenceRangeSnapshot(
        ProfileResultItemId profileResultItemId,
        AnalyteId analyteId,
        Sex? sex,
        AgeUnit ageUnit,
        int ageMin,
        int ageMax,
        decimal minValue,
        decimal maxValue,
        string? lowComment,
        string? highComment,
        DateTimeOffset capturedAtUtc)
    {
        ProfileResultItemId = profileResultItemId;
        AnalyteId = analyteId;
        Sex = sex;
        AgeUnit = ageUnit;
        AgeMin = ageMin;
        AgeMax = ageMax;
        MinValue = minValue;
        MaxValue = maxValue;
        LowComment = lowComment;
        HighComment = highComment;
        CapturedAtUtc = capturedAtUtc;
    }

    public static ProfileResultItemReferenceRangeSnapshot Create(
        ProfileResultItemId profileResultItemId,
        AnalyteId analyteId,
        Sex? sex,
        AgeUnit ageUnit,
        int ageMin,
        int ageMax,
        decimal minValue,
        decimal maxValue,
        string? lowComment,
        string? highComment,
        DateTimeOffset capturedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(profileResultItemId);
        ArgumentNullException.ThrowIfNull(analyteId);

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

        return new ProfileResultItemReferenceRangeSnapshot(
            profileResultItemId,
            analyteId,
            sex,
            ageUnit,
            ageMin,
            ageMax,
            minValue,
            maxValue,
            lowComment,
            highComment,
            capturedAtUtc);
    }
}