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
}
