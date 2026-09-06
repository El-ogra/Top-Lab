using Microsoft.EntityFrameworkCore;
using TopLab.Domain.Patients;
using TopLab.Infrastructure.Persistence;
using TopLab.Infrastructure.Tests.Common;
using Xunit;

namespace TopLab.Infrastructure.Tests.Persistence.Configurations;

public class F5ConfigurationTests
{
    private static Microsoft.EntityFrameworkCore.Metadata.IEntityType GetEntityType<T>() where T : class
    {
        using var ctx = new ApplicationDbContext(InMemoryContextFactory.Create());
        var et = ctx.Model.FindEntityType(typeof(T));
        Assert.NotNull(et);
        return et!;
    }

    [Fact]
    public void Patient_HasExpectedColumns()
    {
        var et = GetEntityType<Patient>();
        Assert.NotNull(et.FindProperty(nameof(Patient.FullName)));
        Assert.NotNull(et.FindProperty(nameof(Patient.LabId)));
        Assert.NotNull(et.FindProperty(nameof(Patient.RegistrationDateUtc)));
        var idx = et.GetIndexes().FirstOrDefault(i => i.Properties.Any(p => p.Name == nameof(Patient.LabId)));
        Assert.NotNull(idx);
    }

    [Fact]
    public void PatientPhoneNumber_HasIndexOnPhoneNumber()
    {
        var et = GetEntityType<PatientPhoneNumber>();
        var idx = et.GetIndexes().FirstOrDefault(i => i.Properties.Any(p => p.Name == nameof(PatientPhoneNumber.PhoneNumber)));
        Assert.NotNull(idx);
    }

    [Fact]
    public void Test_HasDecimalPrecision()
    {
        var et = GetEntityType<TopLab.Domain.Tests.Test>();
        var prop = et.FindProperty(nameof(TopLab.Domain.Tests.Test.PatientPrice));
        Assert.NotNull(prop);
        Assert.Equal(18, prop.GetPrecision());
        Assert.Equal(2, prop.GetScale());
    }

    [Fact]
    public void ReferenceRange_HasDecimalPrecision_18_4()
    {
        var et = GetEntityType<TopLab.Domain.Tests.ReferenceRange>();
        var prop = et.FindProperty(nameof(TopLab.Domain.Tests.ReferenceRange.MinValue));
        Assert.Equal(18, prop!.GetPrecision());
        Assert.Equal(4, prop.GetScale());
    }

    [Fact]
    public void SystemSettings_ShouldHaveSingletonSeed()
    {
        using var ctx = new ApplicationDbContext(InMemoryContextFactory.Create());
        ctx.Database.EnsureCreated();
        var count = ctx.Set<TopLab.Domain.Settings.SystemSettings>().Count();
        Assert.Equal(1, count);
    }

    [Fact]
    public void Permission_SeedCount_Is13()
    {
        using var ctx = new ApplicationDbContext(InMemoryContextFactory.Create());
        ctx.Database.EnsureCreated();
        var perms = ctx.Set<TopLab.Domain.Users.Permission>().ToList();
        Assert.Equal(13, perms.Count);
    }

    [Fact]
    public void EnvelopePrintItemPosition_SeedCount_Is4()
    {
        using var ctx = new ApplicationDbContext(InMemoryContextFactory.Create());
        ctx.Database.EnsureCreated();
        Assert.Equal(4, ctx.Set<TopLab.Domain.Settings.EnvelopePrintItemPosition>().Count());
    }

    [Fact]
    public void PatientTest_HasCompositeIndex()
    {
        var et = GetEntityType<TopLab.Domain.Results.PatientTest>();
        var idx = et.GetIndexes().FirstOrDefault(i => i.Properties.Count == 3);
        Assert.NotNull(idx);
    }

    [Fact]
    public void Test_HasTestCodeColumn_UniqueIndex()
    {
        var et = GetEntityType<TopLab.Domain.Tests.Test>();
        var prop = et.FindProperty(nameof(TopLab.Domain.Tests.Test.TestCode));
        Assert.NotNull(prop);
        Assert.False(prop.IsNullable);
        Assert.Equal(50, prop.GetMaxLength());
        var idx = et.GetIndexes().FirstOrDefault(i => i.Properties.Any(p => p.Name == nameof(TopLab.Domain.Tests.Test.TestCode)));
        Assert.NotNull(idx);
        Assert.True(idx!.IsUnique);
    }

    [Fact]
    public void Test_HasIsActiveColumn_DefaultTrue()
    {
        var et = GetEntityType<TopLab.Domain.Tests.Test>();
        var prop = et.FindProperty(nameof(TopLab.Domain.Tests.Test.IsActive));
        Assert.NotNull(prop);
        Assert.False(prop.IsNullable);
        Assert.Equal(true, prop.GetDefaultValue());
    }

    [Fact]
    public void TestGroup_HasIsActiveColumn_DefaultTrue()
    {
        var et = GetEntityType<TopLab.Domain.Tests.TestGroup>();
        var prop = et.FindProperty(nameof(TopLab.Domain.Tests.TestGroup.IsActive));
        Assert.NotNull(prop);
        Assert.False(prop.IsNullable);
        Assert.Equal(true, prop.GetDefaultValue());
    }

    [Fact]
    public void ExternalEntity_HasExpectedMapping()
    {
        var et = GetEntityType<TopLab.Domain.ExternalEntities.ExternalEntity>();

        var name = et.FindProperty(nameof(TopLab.Domain.ExternalEntities.ExternalEntity.Name));
        Assert.NotNull(name);
        Assert.False(name!.IsNullable);
        Assert.Equal(200, name.GetMaxLength());

        var city = et.FindProperty(nameof(TopLab.Domain.ExternalEntities.ExternalEntity.City));
        Assert.NotNull(city);
        Assert.True(city!.IsNullable);
        Assert.Equal(100, city.GetMaxLength());

        var code = et.FindProperty(nameof(TopLab.Domain.ExternalEntities.ExternalEntity.GeneratedIdCode));
        Assert.NotNull(code);
        Assert.True(code!.IsNullable);
        Assert.Equal(50, code.GetMaxLength());

        var priceListId = et.FindProperty(nameof(TopLab.Domain.ExternalEntities.ExternalEntity.PriceListId));
        Assert.NotNull(priceListId);
        Assert.True(priceListId!.IsNullable);

        var percent = et.FindProperty(nameof(TopLab.Domain.ExternalEntities.ExternalEntity.DiscountOrCommissionPercent));
        Assert.NotNull(percent);
        Assert.True(percent!.IsNullable);
        Assert.Equal(5, percent.GetPrecision());
        Assert.Equal(2, percent.GetScale());

        var typeIndex = et.GetIndexes().FirstOrDefault(i =>
            i.Properties.Any(p => p.Name == nameof(TopLab.Domain.ExternalEntities.ExternalEntity.EntityType)));
        Assert.NotNull(typeIndex);
    }

    [Fact]
    public void ExternalEntity_HasNoUniqueIndexOnGeneratedIdCode()
    {
        var et = GetEntityType<TopLab.Domain.ExternalEntities.ExternalEntity>();

        Assert.DoesNotContain(et.GetIndexes(), i =>
            i.IsUnique && i.Properties.Any(p => p.Name == nameof(TopLab.Domain.ExternalEntities.ExternalEntity.GeneratedIdCode)));
    }
}
