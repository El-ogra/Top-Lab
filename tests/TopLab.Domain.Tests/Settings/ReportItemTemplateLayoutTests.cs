using TopLab.Domain.Settings;
using Xunit;

namespace TopLab.Domain.Tests.Settings;

/// <summary>
/// GAP-015 (Phase 8): per-item line spacing, left offset and marker symbol
/// validation on <see cref="ReportItemTemplate"/>.
/// </summary>
public class ReportItemTemplateLayoutTests
{
    private static ReportItemTemplate CreateWithLayout(
        decimal? lineSpacing = null,
        decimal? leftOffsetCm = null,
        string? markerSymbol = null)
        => ReportItemTemplate.Create(
            ReportItemKeys.TestName,
            "التحليل",
            lineSpacing: lineSpacing,
            leftOffsetCm: leftOffsetCm,
            markerSymbol: markerSymbol);

    [Fact]
    public void Create_WithoutLayout_KeepsAllThreeFieldsNull()
    {
        var t = CreateWithLayout();

        Assert.Null(t.LineSpacing);
        Assert.Null(t.LeftOffsetCm);
        Assert.Null(t.MarkerSymbol);
    }

    [Theory]
    [InlineData(0.5)]
    [InlineData(1.0)]
    [InlineData(2.5)]
    [InlineData(10.0)]
    public void Create_AcceptsLineSpacingInsideRange(double value)
    {
        var t = CreateWithLayout(lineSpacing: (decimal)value);

        Assert.Equal((decimal)value, t.LineSpacing);
    }

    [Theory]
    [InlineData(0.49)]
    [InlineData(10.01)]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_RejectsLineSpacingOutsideRange(double value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => CreateWithLayout(lineSpacing: (decimal)value));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0.5)]
    [InlineData(10.0)]
    public void Create_AcceptsLeftOffsetInsideRange(double value)
    {
        var t = CreateWithLayout(leftOffsetCm: (decimal)value);

        Assert.Equal((decimal)value, t.LeftOffsetCm);
    }

    [Theory]
    [InlineData(10.01)]
    [InlineData(-0.01)]
    public void Create_RejectsLeftOffsetOutsideRange(double value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => CreateWithLayout(leftOffsetCm: (decimal)value));
    }

    [Theory]
    [InlineData("•")]
    [InlineData("*")]
    [InlineData("-")]
    [InlineData("ن")]
    [InlineData("👍")] // surrogate pair: one text element, two UTF-16 code units
    [InlineData("é")] // base + combining acute: one text element, two code units
    public void Create_AcceptsSingleVisibleMarkerSymbol(string marker)
    {
        var t = CreateWithLayout(markerSymbol: marker);

        Assert.Equal(marker, t.MarkerSymbol);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("**")]
    [InlineData("أ ب")]
    public void Create_RejectsBlankOrMultiCharacterMarkerSymbol(string marker)
    {
        Assert.Throws<ArgumentException>(() => CreateWithLayout(markerSymbol: marker));
    }

    [Fact]
    public void Create_RejectsMarkerLongerThanStorageHeadroom()
    {
        var marker = new string('*', ReportItemTemplate.MaxMarkerSymbolLength + 1);

        Assert.Throws<ArgumentException>(() => CreateWithLayout(markerSymbol: marker));
    }

    [Fact]
    public void Create_RejectsControlCharacterMarkerSymbol()
    {
        Assert.Throws<ArgumentException>(() => CreateWithLayout(markerSymbol: "\t"));
    }

    [Fact]
    public void Update_PersistsLayoutFields()
    {
        var t = CreateWithLayout();

        t.Update("التحليل", null, true, null, false, 1.75m, 2.25m, "#");

        Assert.Equal(1.75m, t.LineSpacing);
        Assert.Equal(2.25m, t.LeftOffsetCm);
        Assert.Equal("#", t.MarkerSymbol);
    }

    [Fact]
    public void Update_ClearingLayout_ResetsToNull()
    {
        var t = CreateWithLayout(2.0m, 1.0m, "*");

        t.Update("التحليل", null, true, null, false, null, null, null);

        Assert.Null(t.LineSpacing);
        Assert.Null(t.LeftOffsetCm);
        Assert.Null(t.MarkerSymbol);
    }

    [Theory]
    [InlineData(0.49, 0, null)]
    [InlineData(10.01, 0, null)]
    [InlineData(1.0, 10.01, null)]
    [InlineData(1.0, -0.5, null)]
    [InlineData(1.0, 0, "ab")]
    public void Update_RejectsOutOfRangeLayout(double lineSpacing, double leftOffsetCm, string? marker)
    {
        var t = CreateWithLayout();

        Assert.ThrowsAny<ArgumentException>(() => t.Update(
            "التحليل",
            null,
            true,
            null,
            false,
            (decimal)lineSpacing,
            (decimal)leftOffsetCm,
            marker));
    }

    [Fact]
    public void Update_InvalidLayout_LeavesPreviousValuesUnchanged()
    {
        var t = CreateWithLayout(2.0m, 1.0m, "*");

        Assert.Throws<ArgumentOutOfRangeException>(() => t.Update("جديد", null, true, null, false, 99m));

        Assert.Equal(2.0m, t.LineSpacing);
        Assert.Equal(1.0m, t.LeftOffsetCm);
        Assert.Equal("*", t.MarkerSymbol);
    }
}