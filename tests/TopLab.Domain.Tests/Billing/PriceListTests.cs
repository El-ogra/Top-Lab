using TopLab.Domain.Billing;
using TopLab.Domain.Common.Ids;
using Xunit;

namespace TopLab.Domain.Tests.Billing;

public class PriceListTests
{
    [Fact]
    public void Create_Valid()
    {
        var list = PriceList.Create(PriceListId.Create(1), "Default");

        Assert.Equal("Default", list.Name);
        Assert.Empty(list.Items);
    }

    [Fact]
    public void Create_TrimsName()
    {
        var list = PriceList.Create(PriceListId.Create(1), "  Default  ");

        Assert.Equal("Default", list.Name);
    }

    [Fact]
    public void Create_EmptyName_Throws()
    {
        Assert.Throws<ArgumentException>(() => PriceList.Create(PriceListId.Create(1), " "));
    }

    [Fact]
    public void Rename_Valid()
    {
        var list = PriceList.Create(PriceListId.Create(1), "Default");

        list.Rename("Insurance");

        Assert.Equal("Insurance", list.Name);
    }

    [Fact]
    public void Rename_TrimsName()
    {
        var list = PriceList.Create(PriceListId.Create(1), "Default");

        list.Rename("  Insurance  ");

        Assert.Equal("Insurance", list.Name);
    }

    [Fact]
    public void Rename_EmptyName_Throws()
    {
        var list = PriceList.Create(PriceListId.Create(1), "Default");

        Assert.Throws<ArgumentException>(() => list.Rename(""));
    }

    [Fact]
    public void AddItem_AddsTestWithPrice()
    {
        var list = PriceList.Create(PriceListId.Create(1), "Default");
        var testId = TestId.Create(5);

        list.AddItem(testId, 12.50m);

        Assert.True(list.ContainsTest(testId));
        Assert.Single(list.Items);
        Assert.Equal(12.50m, list.Items.Single().Price);
    }

    [Fact]
    public void AddItem_ZeroPrice_Accepted()
    {
        var list = PriceList.Create(PriceListId.Create(1), "Default");

        list.AddItem(TestId.Create(5), 0m);

        Assert.Single(list.Items);
    }

    [Fact]
    public void AddItem_NegativePrice_Throws()
    {
        var list = PriceList.Create(PriceListId.Create(1), "Default");

        Assert.Throws<ArgumentException>(() => list.AddItem(TestId.Create(5), -1m));
    }

    [Fact]
    public void AddItem_DuplicateTest_Throws()
    {
        var list = PriceList.Create(PriceListId.Create(1), "Default");
        var testId = TestId.Create(5);

        list.AddItem(testId, 10m);

        Assert.Throws<ArgumentException>(() => list.AddItem(testId, 20m));
    }

    [Fact]
    public void SetItemPrice_AbsentTest_Adds()
    {
        var list = PriceList.Create(PriceListId.Create(1), "Default");
        var testId = TestId.Create(5);

        list.SetItemPrice(testId, 10m);

        Assert.True(list.ContainsTest(testId));
        Assert.Equal(10m, list.Items.Single().Price);
    }

    [Fact]
    public void SetItemPrice_ExistingTest_UpdatesPrice()
    {
        var list = PriceList.Create(PriceListId.Create(1), "Default");
        var testId = TestId.Create(5);

        list.AddItem(testId, 10m);
        list.SetItemPrice(testId, 25m);

        Assert.Single(list.Items);
        Assert.Equal(25m, list.Items.Single().Price);
    }

    [Fact]
    public void SetItemPrice_ExistingTest_NegativePrice_Throws()
    {
        var list = PriceList.Create(PriceListId.Create(1), "Default");
        var testId = TestId.Create(5);

        list.AddItem(testId, 10m);

        Assert.Throws<ArgumentException>(() => list.SetItemPrice(testId, -5m));
    }

    [Fact]
    public void RemoveItem_RemovesTest()
    {
        var list = PriceList.Create(PriceListId.Create(1), "Default");
        var testId = TestId.Create(5);

        list.AddItem(testId, 10m);
        list.RemoveItem(testId);

        Assert.Empty(list.Items);
        Assert.False(list.ContainsTest(testId));
    }

    [Fact]
    public void RemoveItem_AbsentTest_Throws()
    {
        var list = PriceList.Create(PriceListId.Create(1), "Default");

        Assert.Throws<ArgumentException>(() => list.RemoveItem(TestId.Create(5)));
    }

    [Fact]
    public void ContainsTest_ReturnsTrueForMember()
    {
        var list = PriceList.Create(PriceListId.Create(1), "Default");
        var testId = TestId.Create(5);

        list.AddItem(testId, 10m);

        Assert.True(list.ContainsTest(testId));
    }

    [Fact]
    public void ContainsTest_ReturnsFalseForNonMember()
    {
        var list = PriceList.Create(PriceListId.Create(1), "Default");

        Assert.False(list.ContainsTest(TestId.Create(5)));
    }
}
