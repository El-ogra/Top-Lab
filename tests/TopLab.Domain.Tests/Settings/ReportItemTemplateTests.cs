using TopLab.Domain.Settings;
using Xunit;

namespace TopLab.Domain.Tests.Settings;

public class ReportItemTemplateTests
{
    [Fact]
    public void Create_Valid_StoresTrimmedValues()
    {
        var t = ReportItemTemplate.Create(
            "TestName", "  التحليل  ", "  المدى  ", showHide: false, listMembership: "  A  ", freeTextOverride: true);

        Assert.Equal("TestName", t.ItemKey);
        Assert.Equal("التحليل", t.Label);
        Assert.Equal("المدى", t.ReferenceValueExpression);
        Assert.False(t.ShowHide);
        Assert.Equal("A", t.ListMembership);
        Assert.True(t.FreeTextOverride);
    }

    [Fact]
    public void Create_Defaults_AreVisibleStructured()
    {
        var t = ReportItemTemplate.Create("TestName", "التحليل");

        Assert.True(t.ShowHide);
        Assert.False(t.FreeTextOverride);
        Assert.Null(t.ReferenceValueExpression);
        Assert.Null(t.ListMembership);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_BlankItemKey_Throws(string key)
    {
        Assert.Throws<ArgumentException>(() => ReportItemTemplate.Create(key, "التحليل"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_BlankLabel_Throws(string label)
    {
        Assert.Throws<ArgumentException>(() => ReportItemTemplate.Create("TestName", label));
    }

    [Fact]
    public void Create_OverLongLabel_Throws()
    {
        var label = new string('ع', ReportItemTemplate.MaxLabelLength + 1);
        Assert.Throws<ArgumentException>(() => ReportItemTemplate.Create("TestName", label));
    }

    [Fact]
    public void Create_OverLongReferenceExpression_Throws()
    {
        var value = new string('x', ReportItemTemplate.MaxReferenceValueExpressionLength + 1);
        Assert.Throws<ArgumentException>(() => ReportItemTemplate.Create("TestName", "التحليل", value));
    }

    [Fact]
    public void Create_OverLongListMembership_Throws()
    {
        var value = new string('x', ReportItemTemplate.MaxListMembershipLength + 1);
        Assert.Throws<ArgumentException>(() => ReportItemTemplate.Create("TestName", "التحليل", null, true, value));
    }

    [Fact]
    public void Create_BlankOptional_NormalizesToNull()
    {
        var t = ReportItemTemplate.Create("TestName", "التحليل", "   ", listMembership: "   ");
        Assert.Null(t.ReferenceValueExpression);
        Assert.Null(t.ListMembership);
    }

    [Fact]
    public void Update_Valid_UpdatesAllButKey()
    {
        var t = ReportItemTemplate.Create("TestName", "التحليل");
        t.Update("اسم جديد", "المدى", showHide: false, listMembership: "L", freeTextOverride: true);

        Assert.Equal("TestName", t.ItemKey);
        Assert.Equal("اسم جديد", t.Label);
        Assert.Equal("المدى", t.ReferenceValueExpression);
        Assert.False(t.ShowHide);
        Assert.Equal("L", t.ListMembership);
        Assert.True(t.FreeTextOverride);
    }

    [Fact]
    public void Update_BlankLabel_Throws()
    {
        var t = ReportItemTemplate.Create("TestName", "التحليل");
        Assert.Throws<ArgumentException>(() => t.Update("  ", null, true, null, false));
    }
}
