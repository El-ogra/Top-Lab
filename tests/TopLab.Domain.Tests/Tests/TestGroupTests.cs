using TopLab.Domain.Common.Ids;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Domain.Tests.Tests;

public class TestGroupTests
{
    [Fact]
    public void Create_Valid()
    {
        var g = TestGroup.Create(TestGroupId.Create(1), "Kidney");

        Assert.Equal("Kidney", g.Name);
        Assert.True(g.IsActive);
    }

    [Fact]
    public void Create_TrimsName()
    {
        var g = TestGroup.Create(TestGroupId.Create(1), "  Kidney  ");

        Assert.Equal("Kidney", g.Name);
    }

    [Fact]
    public void Create_EmptyName_Throws()
    {
        Assert.Throws<ArgumentException>(() => TestGroup.Create(TestGroupId.Create(1), " "));
    }

    [Fact]
    public void Create_IsActiveFalse_Parameter()
    {
        var g = TestGroup.Create(TestGroupId.Create(1), "Kidney", isActive: false);

        Assert.False(g.IsActive);
    }

    [Fact]
    public void Rename_Valid()
    {
        var g = TestGroup.Create(TestGroupId.Create(1), "Kidney");

        g.Rename("  Nephrology  ");

        Assert.Equal("Nephrology", g.Name);
    }

    [Fact]
    public void Rename_EmptyName_Throws()
    {
        var g = TestGroup.Create(TestGroupId.Create(1), "Kidney");

        Assert.Throws<ArgumentException>(() => g.Rename("   "));
    }

    [Fact]
    public void Deactivate_SetsIsActiveFalse()
    {
        var g = TestGroup.Create(TestGroupId.Create(1), "Kidney");
        Assert.True(g.IsActive);

        g.Deactivate();

        Assert.False(g.IsActive);
    }

    [Fact]
    public void Reactivate_SetsIsActiveTrue()
    {
        var g = TestGroup.Create(TestGroupId.Create(1), "Kidney", isActive: false);
        Assert.False(g.IsActive);

        g.Reactivate();

        Assert.True(g.IsActive);
    }
}