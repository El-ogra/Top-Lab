using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using TopLab.Domain.Billing;
using TopLab.Domain.Tests;
using TopLab.Infrastructure.Persistence;
using TopLab.Infrastructure.Tests.Common;
using Xunit;

namespace TopLab.Infrastructure.Tests.Persistence;

public class PriceListCustomGroupDeleteBehaviorTests
{
    private static IForeignKey? FkToPrincipal<TDependent>(System.Type principal) where TDependent : class
    {
        using var ctx = new ApplicationDbContext(InMemoryContextFactory.Create());
        var dependent = ctx.Model.FindEntityType(typeof(TDependent));
        return dependent?.GetForeignKeys()
            .Where(fk => fk.PrincipalEntityType.ClrType == principal)
            .FirstOrDefault();
    }

    private static IForeignKey? FkFromPrincipal<TPrincipal>(System.Type dependent) where TPrincipal : class
    {
        using var ctx = new ApplicationDbContext(InMemoryContextFactory.Create());
        var principal = ctx.Model.FindEntityType(typeof(TPrincipal));
        return principal?.GetReferencingForeignKeys()
            .Where(fk => fk.DeclaringEntityType.ClrType == dependent)
            .FirstOrDefault();
    }

    private static IEntityType GetEntityType<T>() where T : class
    {
        using var ctx = new ApplicationDbContext(InMemoryContextFactory.Create());
        var et = ctx.Model.FindEntityType(typeof(T));
        Assert.NotNull(et);
        return et!;
    }

    [Fact]
    public void DeletingPriceList_CascadesToPriceListItems()
    {
        var fk = FkFromPrincipal<PriceList>(typeof(PriceListItem));
        Assert.NotNull(fk);
        Assert.Equal(DeleteBehavior.Cascade, fk!.DeleteBehavior);

        var navigation = fk.DependentToPrincipal;
        if (navigation is not null)
        {
            Assert.Equal("Items", navigation.Name);
        }
    }

    [Fact]
    public void DeletingCustomGroup_CascadesToCustomGroupItems()
    {
        var fk = FkFromPrincipal<CustomGroup>(typeof(CustomGroupItem));
        Assert.NotNull(fk);
        Assert.Equal(DeleteBehavior.Cascade, fk!.DeleteBehavior);

        var navigation = fk.DependentToPrincipal;
        if (navigation is not null)
        {
            Assert.Equal("Items", navigation.Name);
        }
    }

    [Fact]
    public void DeletingTest_CascadesToTestComments()
    {
        var fk = FkToPrincipal<TestComment>(typeof(Test));
        Assert.NotNull(fk);
        Assert.Equal(DeleteBehavior.Cascade, fk!.DeleteBehavior);

        var testIdIdx = GetEntityType<TestComment>().GetIndexes()
            .FirstOrDefault(i => i.Properties.Any(p => p.Name == nameof(TestComment.TestId)));
        Assert.NotNull(testIdIdx);
    }

    [Fact]
    public void DeletingPriceList_NullsExternalEntityReference()
    {
        var fk = FkToPrincipal<TopLab.Domain.ExternalEntities.ExternalEntity>(typeof(PriceList));
        Assert.NotNull(fk);
        Assert.Equal("PriceListId", Assert.Single(fk!.Properties).Name);
        Assert.Equal(DeleteBehavior.SetNull, fk.DeleteBehavior);
    }

    // ---------- Negative (no-FK) assertions ----------

    [Fact]
    public void PriceListItem_HasNoRelationshipToTest()
    {
        var dependent = GetEntityType<PriceListItem>();
        Assert.DoesNotContain(
            dependent.GetForeignKeys(),
            fk => fk.PrincipalEntityType.ClrType == typeof(Test));
    }

    [Fact]
    public void CustomGroupItem_HasNoRelationshipToTest()
    {
        var dependent = GetEntityType<CustomGroupItem>();
        Assert.DoesNotContain(
            dependent.GetForeignKeys(),
            fk => fk.PrincipalEntityType.ClrType == typeof(Test));
    }
}
