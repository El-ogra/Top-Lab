using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using TopLab.Domain.Billing;
using TopLab.Domain.Patients;
using TopLab.Infrastructure.Persistence;
using TopLab.Infrastructure.Tests.Common;
using Xunit;

namespace TopLab.Infrastructure.Tests.Persistence;

public class InvoiceIssueConfigurationTests
{
    private static IEntityType EntityType()
    {
        // Relational annotations (table names) need the SQL Server provider;
        // model building connects to nothing, so this stays a unit test.
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=TopLab_ModelOnly;Trusted_Connection=True;TrustServerCertificate=True")
            .Options;
        using var ctx = new ApplicationDbContext(options);
        var et = ctx.Model.FindEntityType(typeof(InvoiceIssue));
        Assert.NotNull(et);
        return et!;
    }

    [Fact]
    public void InvoiceIssue_MapsToInvoiceIssuesTable()
    {
        Assert.Equal("InvoiceIssues", EntityType().GetTableName());
    }

    [Fact]
    public void InvoiceNumber_HasUniqueIndex()
    {
        var index = Assert.Single(
            EntityType().GetIndexes(),
            i => i.Properties.Count == 1 && i.Properties[0].Name == nameof(InvoiceIssue.InvoiceNumber));

        Assert.True(index.IsUnique);
    }

    [Fact]
    public void PatientId_HasNonUniqueIndex()
    {
        var index = Assert.Single(
            EntityType().GetIndexes(),
            i => i.Properties.Count == 1 && i.Properties[0].Name == nameof(InvoiceIssue.PatientId));

        Assert.False(index.IsUnique);
    }

    [Fact]
    public void InvoiceIssue_HasNoModelForeignKeyToPatient_ApplicationLevelReference()
    {
        Assert.DoesNotContain(
            EntityType().GetForeignKeys(),
            fk => fk.PrincipalEntityType.ClrType == typeof(Patient));
    }

    [Fact]
    public void InvoiceIssue_HasNoForeignKeysAtAll()
    {
        Assert.Empty(EntityType().GetForeignKeys());
    }
}
