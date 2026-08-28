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
        pt.MarkReviewed(1, DateTime.UtcNow);
        Assert.True(pt.IsReviewed);
    }

    [Fact]
    public void MarkPrinted_Increments()
    {
        var pt = PatientTest.Create(PatientTestId.Create(1), PatientId.Create(1), TestId.Create(1), 100m);
        pt.MarkPrinted(1, DateTime.UtcNow);
        pt.MarkPrinted(1, DateTime.UtcNow);
        Assert.Equal(2, pt.PrintCount);
        Assert.True(pt.IsPrinted);
    }
}
