using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using TopLab.Domain.Settings;
using TopLab.Infrastructure.Persistence.Migrations;
using Xunit;

namespace TopLab.Infrastructure.Tests.Persistence.Migrations;

/// <summary>
/// W-02 S7 / M1: reflection over the generated <c>Up</c>/<c>Down</c> operations. The migration is
/// never applied to a database here — the shape is what matters (SD-11: three new migrations only).
/// </summary>
public class AddCombinedReportPrintOptionsMigrationTests
{
    private static IReadOnlyList<MigrationOperation> Operations(bool up)
    {
        var migration = new AddCombinedReportPrintOptions();
        var builder = new MigrationBuilder("Microsoft.EntityFrameworkCore.SqlServer");
        var operations = up
            ? Invoke(migration, "Up", builder)
            : Invoke(migration, "Down", builder);
        return operations;
    }

    private static IReadOnlyList<MigrationOperation> Invoke(
        AddCombinedReportPrintOptions migration, string name, MigrationBuilder builder)
    {
        var method = migration.GetType().GetMethod(
            name,
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        method!.Invoke(migration, new object[] { builder });
        return builder.Operations;
    }

    [Fact]
    public void AddCombinedReportPrintOptions_AddsTwoBitNotNullColumnsWithFalseDefault()
    {
        var adds = Operations(true).OfType<AddColumnOperation>().ToList();

        Assert.Equal(2, adds.Count);
        Assert.All(adds, a =>
        {
            Assert.Equal("ReportSettings", a.Table);
            Assert.Equal("bit", a.ColumnType);
            Assert.False(a.IsNullable);
            Assert.Equal(false, a.DefaultValue);
        });
        Assert.Contains(adds, a => a.Name == "PrintGroupSubTitle");
        Assert.Contains(adds, a => a.Name == "SuppressReprintMessage");
    }

    [Fact]
    public void AddCombinedReportPrintOptions_EmitsUpdateDataForSeededSettingsRow()
    {
        var seed = Assert.Single(Operations(true).OfType<UpdateDataOperation>());

        Assert.Equal("ReportSettings", seed.Table);
        Assert.Equal("ReportSettingsId", Assert.Single(seed.KeyColumns));
        // EF 8 exposes KeyValues as a 2-D array indexed [row, column].
        var keyValues = seed.KeyValues;
        Assert.Equal(1, Convert.ToInt32(keyValues[0, 0]));
    }

    [Fact]
    public void AddCombinedReportPrintOptions_Down_DropsBothColumns()
    {
        var drops = Operations(false).OfType<DropColumnOperation>().ToList();

        Assert.Equal(2, drops.Count);
        Assert.All(drops, d => Assert.Equal("ReportSettings", d.Table));
        Assert.Contains(drops, d => d.Name == "PrintGroupSubTitle");
        Assert.Contains(drops, d => d.Name == "SuppressReprintMessage");
    }

    /// <summary>SD-1 guard: this migration must not touch the print-tracking columns' tables.</summary>
    [Fact]
    public void AddCombinedReportPrintOptions_DoesNotTouchPatientTestsOrProfileResultItems()
    {
        var all = Operations(true).Concat(Operations(false)).ToList();

        Assert.DoesNotContain(all, o => o is DropColumnOperation d && d.Table is "PatientTests" or "ProfileResultItems");
    }

    [Fact]
    public void ReportSettings_DefaultFlagsAreBothFalse()
    {
        var settings = ReportSettings.CreateDefault();

        Assert.False(settings.PrintGroupSubTitle);
        Assert.False(settings.SuppressReprintMessage);
    }

    [Fact]
    public void ReportSettings_SetPrintOptions_AppliesBoth()
    {
        var settings = ReportSettings.CreateDefault();

        settings.SetPrintOptions(true, true);
        Assert.True(settings.PrintGroupSubTitle);
        Assert.True(settings.SuppressReprintMessage);

        settings.SetPrintOptions(false, false);
        Assert.False(settings.PrintGroupSubTitle);
        Assert.False(settings.SuppressReprintMessage);
    }
}
