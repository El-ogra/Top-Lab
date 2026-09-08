using TopLab.Domain.Common.Ids;
using TopLab.Domain.Results;
using Xunit;

namespace TopLab.Domain.Tests.Results;

public class PatientTestTests
{
    [Fact]
    public void Create_Valid()
    {
        var pt = PatientTest.Create(PatientTestId.Create(1), PatientId.Create(1), TestId.Create(1), 150m);
        Assert.Equal(150m, pt.PriceAtOrderTime);
        Assert.False(pt.IsReviewed);
        Assert.Equal(0, pt.PrintCount);
    }

    [Fact]
    public void EnterResult_SetsFlag()
    {
        var pt = PatientTest.Create(PatientTestId.Create(1), PatientId.Create(1), TestId.Create(1), 100m);
        pt.EnterResult("5.5", TopLab.Domain.Common.Enums.ResultFlag.Normal, 42, DateTime.UtcNow);
        Assert.Equal("5.5", pt.ResultValue);
        Assert.Equal(TopLab.Domain.Common.Enums.ResultFlag.Normal, pt.ResultFlag);
    }

    [Fact]
    public void MarkReviewed_Sets()
    {
        var pt = PatientTest.Create(PatientTestId.Create(1), PatientId.Create(1), TestId.Create(1), 100m);
        pt.EnterResult("5.5", TopLab.Domain.Common.Enums.ResultFlag.Normal, 1, DateTime.UtcNow);
        pt.MarkReviewed(1, DateTime.UtcNow);
        Assert.True(pt.IsReviewed);
    }

    [Fact]
    public void MarkPrinted_Increments()
    {
        var pt = PatientTest.Create(PatientTestId.Create(1), PatientId.Create(1), TestId.Create(1), 100m);
        pt.EnterResult("5.5", TopLab.Domain.Common.Enums.ResultFlag.Normal, 1, DateTime.UtcNow);
        pt.MarkReviewed(1, DateTime.UtcNow);
        pt.MarkPrinted(1, DateTime.UtcNow);
        pt.MarkPrinted(1, DateTime.UtcNow);
        Assert.Equal(2, pt.PrintCount);
        Assert.True(pt.IsPrinted);
    }

    [Fact]
    public void UpdateSampleFlags_RoundTrips_AllSixBooleans()
    {
        var pt = PatientTest.Create(PatientTestId.Create(1), PatientId.Create(1), TestId.Create(1), 100m);
        pt.UpdateSampleFlags(isUrine: true, isStool: false, isBlood: true, isSemen: false, isCsf: false, isTakenOutsideLab: true);
        Assert.True(pt.IsUrine);
        Assert.False(pt.IsStool);
        Assert.True(pt.IsBlood);
        Assert.False(pt.IsSemen);
        Assert.False(pt.IsCsf);
        Assert.True(pt.IsTakenOutsideLab);
    }

    [Fact]
    public void MarkSampleDrawn_RoundTrips()
    {
        var pt = PatientTest.Create(PatientTestId.Create(1), PatientId.Create(1), TestId.Create(1), 100m);
        pt.MarkSampleDrawn(DateTime.UtcNow);
        Assert.True(pt.IsSampleDrawn);
        Assert.NotNull(pt.SampleDrawnAtUtc);
    }

    [Fact]
    public void EnterResult_AfterReview_Throws()
    {
        var pt = PatientTest.Create(PatientTestId.Create(1), PatientId.Create(1), TestId.Create(1), 100m);
        pt.EnterResult("5.5", TopLab.Domain.Common.Enums.ResultFlag.Normal, 1, DateTime.UtcNow);
        pt.MarkReviewed(1, DateTime.UtcNow);
        var ex = Assert.Throws<InvalidOperationException>(() =>
            pt.EnterResult("6.6", TopLab.Domain.Common.Enums.ResultFlag.High, 1, DateTime.UtcNow));
        Assert.Equal("Result is reviewed; unreview first.", ex.Message);
    }

    [Fact]
    public void MarkEntered_Rejects_WhenReviewed()
    {
        var pt = PatientTest.Create(PatientTestId.Create(1), PatientId.Create(1), TestId.Create(1), 100m);
        pt.EnterResult("5.5", TopLab.Domain.Common.Enums.ResultFlag.Normal, 1, DateTime.UtcNow);
        pt.MarkReviewed(1, DateTime.UtcNow);
        var ex = Assert.Throws<InvalidOperationException>(() => pt.MarkEntered(2, DateTime.UtcNow));
        Assert.Equal("Result is reviewed; unreview first.", ex.Message);
    }

    [Fact]
    public void MarkEntered_Stamps_WithoutTouchingResultValue()
    {
        var pt = PatientTest.Create(PatientTestId.Create(1), PatientId.Create(1), TestId.Create(1), 100m);
        pt.MarkEntered(7, DateTime.UtcNow);
        Assert.Equal(7, pt.EnteredByUserId);
        Assert.NotNull(pt.EnteredAtUtc);
        Assert.Null(pt.ResultValue);
        Assert.Null(pt.ResultFlag);
    }

