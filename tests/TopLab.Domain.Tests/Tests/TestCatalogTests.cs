using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Domain.Tests.Tests;

public class TestCatalogTests
{
    [Fact]
    public void Test_Create_Valid()
    {
        var t = Test.Create(TestId.Create(1), "CBC", "CBC Report", "CBC Receipt", 60, 100m);
        Assert.Equal("CBC", t.Name);
        Assert.False(t.IsSentOut);
    }

    [Fact]
    public void Test_Create_SentOut_WithoutCost_Throws()
    {
        Assert.Throws<ArgumentException>(() => Test.Create(TestId.Create(1), "CBC", "R", "Rec", 60, 100m, isSentOut: true, sentOutCostPrice: null));
    }

    [Fact]
    public void Test_Create_ZeroDuration_Throws()
    {
        Assert.Throws<ArgumentException>(() => Test.Create(TestId.Create(1), "CBC", "R", "Rec", 0, 100m));
    }

    [Fact]
    public void TestGroup_Create_Valid()
    {
        var g = TestGroup.Create(TestGroupId.Create(1), "Kidney");
        Assert.Equal("Kidney", g.Name);
    }

    [Fact]
    public void Antibiotic_Create_Valid()
    {
        var a = Antibiotic.Create(AntibioticId.Create(1), "Amoxicillin", isPregnancyFlagged: true);
        Assert.True(a.IsPregnancyFlagged);
    }
}
