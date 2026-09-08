using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Domain.Tests.Results;

public class PatientTestReferenceRangeSnapshotTests
{
    [Fact]
    public void FromSnapshot_Maps_AllTenFields_OneToOne()
    {
        var captured = new DateTimeOffset(2026, 9, 8, 12, 0, 0, TimeSpan.Zero);
        var snapshot = new ReferenceRangeSnapshot(
            42,
            Sex.Female,
            AgeUnit.Year,
            18,
            65,
            3.5m,
            11.0m,
            "low",
            "high",
            captured);

        var entity = PatientTestReferenceRangeSnapshot.FromSnapshot(PatientTestId.Create(7), snapshot);

        Assert.Equal(7, entity.PatientTestId.Value);
        Assert.Equal(42, entity.TestId);
        Assert.Equal(Sex.Female, entity.Sex);
        Assert.Equal(AgeUnit.Year, entity.AgeUnit);
        Assert.Equal(18, entity.AgeMin);
        Assert.Equal(65, entity.AgeMax);
        Assert.Equal(3.5m, entity.MinValue);
        Assert.Equal(11.0m, entity.MaxValue);
        Assert.Equal("low", entity.LowComment);
        Assert.Equal("high", entity.HighComment);
        Assert.Equal(captured, entity.CapturedAtUtc);
    }

    [Fact]
    public void FromSnapshot_Preserves_NullSexAndNullComments()
    {
        var captured = new DateTimeOffset(2026, 1, 2, 0, 0, 0, TimeSpan.Zero);
        var snapshot = new ReferenceRangeSnapshot(
            9,
            null,
            AgeUnit.Month,
            0,
            11,
            0.5m,
            1.5m,
            null,
            null,
            captured);

        var entity = PatientTestReferenceRangeSnapshot.FromSnapshot(PatientTestId.Create(1), snapshot);

        Assert.Null(entity.Sex);
        Assert.Null(entity.LowComment);
        Assert.Null(entity.HighComment);
        Assert.Equal(9, entity.TestId);
        Assert.Equal(AgeUnit.Month, entity.AgeUnit);
    }
}