    [Fact]
    public void ClearResult_Resets_AllFields()
    {
        var pt = PatientTest.Create(PatientTestId.Create(1), PatientId.Create(1), TestId.Create(1), 100m);
        pt.EnterResult("5.5", TopLab.Domain.Common.Enums.ResultFlag.Normal, 42, DateTime.UtcNow, "note");
        pt.ClearResult();
        Assert.Null(pt.ResultValue);
        Assert.Null(pt.ResultFlag);
        Assert.Null(pt.Notes);
        Assert.Null(pt.EnteredByUserId);
        Assert.Null(pt.EnteredAtUtc);
    }

    [Theory]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(false, false, true)]
    public void ClearResult_Locked_Throws(bool reviewed, bool printed, bool delivered)
    {
        var pt = PatientTest.Create(PatientTestId.Create(1), PatientId.Create(1), TestId.Create(1), 100m);
        pt.EnterResult("5.5", TopLab.Domain.Common.Enums.ResultFlag.Normal, 1, DateTime.UtcNow);
        if (reviewed || printed || delivered)
        {
            pt.MarkReviewed(1, DateTime.UtcNow);
        }

        if (printed || delivered)
        {
            pt.MarkPrinted(1, DateTime.UtcNow);
        }

        if (delivered)
        {
            pt.MarkDelivered(1, DateTime.UtcNow);
        }

        var ex = Assert.Throws<InvalidOperationException>(() => pt.ClearResult());
        Assert.Equal("Result is locked.", ex.Message);
    }

    [Fact]
    public void Unreview_Clears_ReviewColumns()
    {
        var pt = PatientTest.Create(PatientTestId.Create(1), PatientId.Create(1), TestId.Create(1), 100m);
        pt.EnterResult("5.5", TopLab.Domain.Common.Enums.ResultFlag.Normal, 1, DateTime.UtcNow);
        pt.MarkReviewed(5, DateTime.UtcNow);
        pt.Unreview();
        Assert.False(pt.IsReviewed);
        Assert.Null(pt.ReviewedByUserId);
        Assert.Null(pt.ReviewedAtUtc);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void Unreview_AfterPrintOrDeliver_Throws(bool printed, bool delivered)
    {
        var pt = PatientTest.Create(PatientTestId.Create(1), PatientId.Create(1), TestId.Create(1), 100m);
        pt.EnterResult("5.5", TopLab.Domain.Common.Enums.ResultFlag.Normal, 1, DateTime.UtcNow);
        pt.MarkReviewed(1, DateTime.UtcNow);
        if (printed || delivered)
        {
            pt.MarkPrinted(1, DateTime.UtcNow);
        }

        if (delivered)
        {
            pt.MarkDelivered(1, DateTime.UtcNow);
        }

        var ex = Assert.Throws<InvalidOperationException>(() => pt.Unreview());
        Assert.Equal("Printed or delivered results cannot be un-reviewed.", ex.Message);
    }

    [Fact]
    public void MarkReviewed_WithoutEntry_Throws()
    {
        var pt = PatientTest.Create(PatientTestId.Create(1), PatientId.Create(1), TestId.Create(1), 100m);
        var ex = Assert.Throws<InvalidOperationException>(() => pt.MarkReviewed(1, DateTime.UtcNow));
        Assert.Equal("Result not entered.", ex.Message);
    }

    [Fact]
    public void MarkPrinted_WithoutReview_Throws()
    {
        var pt = PatientTest.Create(PatientTestId.Create(1), PatientId.Create(1), TestId.Create(1), 100m);
        pt.EnterResult("5.5", TopLab.Domain.Common.Enums.ResultFlag.Normal, 1, DateTime.UtcNow);
        var ex = Assert.Throws<InvalidOperationException>(() => pt.MarkPrinted(1, DateTime.UtcNow));
        Assert.Equal("Result not reviewed.", ex.Message);
    }

    [Fact]
    public void MarkPrinted_WithoutEntry_Throws()
    {
        var pt = PatientTest.Create(PatientTestId.Create(1), PatientId.Create(1), TestId.Create(1), 100m);
        var ex = Assert.Throws<InvalidOperationException>(() => pt.MarkPrinted(1, DateTime.UtcNow));
        Assert.Equal("Result not reviewed.", ex.Message);
    }

    [Fact]
    public void MarkDelivered_WithoutPrint_Throws()
    {
        var pt = PatientTest.Create(PatientTestId.Create(1), PatientId.Create(1), TestId.Create(1), 100m);
        pt.EnterResult("5.5", TopLab.Domain.Common.Enums.ResultFlag.Normal, 1, DateTime.UtcNow);
        pt.MarkReviewed(1, DateTime.UtcNow);
        var ex = Assert.Throws<InvalidOperationException>(() => pt.MarkDelivered(1, DateTime.UtcNow));
        Assert.Equal("Result not printed.", ex.Message);
    }
}
