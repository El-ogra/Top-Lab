using TopLab.Domain.Common.Ids;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Domain.Tests.Tests;

public class CustomGroupTests
{
    [Fact]
    public void Create_Valid()
    {
        var group = CustomGroup.Create(CustomGroupId.Create(1), "Checkup");

        Assert.Equal("Checkup", group.Name);
        Assert.Empty(group.Items);
    }

    [Fact]
    public void Create_TrimsName()
    {
        var group = CustomGroup.Create(CustomGroupId.Create(1), "  Checkup  ");

        Assert.Equal("Checkup", group.Name);
    }

    [Fact]
    public void Create_EmptyName_Throws()
    {
        Assert.Throws<ArgumentException>(() => CustomGroup.Create(CustomGroupId.Create(1), " "));
    }

    [Fact]
    public void Rename_Valid()
    {
        var group = CustomGroup.Create(CustomGroupId.Create(1), "Checkup");

        group.Rename("Full Panel");

        Assert.Equal("Full Panel", group.Name);
    }

    [Fact]
    public void Rename_TrimsName()
    {
        var group = CustomGroup.Create(CustomGroupId.Create(1), "Checkup");

        group.Rename("  Full Panel  ");

        Assert.Equal("Full Panel", group.Name);
    }

    [Fact]
    public void Rename_EmptyName_Throws()
    {
        var group = CustomGroup.Create(CustomGroupId.Create(1), "Checkup");

        Assert.Throws<ArgumentException>(() => group.Rename(""));
    }

    [Fact]
    public void AddItem_AddsTestWithPrice()
    {
        var group = CustomGroup.Create(CustomGroupId.Create(1), "Checkup");
        var testId = TestId.Create(5);

        group.AddItem(testId, 12.50m);

        Assert.True(group.ContainsTest(testId));
        Assert.Single(group.Items);
        Assert.Equal(12.50m, group.Items.Single().Price);
    }

    [Fact]
    public void AddItem_ZeroPrice_Accepted()
    {
        var group = CustomGroup.Create(CustomGroupId.Create(1), "Checkup");

        group.AddItem(TestId.Create(5), 0m);

        Assert.Single(group.Items);
    }

    [Fact]
    public void AddItem_NegativePrice_Throws()
    {
        var group = CustomGroup.Create(CustomGroupId.Create(1), "Checkup");

        Assert.Throws<ArgumentException>(() => group.AddItem(TestId.Create(5), -1m));
    }

    [Fact]
    public void AddItem_DuplicateTest_Throws()
    {
        var group = CustomGroup.Create(CustomGroupId.Create(1), "Checkup");
        var testId = TestId.Create(5);

        group.AddItem(testId, 10m);

        Assert.Throws<ArgumentException>(() => group.AddItem(testId, 20m));
    }

    [Fact]
    public void SetItemPrice_AbsentTest_Adds()
    {
        var group = CustomGroup.Create(CustomGroupId.Create(1), "Checkup");
        var testId = TestId.Create(5);

        group.SetItemPrice(testId, 10m);

        Assert.True(group.ContainsTest(testId));
        Assert.Equal(10m, group.Items.Single().Price);
    }

    [Fact]
    public void SetItemPrice_ExistingTest_UpdatesPrice()
    {
        var group = CustomGroup.Create(CustomGroupId.Create(1), "Checkup");
        var testId = TestId.Create(5);

        group.AddItem(testId, 10m);
        group.SetItemPrice(testId, 25m);

        Assert.Single(group.Items);
        Assert.Equal(25m, group.Items.Single().Price);
    }

    [Fact]
    public void SetItemPrice_ExistingTest_NegativePrice_Throws()
    {
        var group = CustomGroup.Create(CustomGroupId.Create(1), "Checkup");
        var testId = TestId.Create(5);

        group.AddItem(testId, 10m);

        Assert.Throws<ArgumentException>(() => group.SetItemPrice(testId, -5m));
    }

    [Fact]
    public void RemoveItem_RemovesTest()
    {
        var group = CustomGroup.Create(CustomGroupId.Create(1), "Checkup");
        var testId = TestId.Create(5);

        group.AddItem(testId, 10m);
        group.RemoveItem(testId);

        Assert.Empty(group.Items);
        Assert.False(group.ContainsTest(testId));
    }

    [Fact]
    public void RemoveItem_AbsentTest_Throws()
    {
        var group = CustomGroup.Create(CustomGroupId.Create(1), "Checkup");

        Assert.Throws<ArgumentException>(() => group.RemoveItem(TestId.Create(5)));
    }

    [Fact]
    public void ContainsTest_ReturnsTrueForMember()
    {
        var group = CustomGroup.Create(CustomGroupId.Create(1), "Checkup");
        var testId = TestId.Create(5);

        group.AddItem(testId, 10m);

        Assert.True(group.ContainsTest(testId));
    }

    [Fact]
    public void ContainsTest_ReturnsFalseForNonMember()
    {
        var group = CustomGroup.Create(CustomGroupId.Create(1), "Checkup");

        Assert.False(group.ContainsTest(TestId.Create(5)));
    }
}
