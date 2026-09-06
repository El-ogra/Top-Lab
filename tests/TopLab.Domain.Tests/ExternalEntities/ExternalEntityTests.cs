using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.ExternalEntities;
using Xunit;

namespace TopLab.Domain.Tests.ExternalEntities;

public class ExternalEntityTests
{
    private static PriceListId ListId => PriceListId.Create(7);

    [Fact]
    public void Create_DoctorWithoutList_SetsProperties()
    {
        var e = ExternalEntity.Create(
            ExternalEntityId.Create(1), EntityType.TreatingDoctor, "  Dr. Ahmed  ",
            city: "Cairo", phone: "0100", discountOrCommissionPercent: 10m);

        Assert.Equal(EntityType.TreatingDoctor, e.EntityType);
        Assert.Equal("Dr. Ahmed", e.Name);
        Assert.Equal("Cairo", e.City);
        Assert.Equal("0100", e.Phone);
        Assert.Null(e.PriceListId);
        Assert.Equal(10m, e.DiscountOrCommissionPercent);
        Assert.Null(e.GeneratedIdCode);
    }

    [Fact]
    public void Create_DoctorWithList_Throws()
    {
        Assert.Throws<ArgumentException>(() => ExternalEntity.Create(
            ExternalEntityId.Create(1), EntityType.TreatingDoctor, "Dr. Ahmed", priceListId: ListId));
    }

    [Fact]
    public void Create_ReferralWithoutList_Throws()
    {
        Assert.Throws<ArgumentException>(() => ExternalEntity.Create(
            ExternalEntityId.Create(1), EntityType.ReferralOrContract, "Entity"));
    }

    [Fact]
    public void Create_ReferralWithList_Succeeds()
    {
        var e = ExternalEntity.Create(
            ExternalEntityId.Create(1), EntityType.ReferralOrContract, "Entity", priceListId: ListId);

        Assert.Equal(ListId, e.PriceListId);
    }

    [Fact]
    public void Create_PartnerLab_WithAndWithoutList_Succeeds()
    {
        var withList = ExternalEntity.Create(
            ExternalEntityId.Create(1), EntityType.PartnerLab, "Lab", priceListId: ListId);
        var withoutList = ExternalEntity.Create(
            ExternalEntityId.Create(2), EntityType.PartnerLab, "Lab");

        Assert.Equal(ListId, withList.PriceListId);
        Assert.Null(withoutList.PriceListId);
    }

