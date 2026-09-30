using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.ExternalEntities;
using Xunit;

namespace TopLab.Domain.Tests.ExternalEntities;

public class ExternalEntityEmailTests
{
    [Fact]
    public void Create_PersistsEmail()
    {
        var e = ExternalEntity.Create(
            ExternalEntityId.Create(1),
            EntityType.TreatingDoctor,
            "Dr. X",
            email: "dr@lab.test");

        Assert.Equal("dr@lab.test", e.Email);
    }

    [Fact]
    public void Update_PersistsEmail()
    {
        var e = ExternalEntity.Create(ExternalEntityId.Create(1), EntityType.TreatingDoctor, "Dr. X");
        e.Update(EntityType.TreatingDoctor, "Dr. X", email: "new@lab.test");

        Assert.Equal("new@lab.test", e.Email);
    }

    [Fact]
    public void Create_EmailTooLong_Throws()
    {
        Assert.Throws<ArgumentException>(() => ExternalEntity.Create(
            ExternalEntityId.Create(1),
            EntityType.TreatingDoctor,
            "Dr. X",
            email: new string('a', 201)));
    }

    [Fact]
    public void ValidatePriceListRule_IsUntouched()
    {
        // SD-8 pin
        Assert.Throws<ArgumentException>(() => ExternalEntity.Create(
            ExternalEntityId.Create(1),
            EntityType.ReferralOrContract,
            "Ref"));
    }
}
