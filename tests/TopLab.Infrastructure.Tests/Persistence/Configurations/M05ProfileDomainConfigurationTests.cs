using Microsoft.EntityFrameworkCore;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;
using TopLab.Infrastructure.Persistence;
using TopLab.Infrastructure.Tests.Common;
using Xunit;

namespace TopLab.Infrastructure.Tests.Persistence.Configurations;

/// <summary>
/// M-05 FK matrix + mapping assertions over the real EF model (F5 conventions).
/// Pins the settled profile domain relationships: Analyte/<see cref="AnalyteReferenceRange"/>
/// one-to-one, snapshot 1:1 by <see cref="ProfileResultItem"/>, printed-item amendment
/// history cascade, and the Restrict guard on the item→Analyte reference.
/// </summary>
public class M05ProfileDomainConfigurationTests
{
    private static Microsoft.EntityFrameworkCore.Metadata.IEntityType GetEntityType<T>() where T : class
    {
        using var ctx = new ApplicationDbContext(InMemoryContextFactory.Create());
        var et = ctx.Model.FindEntityType(typeof(T));
        Assert.NotNull(et);
        return et!;
    }

    [Fact]
    public void Analyte_HasUniqueName_AndUniqueOneToOneLiveRange()
    {
        var analyte = GetEntityType<Analyte>();
        var nameIdx = analyte.GetIndexes().Single(i => i.Properties.Any(p => p.Name == nameof(Analyte.Name)));
        Assert.True(nameIdx.IsUnique);

        var range = GetEntityType<AnalyteReferenceRange>();
        var analyteIdx = range.GetIndexes().Single(i => i.Properties.Any(p => p.Name == nameof(AnalyteReferenceRange.AnalyteId)));
        Assert.True(analyteIdx.IsUnique);

        var fk = range.GetForeignKeys().Single(f => f.PrincipalEntityType.ClrType == typeof(Analyte));
        Assert.Equal(DeleteBehavior.Cascade, fk.DeleteBehavior);
    }

    [Fact]
    public void Profile_HasUniqueTestForeignKey_AndNameIndex()
    {
        var et = GetEntityType<Profile>();
        var testIdx = et.GetIndexes().Single(i => i.Properties.Any(p => p.Name == nameof(Profile.TestId)));
        Assert.True(testIdx.IsUnique);
        Assert.NotNull(et.GetIndexes().FirstOrDefault(i => i.Properties.Any(p => p.Name == nameof(Profile.Name))));
        Assert.Contains(et.GetIndexes(), i => i.Properties.Any(p => p.Name == nameof(Profile.Name)));

        var fk = et.GetForeignKeys().Single(f => f.PrincipalEntityType.ClrType == typeof(Test));
        Assert.Equal(DeleteBehavior.Cascade, fk.DeleteBehavior);
    }

    [Fact]
    public void ProfileAnalyte_HasCompositeUniqueIndex_AndCascadingForeignKeys()
    {
        var et = GetEntityType<ProfileAnalyte>();
        var pair = et.GetIndexes().Single(i =>
            i.Properties.Any(p => p.Name == nameof(ProfileAnalyte.ProfileId)) &&
            i.Properties.Any(p => p.Name == nameof(ProfileAnalyte.AnalyteId)));
        Assert.True(pair.IsUnique);
        Assert.Contains(et.GetIndexes(), i => i.Properties.Any(p => p.Name == nameof(ProfileAnalyte.AnalyteId)));

        Assert.All(
            et.GetForeignKeys()
                .Where(f => f.PrincipalEntityType.ClrType == typeof(Profile) || f.PrincipalEntityType.ClrType == typeof(Analyte)),
            fk => Assert.Equal(DeleteBehavior.Cascade, fk.DeleteBehavior));
    }

    [Fact]
    public void ProfileResultItem_HasRestrictForeignKey_ToAnalyte()
    {
        var et = GetEntityType<ProfileResultItem>();
        var fk = et.GetForeignKeys().Single(f => f.PrincipalEntityType.ClrType == typeof(Analyte));
        Assert.Equal(DeleteBehavior.Restrict, fk.DeleteBehavior);
    }

    [Fact]
    public void ProfileResultItem_HasCascadingForeignKey_ToPatientTest_AndAnalyteIndex()
    {
        var et = GetEntityType<ProfileResultItem>();
        var fk = et.GetForeignKeys().Single(f => f.PrincipalEntityType.ClrType == typeof(PatientTest));
        Assert.Equal(DeleteBehavior.Cascade, fk.DeleteBehavior);
        Assert.NotNull(et.GetIndexes().FirstOrDefault(i => i.Properties.Any(p => p.Name == nameof(ProfileResultItem.PatientTestId))));
        Assert.NotNull(et.GetIndexes().FirstOrDefault(i => i.Properties.Any(p => p.Name == nameof(ProfileResultItem.AnalyteId))));
    }

    [Fact]
    public void ProfileResultItemReferenceRangeSnapshot_IsOneToOneWithItem_AndCascades()
    {
        var et = GetEntityType<ProfileResultItemReferenceRangeSnapshot>();
        Assert.Equal(nameof(ProfileResultItemReferenceRangeSnapshot.ProfileResultItemId), et.FindPrimaryKey()!.Properties.Single().Name);

        var fk = et.GetForeignKeys().Single();
        Assert.Equal(typeof(ProfileResultItem), fk.PrincipalEntityType.ClrType);
        Assert.True(fk.IsUnique);
        Assert.Equal(DeleteBehavior.Cascade, fk.DeleteBehavior);
    }

    [Fact]
    public void ProfileResultAmendment_HasItemIndex_AndCascadingForeignKey()
    {
        var et = GetEntityType<ProfileResultAmendment>();
        Assert.NotNull(et.GetIndexes().FirstOrDefault(i => i.Properties.Any(p => p.Name == nameof(ProfileResultAmendment.ProfileResultItemId))));
        Assert.NotNull(et.FindProperty(nameof(ProfileResultAmendment.OldResultValue)));
        Assert.NotNull(et.FindProperty(nameof(ProfileResultAmendment.NewResultValue)));
        Assert.NotNull(et.FindProperty(nameof(ProfileResultAmendment.Reason)));

        var fk = et.GetForeignKeys().Single(f => f.PrincipalEntityType.ClrType == typeof(ProfileResultItem));
        Assert.Equal(DeleteBehavior.Cascade, fk.DeleteBehavior);
    }
}