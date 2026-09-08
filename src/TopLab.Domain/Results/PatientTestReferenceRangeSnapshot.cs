using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Tests;

namespace TopLab.Domain.Results;

/// <summary>
/// Persisted BR-05 reference-range freeze as a dedicated 1:1 child table keyed by
/// <see cref="PatientTestId"/> (PK + FK to <see cref="PatientTest"/>, Cascade).
/// Mirrors the <see cref="ReferenceRangeSnapshot"/> record shape verbatim.
/// <c>TestId</c> is stored as the record's plain <c>int</c>, consistent with the
/// record's shape — no FK to <c>Tests</c> on this scalar: the snapshot is a
/// historical document, not a live reference. The snapshot dies with its result row.
/// </summary>
public sealed class PatientTestReferenceRangeSnapshot
{
    public PatientTestId PatientTestId { get; private set; } = default!;

    public int TestId { get; private set; }

    public Sex? Sex { get; private set; }

    public AgeUnit AgeUnit { get; private set; }

    public int AgeMin { get; private set; }

    public int AgeMax { get; private set; }

    public decimal MinValue { get; private set; }

    public decimal MaxValue { get; private set; }

    public string? LowComment { get; private set; }

    public string? HighComment { get; private set; }

    public DateTimeOffset CapturedAtUtc { get; private set; }

    private PatientTestReferenceRangeSnapshot()
    {
    }

    private PatientTestReferenceRangeSnapshot(
        PatientTestId patientTestId,
        int testId,
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
        PatientTestId = patientTestId;
        TestId = testId;
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

    public static PatientTestReferenceRangeSnapshot FromSnapshot(
        PatientTestId patientTestId,
        ReferenceRangeSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(patientTestId);
        ArgumentNullException.ThrowIfNull(snapshot);

        return new PatientTestReferenceRangeSnapshot(
            patientTestId,
            snapshot.TestId,
            snapshot.Sex,
            snapshot.AgeUnit,
            snapshot.AgeMin,
            snapshot.AgeMax,
            snapshot.MinValue,
            snapshot.MaxValue,
            snapshot.LowComment,
            snapshot.HighComment,
            snapshot.CapturedAtUtc);
    }
}
