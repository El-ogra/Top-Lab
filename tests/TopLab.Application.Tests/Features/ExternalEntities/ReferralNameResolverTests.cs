using TopLab.Application.Features.ExternalEntities.Common;
using TopLab.Domain.Common.Enums;
using Xunit;

namespace TopLab.Application.Tests.Features.ExternalEntities;

public class ReferralNameResolverTests
{
    [Fact]
    public void Resolve_NonEmptyName_ReturnsTrimmedName()
    {
        Assert.Equal("Dr. Ahmed", ReferralNameResolver.Resolve("  Dr. Ahmed  ", Sex.Male));
        Assert.Equal("Dr. Ahmed", ReferralNameResolver.Resolve("Dr. Ahmed", Sex.Female));
    }

    [Fact]
    public void Resolve_NullNameMale_ReturnsHimself()
    {
        Assert.Equal("Himself", ReferralNameResolver.Resolve(null, Sex.Male));
    }

    [Fact]
    public void Resolve_NullNameFemale_ReturnsHerself()
    {
        Assert.Equal("Herself", ReferralNameResolver.Resolve(null, Sex.Female));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Resolve_WhitespaceName_ReturnsSexDefault(string name)
    {
        Assert.Equal("Himself", ReferralNameResolver.Resolve(name, Sex.Male));
        Assert.Equal("Herself", ReferralNameResolver.Resolve(name, Sex.Female));
    }
}
