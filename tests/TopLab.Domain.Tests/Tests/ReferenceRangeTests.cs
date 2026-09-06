using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Domain.Tests.Tests;

public class ReferenceRangeTests
{
    private static ReferenceRange CreateRange(Sex? sex = null, AgeUnit ageUnit = AgeUnit.Day, int ageMin = 1, int ageMax = 60, string? lowComment = null, string? highComment = null)
    {
        return ReferenceRange.Create(ReferenceRangeId.Create(1), TestId.Create(1), ageUnit, ageMin, ageMax, 0m, 100m, sex, lowComment, highComment);
    }

    [Theory]
    [InlineData(15)]
    [InlineData(35)]
    public void Matches_AgeWithinRange_ReturnsTrue(int ageValue)
    {
        var range = CreateRange(ageMin: 1, ageMax: 60);

        Assert.True(range.Matches(Sex.Male, AgeUnit.Day, ageValue));
    }

    [Fact]
    public void Matches_Boundaries_ReturnsTrue()
    {
        var range = CreateRange(ageMin: 1, ageMax: 60);

        Assert.True(range.Matches(Sex.Male, AgeUnit.Day, 1));
        Assert.True(range.Matches(Sex.Male, AgeUnit.Day, 60));
    }

    [Fact]
    public void Matches_OutOfRange_ReturnsFalse()
    {
        var range = CreateRange(ageMin: 1, ageMax: 60);

        Assert.False(range.Matches(Sex.Male, AgeUnit.Day, 61));
    }

    [Fact]
    public void Matches_DifferentAgeUnit_ReturnsFalse()
    {
        var range = CreateRange(ageMin: 1, ageMax: 60);

        Assert.False(range.Matches(Sex.Male, AgeUnit.Month, 1));
    }

    [Fact]
    public void Matches_NullSex_ServesBothSexes()
    {
        var range = CreateRange(sex: null);

        Assert.True(range.Matches(Sex.Male, AgeUnit.Day, 30));
        Assert.True(range.Matches(Sex.Female, AgeUnit.Day, 30));
    }

    [Fact]
    public void Matches_SpecificSex_RejectsOtherSex()
    {
        var range = CreateRange(sex: Sex.Male);

        Assert.True(range.Matches(Sex.Male, AgeUnit.Day, 30));
        Assert.False(range.Matches(Sex.Female, AgeUnit.Day, 30));
    }

    [Fact]
    public void Update_ChangesAllFields()
    {
        var range = CreateRange(sex: Sex.Male, ageUnit: AgeUnit.Day, ageMin: 1, ageMax: 60);

        range.Update(Sex.Female, AgeUnit.Month, 6, 10, 1.5m, 2.5m, "low", "high");

        Assert.Equal(Sex.Female, range.Sex);
        Assert.Equal(AgeUnit.Month, range.AgeUnit);
        Assert.Equal(6, range.AgeMin);
        Assert.Equal(10, range.AgeMax);
        Assert.Equal(1.5m, range.MinValue);
        Assert.Equal(2.5m, range.MaxValue);
        Assert.Equal("low", range.LowComment);
        Assert.Equal("high", range.HighComment);
    }

    [Fact]
    public void Update_AgeMinGreaterThanAgeMax_Throws()
    {
        var range = CreateRange();

        Assert.Throws<ArgumentException>(() => range.Update(null, AgeUnit.Day, 10, 5, 0m, 100m, null, null));
    }

    [Fact]
    public void Update_MinValueGreaterThanMaxValue_Throws()
    {
        var range = CreateRange();

        Assert.Throws<ArgumentException>(() => range.Update(null, AgeUnit.Day, 1, 60, 100m, 0m, null, null));
    }

    [Fact]
    public void Update_AgeMinNegative_Throws()
    {
        var range = CreateRange();

        Assert.Throws<ArgumentException>(() => range.Update(null, AgeUnit.Day, -1, 60, 0m, 100m, null, null));
    }

    [Fact]
    public void Update_LowCommentTooLong_Throws()
    {
        var range = CreateRange();
        var longComment = new string('X', ReferenceRange.MaxCommentLength + 1);

        Assert.Throws<ArgumentException>(() => range.Update(null, AgeUnit.Day, 1, 60, 0m, 100m, longComment, null));
    }

    [Fact]
    public void Update_HighCommentTooLong_Throws()
    {
        var range = CreateRange();
        var longComment = new string('X', ReferenceRange.MaxCommentLength + 1);

        Assert.Throws<ArgumentException>(() => range.Update(null, AgeUnit.Day, 1, 60, 0m, 100m, null, longComment));
    }

    [Fact]
    public void Update_CommentAtMaxLength_Allowed()
    {
        var range = CreateRange();
        var maxComment = new string('X', ReferenceRange.MaxCommentLength);

        range.Update(null, AgeUnit.Day, 1, 60, 0m, 100m, maxComment, null);

        Assert.Equal(maxComment, range.LowComment);
    }

    [Fact]
    public void Create_AgeMinNegative_Throws()
    {
        Assert.Throws<ArgumentException>(() => CreateRange(ageMin: -1));
    }

    [Fact]
    public void Create_CommentTooLong_Throws()
    {
        var longComment = new string('X', ReferenceRange.MaxCommentLength + 1);

        Assert.Throws<ArgumentException>(() => CreateRange(lowComment: longComment));
    }

    [Fact]
    public void CaptureSnapshot_ReturnsValueEqualImmutableCopy()
    {
        var range = CreateRange(sex: Sex.Male, ageUnit: AgeUnit.Day, ageMin: 1, ageMax: 60);
        var before = DateTimeOffset.UtcNow;

        var snapshot = range.CaptureSnapshot();

        Assert.IsType<ReferenceRangeSnapshot>(snapshot);
        Assert.Equal(range.TestId.Value, snapshot.TestId);
        Assert.Equal(Sex.Male, snapshot.Sex);
        Assert.Equal(AgeUnit.Day, snapshot.AgeUnit);
        Assert.Equal(1, snapshot.AgeMin);
        Assert.Equal(60, snapshot.AgeMax);
        Assert.Equal(0m, snapshot.MinValue);
        Assert.Equal(100m, snapshot.MaxValue);
        Assert.True(snapshot.CapturedAtUtc >= before);
        Assert.NotEqual(default, snapshot.CapturedAtUtc);

        var copied = snapshot with { CapturedAtUtc = snapshot.CapturedAtUtc };
        Assert.Equal(snapshot, copied);
    }
}