    [Fact]
    public void Create_PercentNull_Accepted()
    {
        var e = ExternalEntity.Create(
            ExternalEntityId.Create(1), EntityType.TreatingDoctor, "Dr", discountOrCommissionPercent: null);

        Assert.Null(e.DiscountOrCommissionPercent);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(100.0)]
    [InlineData(12.5)]
    public void Create_PercentBoundaries_Accepted(double percent)
    {
        var expected = (decimal)percent;
        var e = ExternalEntity.Create(
            ExternalEntityId.Create(1), EntityType.TreatingDoctor, "Dr", discountOrCommissionPercent: expected);

        Assert.Equal(expected, e.DiscountOrCommissionPercent);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(-5)]
    [InlineData(100.01)]
    [InlineData(250)]
    public void Create_PercentOutOfRange_Throws(decimal percent)
    {
        Assert.Throws<ArgumentException>(() => ExternalEntity.Create(
            ExternalEntityId.Create(1), EntityType.TreatingDoctor, "Dr", discountOrCommissionPercent: percent));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_NameMissing_Throws(string? name)
    {
        Assert.Throws<ArgumentException>(() => ExternalEntity.Create(
            ExternalEntityId.Create(1), EntityType.TreatingDoctor, name!));
    }

    [Fact]
    public void Create_NameTooLong_Throws()
    {
        var longName = new string('N', ExternalEntity.MaxNameLength + 1);

        Assert.Throws<ArgumentException>(() => ExternalEntity.Create(
            ExternalEntityId.Create(1), EntityType.TreatingDoctor, longName));
    }

    [Fact]
    public void Create_OptionalWhitespace_NormalizedToNull()
    {
        var e = ExternalEntity.Create(
            ExternalEntityId.Create(1), EntityType.TreatingDoctor, "Dr",
            city: "   ", address: "", phone: "  ", fax: null,
            responsiblePersonName: " ", responsiblePersonPhone: "");

        Assert.Null(e.City);
        Assert.Null(e.Address);
        Assert.Null(e.Phone);
        Assert.Null(e.Fax);
        Assert.Null(e.ResponsiblePersonName);
        Assert.Null(e.ResponsiblePersonPhone);
    }

    [Fact]
    public void Create_OptionalValues_Trimmed()
    {
        var e = ExternalEntity.Create(
            ExternalEntityId.Create(1), EntityType.TreatingDoctor, "Dr",
            city: "  Cairo  ", responsiblePersonName: "  Mona  ");

        Assert.Equal("Cairo", e.City);
        Assert.Equal("Mona", e.ResponsiblePersonName);
    }

    [Fact]
    public void Update_HappyPath_ReplacesAllFields()
    {
        var e = ExternalEntity.Create(
            ExternalEntityId.Create(1), EntityType.TreatingDoctor, "Dr",
            city: "Old", discountOrCommissionPercent: 5m);

        e.Update(EntityType.ReferralOrContract, "  New Entity  ",
            city: "Giza", address: "Street", phone: "0111", fax: "0222",
            responsiblePersonName: "Omar", responsiblePersonPhone: "0333",
            priceListId: ListId, discountOrCommissionPercent: 20m);

        Assert.Equal(EntityType.ReferralOrContract, e.EntityType);
        Assert.Equal("New Entity", e.Name);
        Assert.Equal("Giza", e.City);
        Assert.Equal("Street", e.Address);
        Assert.Equal("0111", e.Phone);
        Assert.Equal("0222", e.Fax);
        Assert.Equal("Omar", e.ResponsiblePersonName);
        Assert.Equal("0333", e.ResponsiblePersonPhone);
        Assert.Equal(ListId, e.PriceListId);
        Assert.Equal(20m, e.DiscountOrCommissionPercent);
    }

    [Fact]
    public void Update_DoctorToReferralWithoutList_Throws()
    {
        var e = ExternalEntity.Create(
            ExternalEntityId.Create(1), EntityType.TreatingDoctor, "Dr");

        Assert.Throws<ArgumentException>(() => e.Update(EntityType.ReferralOrContract, "Dr"));
    }

    [Fact]
    public void Update_ReferralToDoctorWithList_Throws()
    {
        var e = ExternalEntity.Create(
            ExternalEntityId.Create(1), EntityType.ReferralOrContract, "Entity", priceListId: ListId);

        Assert.Throws<ArgumentException>(() => e.Update(
            EntityType.TreatingDoctor, "Dr", priceListId: ListId));
    }

    [Fact]
    public void Update_ReferralToDoctorClearingList_Succeeds()
    {
        var e = ExternalEntity.Create(
            ExternalEntityId.Create(1), EntityType.ReferralOrContract, "Entity", priceListId: ListId);

        e.Update(EntityType.TreatingDoctor, "Dr");

        Assert.Equal(EntityType.TreatingDoctor, e.EntityType);
        Assert.Null(e.PriceListId);
    }

    [Fact]
    public void Update_DoctorAddingList_Throws()
    {
        var e = ExternalEntity.Create(
            ExternalEntityId.Create(1), EntityType.TreatingDoctor, "Dr");

        Assert.Throws<ArgumentException>(() => e.Update(
            EntityType.TreatingDoctor, "Dr", priceListId: ListId));
    }

    [Fact]
    public void Update_PercentOutOfRange_Throws()
    {
        var e = ExternalEntity.Create(
            ExternalEntityId.Create(1), EntityType.TreatingDoctor, "Dr");

        Assert.Throws<ArgumentException>(() => e.Update(
            EntityType.TreatingDoctor, "Dr", discountOrCommissionPercent: 101m));
    }

    [Fact]
    public void Update_NameWhitespace_Throws()
    {
        var e = ExternalEntity.Create(
            ExternalEntityId.Create(1), EntityType.TreatingDoctor, "Dr");

        Assert.Throws<ArgumentException>(() => e.Update(EntityType.TreatingDoctor, "  "));
    }

    [Fact]
    public void RegenerateIdCode_Valid_StoresTrimmed()
    {
        var e = ExternalEntity.Create(
            ExternalEntityId.Create(1), EntityType.TreatingDoctor, "Dr");

        e.RegenerateIdCode("  AB12CD34  ");

        Assert.Equal("AB12CD34", e.GeneratedIdCode);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void RegenerateIdCode_Missing_Throws(string? code)
    {
        var e = ExternalEntity.Create(
            ExternalEntityId.Create(1), EntityType.TreatingDoctor, "Dr");

        Assert.Throws<ArgumentException>(() => e.RegenerateIdCode(code!));
    }

    [Fact]
    public void RegenerateIdCode_TooLong_Throws()
    {
        var e = ExternalEntity.Create(
            ExternalEntityId.Create(1), EntityType.TreatingDoctor, "Dr");
        var longCode = new string('C', ExternalEntity.MaxGeneratedIdCodeLength + 1);

        Assert.Throws<ArgumentException>(() => e.RegenerateIdCode(longCode));
    }

    [Fact]
    public void RegenerateIdCode_OverwritesPriorCode()
    {
        var e = ExternalEntity.Create(
            ExternalEntityId.Create(1), EntityType.TreatingDoctor, "Dr");
        e.RegenerateIdCode("FIRST");

        e.RegenerateIdCode("SECOND");

        Assert.Equal("SECOND", e.GeneratedIdCode);
    }
}
