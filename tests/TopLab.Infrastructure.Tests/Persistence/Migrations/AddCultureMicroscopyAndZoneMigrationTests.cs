using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using TopLab.Infrastructure.Persistence.Migrations;
using Xunit;

namespace TopLab.Infrastructure.Tests.Persistence.Migrations;

/// <summary>W-02 S9 / M2: microscopy table plus the two zone columns — and nothing commercial.</summary>
public class AddCultureMicroscopyAndZoneMigrationTests
{
    private static IReadOnlyList<MigrationOperation> Operations(bool up)
    {
        var migration = new AddCultureMicroscopyAndZone();
        var builder = new MigrationBuilder("Microsoft.EntityFrameworkCore.SqlServer");
        var method = migration.GetType().GetMethod(
            up ? "Up" : "Down",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        method!.Invoke(migration, new object[] { builder });
        return builder.Operations;
    }

    [Fact]
    public void AddCultureMicroscopyAndZone_CreatesCultureMicroscopiesWithPatientTestPrimaryKey()
    {
        var create = Assert.Single(Operations(true).OfType<CreateTableOperation>());

        Assert.Equal("CultureMicroscopies", create.Name);
        Assert.Contains(create.Columns, c => c.Name == "PatientTestId");
        Assert.Contains(create.Columns, c => c.Name == "PusCells");
        Assert.Contains(create.Columns, c => c.Name == "IsDirect");
    }

    [Fact]
    public void AddCultureMicroscopyAndZone_AddsInhibitionZoneMmDecimal41Nullable()
    {
        var add = Assert.Single(
            Operations(true).OfType<AddColumnOperation>(),
            a => a.Name == "InhibitionZoneMm");

        Assert.Equal("CultureAntibioticResults", add.Table);
        Assert.Equal("decimal(4,1)", add.ColumnType);
        Assert.True(add.IsNullable);
    }

    [Fact]
    public void AddCultureMicroscopyAndZone_AddsSensitivityThresholdMmDecimal41Nullable()
    {
        var add = Assert.Single(
            Operations(true).OfType<AddColumnOperation>(),
            a => a.Name == "SensitivityThresholdMm");

        Assert.Equal("CultureAntibioticAttachments", add.Table);
        Assert.Equal("decimal(4,1)", add.ColumnType);
        Assert.True(add.IsNullable);
    }

    [Fact]
    public void AddCultureMicroscopyAndZone_Down_DropsBothColumnsAndTable()
    {
        var down = Operations(false).ToList();

        Assert.Contains(down, o => o is DropTableOperation d && d.Name == "CultureMicroscopies");
        Assert.Contains(down, o => o is DropColumnOperation d && d.Name == "InhibitionZoneMm");
        Assert.Contains(down, o => o is DropColumnOperation d && d.Name == "SensitivityThresholdMm");
    }

    /// <summary>SD-2 (decision 3 = c): no commercial-name artefact anywhere in M2.</summary>
    [Fact]
    public void AddCultureMicroscopyAndZone_ContainsNoCommercialNameColumn()
    {
        var all = Operations(true).Concat(Operations(false)).ToList();

        Assert.DoesNotContain(all, o => o is CreateTableOperation c &&
            c.Name.Contains("Commercial", StringComparison.OrdinalIgnoreCase));
    }
}
