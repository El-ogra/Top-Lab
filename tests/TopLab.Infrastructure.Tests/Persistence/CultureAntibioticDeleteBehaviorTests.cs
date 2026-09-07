using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;
using TopLab.Infrastructure.Persistence;
using TopLab.Infrastructure.Tests.Common;
using Xunit;

namespace TopLab.Infrastructure.Tests.Persistence;

public class CultureAntibioticDeleteBehaviorTests
{
    private static IEntityType EntityTypeOf<T>() where T : class
    {
        using var ctx = new ApplicationDbContext(InMemoryContextFactory.Create());
        var et = ctx.Model.FindEntityType(typeof(T));
        Assert.NotNull(et);
        return et!;
    }

    private static IForeignKey? FkToPrincipal<TDependent>(System.Type principal) where TDependent : class
    {
        return EntityTypeOf<TDependent>().GetForeignKeys()
            .Where(fk => fk.PrincipalEntityType.ClrType == principal)
            .FirstOrDefault();
    }

    // ---------- Positive FK matrix ----------

    [Fact]
    public void DeletingAntibioticIsRestrictedForCultureAntibioticResults()
    {
        var fk = FkToPrincipal<CultureAntibioticResult>(typeof(Antibiotic));

        Assert.NotNull(fk);
        Assert.Equal("AntibioticId", Assert.Single(fk!.Properties).Name);
        Assert.Equal(DeleteBehavior.Restrict, fk.DeleteBehavior);
    }

    [Fact]
    public void DeletingCultureResultCascadesToCultureAntibioticResults()
    {
        var fk = FkToPrincipal<CultureAntibioticResult>(typeof(CultureResult));

        Assert.NotNull(fk);
        Assert.Equal("PatientTestId", Assert.Single(fk!.Properties).Name);
        Assert.Equal(DeleteBehavior.Cascade, fk.DeleteBehavior);
    }

    [Fact]
    public void DeletingPatientTestCascadesToCultureResult()
    {
        var fk = FkToPrincipal<CultureResult>(typeof(PatientTest));

        Assert.NotNull(fk);
        Assert.Equal("PatientTestId", Assert.Single(fk!.Properties).Name);
        Assert.Equal(DeleteBehavior.Cascade, fk.DeleteBehavior);
    }

    // ---------- Negative (no-FK) assertions ----------

    [Fact]
    public void CultureAntibioticAttachment_HasNoRelationshipToTest()
    {
        var dependent = EntityTypeOf<CultureAntibioticAttachment>();

        Assert.DoesNotContain(
            dependent.GetForeignKeys(),
            fk => fk.PrincipalEntityType.ClrType == typeof(Test));
    }

    [Fact]
    public void CultureAntibioticAttachment_HasNoRelationshipToAntibiotic()
    {
        var dependent = EntityTypeOf<CultureAntibioticAttachment>();

        Assert.DoesNotContain(
            dependent.GetForeignKeys(),
            fk => fk.PrincipalEntityType.ClrType == typeof(Antibiotic));
    }
}