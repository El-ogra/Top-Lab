using Microsoft.EntityFrameworkCore;
using TopLab.Domain.Accounting;
using TopLab.Domain.Billing;
using TopLab.Domain.ExternalEntities;
using TopLab.Domain.Patients;
using TopLab.Domain.SentOutSamples;
using TopLab.Infrastructure.Persistence;
using TopLab.Infrastructure.Tests.Common;
using Xunit;

namespace TopLab.Infrastructure.Tests.Persistence;

public class ExternalEntityDeleteBehaviorTests
{
    private static Microsoft.EntityFrameworkCore.Metadata.IEntityType EntityTypeOf<T>() where T : class
    {
        using var ctx = new ApplicationDbContext(InMemoryContextFactory.Create());
        var et = ctx.Model.FindEntityType(typeof(T));
        Assert.NotNull(et);
        return et!;
    }

    private static Microsoft.EntityFrameworkCore.Metadata.IForeignKey? ForeignKeyTo<TDependent>(System.Type principal)
        where TDependent : class
    {
        return EntityTypeOf<TDependent>().GetForeignKeys()
            .FirstOrDefault(fk => fk.PrincipalEntityType.ClrType == principal);
    }

    [Fact]
    public void Patient_HasNoModelForeignKeyToExternalEntity_HandlerGuardIsSoleProtection()
    {
        var patient = EntityTypeOf<Patient>();

        Assert.DoesNotContain(patient.GetForeignKeys(),
            fk => fk.PrincipalEntityType.ClrType == typeof(ExternalEntity));
    }

    [Fact]
    public void DeletingExternalEntity_IsRestrictedForSentOutSamples()
    {
        var fk = ForeignKeyTo<SentOutSample>(typeof(ExternalEntity));

        Assert.NotNull(fk);
        Assert.Equal("ExternalLabEntityId", Assert.Single(fk!.Properties).Name);
        Assert.Equal(DeleteBehavior.Restrict, fk.DeleteBehavior);
    }

    [Fact]
    public void DeletingExternalEntity_NullsCashMovementReference()
    {
        var fk = ForeignKeyTo<CashMovement>(typeof(ExternalEntity));

        Assert.NotNull(fk);
        Assert.Equal("RelatedExternalEntityId", Assert.Single(fk!.Properties).Name);
        Assert.Equal(DeleteBehavior.SetNull, fk.DeleteBehavior);
    }

    [Fact]
    public void DeletingPriceList_NullsExternalEntityReference()
    {
        var fk = ForeignKeyTo<ExternalEntity>(typeof(PriceList));

        Assert.NotNull(fk);
        Assert.Equal("PriceListId", Assert.Single(fk!.Properties).Name);
        Assert.Equal(DeleteBehavior.SetNull, fk.DeleteBehavior);
    }
}
