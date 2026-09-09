using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Results;
using Xunit;

namespace TopLab.Domain.Tests.Results;

public class ProfileResultItemReferenceRangeSnapshotTests
{
    [Fact]
    public void Create_RetainsAnalyteIdentity_AndCapturedBandFields()
    {
        var captured = new DateTimeOffset(2026, 9, 9, 12, 0, 0, TimeSpan.Zero);
        var snapshot = ProfileResultItemReferenceRangeSnapshot.Create(
            ProfileResultItemId.Create(1),
            AnalyteId.Create(42),
            Sex.Male,
            AgeUnit.Year,
            0,
            120,
            4m,
            10m,
            "lo",
            "hi",
            captured);

        Assert.Equal(AnalyteId.Create(42), snapshot.AnalyteId);
        Assert.Equal(Sex.Male, snapshot.Sex);
        Assert.Equal(AgeUnit.Year, snapshot.AgeUnit);
        Assert.Equal(0, snapshot.AgeMin);
        Assert.Equal(120, snapshot.AgeMax);
        Assert.Equal(4m, snapshot.MinValue);
        Assert.Equal(10m, snapshot.MaxValue);
        Assert.Equal("lo", snapshot.LowComment);
        Assert.Equal("hi", snapshot.HighComment);
        Assert.Equal(captured, snapshot.CapturedAtUtc);
    }

    [Fact]
    public void Create_Guards_InvalidBand()
    {
        Assert.Throws<ArgumentException>(() => ProfileResultItemReferenceRangeSnapshot.Create(
            ProfileResultItemId.Create(1),
            AnalyteId.Create(42),
            null,
            AgeUnit.Year,
            10,
            5,
            4m,
            10m,
            null,
            null,
            DateTimeOffset.UtcNow));

        Assert.Throws<ArgumentException>(() => ProfileResultItemReferenceRangeSnapshot.Create(
            ProfileResultItemId.Create(1),
            AnalyteId.Create(42),
            null,
            AgeUnit.Year,
            0,
            100,
            10m,
            4m,
            null,
            null,
            DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Snapshot_IsPureHistoricalDocument_NoLiveEntityReference()
    {
        // The snapshot carries only captured primitives plus the historical AnalyteId;
        // it exposes no navigation to the live Analyte entity or its range aggregate,
        // so reports cannot read the current/live range through it (Decision 1).
        var snapshot = ProfileResultItemReferenceRangeSnapshot.Create(
            ProfileResultItemId.Create(1),
            AnalyteId.Create(42),
            null,
            AgeUnit.Year,
            0,
            120,
            1m,
            2m,
            null,
            null,
            DateTimeOffset.UtcNow);

        Assert.DoesNotContain(typeof(ProfileResultItemReferenceRangeSnapshot).GetProperties(), p => p.PropertyType == typeof(Analyte));
        Assert.DoesNotContain(typeof(ProfileResultItemReferenceRangeSnapshot).GetProperties(), p => p.PropertyType == typeof(AnalyteReferenceRange));
        Assert.Equal(AnalyteId.Create(42), snapshot.AnalyteId);
    }
}