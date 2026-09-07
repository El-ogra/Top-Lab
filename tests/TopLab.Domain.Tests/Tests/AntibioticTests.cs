using TopLab.Domain.Common.Ids;
using Xunit;

namespace TopLab.Domain.Tests.Tests;

public class AntibioticTests
{
    [Fact]
    public void Create_TrimsName_And_StoresFlags()
    {
        var a = Antibiotic.Create(AntibioticId.Create(1), "  Amoxicillin  ",
            isPregnancyFlagged: true, isChildrenFlagged: true);

        Assert.Equal("Amoxicillin", a.Name);
        Assert.True(a.IsPregnancyFlagged);
        Assert.True(a.IsChildrenFlagged);
    }

    [Fact]
    public void Create_DefaultsFlags_ToFalse()
    {
        var a = Antibiotic.Create(AntibioticId.Create(1), "Ciprofloxacin");

        Assert.Equal("Ciprofloxacin", a.Name);
        Assert.False(a.IsPregnancyFlagged);
        Assert.False(a.IsChildrenFlagged);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_NameMissing_Throws(string? name)
    {
        Assert.Throws<ArgumentException>(() =>
            Antibiotic.Create(AntibioticId.Create(1), name!));
    }

    [Fact]
    public void Update_TrimsName_And_MutatesFlags()
    {
        var a = Antibiotic.Create(AntibioticId.Create(1), "Cefotaxime",
            isPregnancyFlagged: false, isChildrenFlagged: false);

        a.Update("  Ceftriaxone  ", isPregnancyFlagged: true, isChildrenFlagged: true);

        Assert.Equal("Ceftriaxone", a.Name);
        Assert.True(a.IsPregnancyFlagged);
        Assert.True(a.IsChildrenFlagged);
    }

    [Fact]
    public void Update_FlagsClearable_ByPassingFalse()
    {
        var a = Antibiotic.Create(AntibioticId.Create(1), "Tetracycline",
            isPregnancyFlagged: true, isChildrenFlagged: true);

        a.Update("Tetracycline", isPregnancyFlagged: false, isChildrenFlagged: false);

        Assert.Equal("Tetracycline", a.Name);
        Assert.False(a.IsPregnancyFlagged);
        Assert.False(a.IsChildrenFlagged);
    }

    [Fact]
    public void Update_TogglesFlagsIndependently()
    {
        var a = Antibiotic.Create(AntibioticId.Create(1), "Gentamicin",
            isPregnancyFlagged: false, isChildrenFlagged: false);

        a.Update("Gentamicin", isPregnancyFlagged: true, isChildrenFlagged: false);

        Assert.True(a.IsPregnancyFlagged);
        Assert.False(a.IsChildrenFlagged);

        a.Update("Gentamicin", isPregnancyFlagged: false, isChildrenFlagged: true);

        Assert.False(a.IsPregnancyFlagged);
        Assert.True(a.IsChildrenFlagged);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_NameMissing_Throws(string? name)
    {
        var a = Antibiotic.Create(AntibioticId.Create(1), "Azithromycin");

        Assert.Throws<ArgumentException>(() =>
            a.Update(name!, isPregnancyFlagged: true, isChildrenFlagged: true));
    }

    [Fact]
    public void Update_DoesNotChangeId()
    {
        var a = Antibiotic.Create(AntibioticId.Create(42), "Ampicillin");

        a.Update("Amoxicillin", isPregnancyFlagged: true, isChildrenFlagged: false);

        Assert.Equal(AntibioticId.Create(42), a.Id);
    }
}