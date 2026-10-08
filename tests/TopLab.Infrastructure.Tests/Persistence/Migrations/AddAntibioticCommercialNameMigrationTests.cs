using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using TopLab.Infrastructure.Persistence.Migrations;
using Xunit;

namespace TopLab.Infrastructure.Tests.Persistence.Migrations;

/// <summary>Phase 1 REF-092: the commercial name ships as one nullable column — nothing more.</summary>
public class AddAntibioticCommercialNameMigrationTests
{
    private static IReadOnlyList<MigrationOperation> Operations(bool up)
    {
        var migration = new AddAntibioticCommercialName();
        var builder = new MigrationBuilder("Microsoft.EntityFrameworkCore.SqlServer");
        var method = migration.GetType().GetMethod(
            up ? "Up" : "Down",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        method!.Invoke(migration, new object[] { builder });
        return builder.Operations;
    }

    [Fact]
    public void AddAntibioticCommercialName_AddsCommercialNameNvarchar150Nullable()
    {
        var add = Assert.Single(
            Operations(true).OfType<AddColumnOperation>(),
            a => a.Name == "CommercialName");

        Assert.Equal("Antibiotics", add.Table);
        Assert.Equal("nvarchar(150)", add.ColumnType);
        Assert.True(add.IsNullable);
    }

    [Fact]
    public void AddAntibioticCommercialName_AddsExactlyOneColumn()
    {
        Assert.Single(Operations(true).OfType<AddColumnOperation>());
    }

    [Fact]
    public void AddAntibioticCommercialName_Up_ContainsNoOtherOperation()
    {
        var ops = Operations(true).ToList();

        Assert.Single(ops);
        Assert.DoesNotContain(ops, o => o is CreateTableOperation);
    }

    [Fact]
    public void AddAntibioticCommercialName_Down_DropsExactlyThatColumn()
    {
        var drops = Operations(false).OfType<DropColumnOperation>().ToList();

        var drop = Assert.Single(drops);
        Assert.Equal("CommercialName", drop.Name);
        Assert.Equal("Antibiotics", drop.Table);
        Assert.Single(Operations(false));
    }
}
