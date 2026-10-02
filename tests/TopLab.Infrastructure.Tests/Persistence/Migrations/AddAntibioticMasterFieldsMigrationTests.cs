using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using TopLab.Infrastructure.Persistence.Migrations;
using Xunit;

namespace TopLab.Infrastructure.Tests.Persistence.Migrations;

/// <summary>W-02 S10 / M3: exactly two nullable columns — and nothing commercial.</summary>
public class AddAntibioticMasterFieldsMigrationTests
{
    private static IReadOnlyList<MigrationOperation> Operations(bool up)
    {
        var migration = new AddAntibioticMasterFields();
        var builder = new MigrationBuilder("Microsoft.EntityFrameworkCore.SqlServer");
        var method = migration.GetType().GetMethod(
            up ? "Up" : "Down",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        method!.Invoke(migration, new object[] { builder });
        return builder.Operations;
    }

    [Fact]
    public void AddAntibioticMasterFields_AddsSymbolNvarchar10Nullable()
    {
        var add = Assert.Single(
            Operations(true).OfType<AddColumnOperation>(),
            a => a.Name == "Symbol");

        Assert.Equal("Antibiotics", add.Table);
        Assert.Equal("nvarchar(10)", add.ColumnType);
        Assert.True(add.IsNullable);
    }

    [Fact]
    public void AddAntibioticMasterFields_AddsScientificNameNvarchar150Nullable()
    {
        var add = Assert.Single(
            Operations(true).OfType<AddColumnOperation>(),
            a => a.Name == "ScientificName");

        Assert.Equal("Antibiotics", add.Table);
        Assert.Equal("nvarchar(150)", add.ColumnType);
        Assert.True(add.IsNullable);
    }

    [Fact]
    public void AddAntibioticMasterFields_AddsExactlyTwoColumns()
    {
        Assert.Equal(2, Operations(true).OfType<AddColumnOperation>().Count());
    }

    /// <summary>SD-2 (decision 3 = c): the commercial-name table must never appear.</summary>
    [Fact]
    public void AddAntibioticMasterFields_ContainsNoCommercialNameColumn()
    {
        var all = Operations(true).Concat(Operations(false)).ToList();

        Assert.DoesNotContain(all, o => o is CreateTableOperation c &&
            c.Name.Contains("Commercial", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(all, o => o is AddColumnOperation a &&
            a.Name.Contains("Commercial", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void AddAntibioticMasterFields_Down_DropsBothColumns()
    {
        var drops = Operations(false).OfType<DropColumnOperation>().ToList();

        Assert.Equal(2, drops.Count);
        Assert.Contains(drops, d => d.Name == "Symbol");
        Assert.Contains(drops, d => d.Name == "ScientificName");
    }
}
