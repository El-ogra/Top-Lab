using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Domain.Tests.Tests;

public class TestTests
{
    [Fact]
    public void Create_WithTestCode_SetsProperties()
    {
        var t = Test.Create(TestId.Create(1), "CBC", "CBC Report", "CBC Receipt", "CBC", 60, 100m, isSentOut: true, sentOutCostPrice: 5m);

        Assert.Equal("CBC", t.Name);
        Assert.Equal("CBC Report", t.ReportName);
        Assert.Equal("CBC Receipt", t.ReceiptName);
        Assert.Equal("CBC", t.TestCode);
        Assert.Equal(60, t.CompletionDurationMinutes);
        Assert.Equal(100m, t.PatientPrice);
        Assert.True(t.IsSentOut);
        Assert.True(t.IsActive);
    }

    [Fact]
    public void Create_TrimsTestCodeAndNames()
    {
        var t = Test.Create(TestId.Create(1), "  CBC  ", "  CBC Report  ", "  CBC Receipt  ", "  CBC01  ", 60, 100m);

        Assert.Equal("CBC", t.Name);
        Assert.Equal("CBC01", t.TestCode);
    }

    [Fact]
    public void Create_IsActiveParameter_False()
    {
        var t = Test.Create(TestId.Create(1), "CBC", "R", "Rec", "CBC01", 60, 100m, isActive: false);

        Assert.False(t.IsActive);
    }

    [Fact]
    public void Create_TestCodeWhiteSpace_Throws()
    {
        Assert.Throws<ArgumentException>(() => Test.Create(TestId.Create(1), "CBC", "R", "Rec", "   ", 60, 100m));
    }

    [Fact]
    public void Create_TestCodeNull_Throws()
    {
        Assert.Throws<ArgumentException>(() => Test.Create(TestId.Create(1), "CBC", "R", "Rec", null!, 60, 100m));
    }

    [Fact]
    public void Create_TestCodeTooLong_Throws()
    {
        var longCode = new string('X', Test.MaxTestCodeLength + 1);

        Assert.Throws<ArgumentException>(() => Test.Create(TestId.Create(1), "CBC", "R", "Rec", longCode, 60, 100m));
    }

    [Fact]
    public void Create_WhiteSpaceName_Throws()
    {
        Assert.Throws<ArgumentException>(() => Test.Create(TestId.Create(1), " ", "R", "Rec", "CBC", 60, 100m));
    }

    [Fact]
    public void Create_ZeroDuration_Throws()
    {
        Assert.Throws<ArgumentException>(() => Test.Create(TestId.Create(1), "CBC", "R", "Rec", "CBC", 0, 100m));
    }

    [Fact]
    public void Create_SentOut_WithoutCost_Throws()
    {
        Assert.Throws<ArgumentException>(() => Test.Create(TestId.Create(1), "CBC", "R", "Rec", "CBC", 60, 100m, isSentOut: true, sentOutCostPrice: null));
    }

    [Fact]
    public void Create_NegativePatientPrice_Throws()
    {
        Assert.Throws<ArgumentException>(() => Test.Create(TestId.Create(1), "CBC", "R", "Rec", "CBC", 60, -1m));
    }

    [Fact]
    public void Update_WithTestCode_UpdatesFields()
    {
        var t = Test.Create(TestId.Create(1), "CBC", "CBC Report", "CBC Receipt", "CBC", 60, 100m);

        t.Update("CBC2", "CBC2 Report", "CBC2 Receipt", "CBC2", 90, 150m, null, "12345", true, 10m, 20m);

        Assert.Equal("CBC2", t.Name);
        Assert.Equal("CBC2", t.TestCode);
        Assert.Equal(90, t.CompletionDurationMinutes);
        Assert.Equal(150m, t.PatientPrice);
        Assert.Equal("12345", t.Barcode);
        Assert.True(t.IsSentOut);
    }

    [Fact]
    public void Update_TestCodeWhiteSpace_Throws()
    {
        var t = Test.Create(TestId.Create(1), "CBC", "R", "Rec", "CBC", 60, 100m);

        Assert.Throws<ArgumentException>(() => t.Update("CBC2", "R", "Rec", "   ", 60, 100m, null, null, false, null, null));
    }

    [Fact]
    public void Update_TestCodeTooLong_Throws()
    {
        var t = Test.Create(TestId.Create(1), "CBC", "R", "Rec", "CBC", 60, 100m);
        var longCode = new string('X', Test.MaxTestCodeLength + 1);

        Assert.Throws<ArgumentException>(() => t.Update("CBC2", "R", "Rec", longCode, 60, 100m, null, null, false, null, null));
    }

    [Fact]
    public void Update_NegativePatientPrice_Throws()
    {
        var t = Test.Create(TestId.Create(1), "CBC", "R", "Rec", "CBC", 60, 100m);

        Assert.Throws<ArgumentException>(() => t.Update("CBC2", "R", "Rec", "CBC", 60, -1m, null, null, false, null, null));
    }

    [Fact]
    public void Update_ZeroDuration_Throws()
    {
        var t = Test.Create(TestId.Create(1), "CBC", "R", "Rec", "CBC", 60, 100m);

        Assert.Throws<ArgumentException>(() => t.Update("CBC2", "R", "Rec", "CBC", 0, 100m, null, null, false, null, null));
    }

    [Fact]
    public void Deactivate_SetsIsActiveFalse()
    {
        var t = Test.Create(TestId.Create(1), "CBC", "R", "Rec", "CBC", 60, 100m);
        Assert.True(t.IsActive);

        t.Deactivate();

        Assert.False(t.IsActive);
    }

    [Fact]
    public void Reactivate_SetsIsActiveTrue()
    {
        var t = Test.Create(TestId.Create(1), "CBC", "R", "Rec", "CBC", 60, 100m, isActive: false);
        Assert.False(t.IsActive);

        t.Reactivate();

        Assert.True(t.IsActive);
    }
}