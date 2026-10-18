using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using TopLab.Domain.Settings;
using TopLab.Infrastructure.Persistence;
using TopLab.Infrastructure.Tests.Common;
using Xunit;

namespace TopLab.Infrastructure.Tests.Persistence;

/// <summary>
/// GAP-015 (Phase 8): the per-item report layout columns on
/// <see cref="ReportItemTemplate"/> — nullable decimals with explicit column
/// type/precision, and a nullable marker string capped at 10 characters.
/// </summary>
public class ReportItemTemplateLayoutConfigurationTests
{
    private static IEntityType SqlEntityType()
    {
        // Relational annotations (column type) need the SQL Server provider;
        // model building connects to nothing, so this stays a unit test.
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=TopLab_ModelOnly;Trusted_Connection=True;TrustServerCertificate=True")
            .Options;
        using var ctx = new ApplicationDbContext(options);
        var et = ctx.Model.FindEntityType(typeof(ReportItemTemplate));
        Assert.NotNull(et);
        return et!;
    }

    [Fact]
    public void LineSpacing_IsNullableDecimal_4_2()
    {
        var property = SqlEntityType().FindProperty(nameof(ReportItemTemplate.LineSpacing));
        Assert.NotNull(property);
        Assert.Equal(typeof(decimal?), property!.ClrType);
        Assert.True(property.IsNullable);
        Assert.Equal("decimal(4,2)", property.GetColumnType());
        Assert.Equal(4, property.GetPrecision());
        Assert.Equal(2, property.GetScale());
    }

    [Fact]
    public void LeftOffsetCm_IsNullableDecimal_5_2()
    {
        var property = SqlEntityType().FindProperty(nameof(ReportItemTemplate.LeftOffsetCm));
        Assert.NotNull(property);
        Assert.Equal(typeof(decimal?), property!.ClrType);
        Assert.True(property.IsNullable);
        Assert.Equal("decimal(5,2)", property.GetColumnType());
        Assert.Equal(5, property.GetPrecision());
        Assert.Equal(2, property.GetScale());
    }

    [Fact]
    public void MarkerSymbol_IsNullableString_MaxTen()
    {
        var property = SqlEntityType().FindProperty(nameof(ReportItemTemplate.MarkerSymbol));
        Assert.NotNull(property);
        Assert.Equal(typeof(string), property!.ClrType);
        Assert.True(property.IsNullable);
        Assert.Equal(ReportItemTemplate.MaxMarkerSymbolLength, property.GetMaxLength());
        Assert.Equal(10, ReportItemTemplate.MaxMarkerSymbolLength);
    }

    [Fact]
    public void SeededRows_KeepNullLayout_UntilConfigured()
    {
        using var ctx = new ApplicationDbContext(InMemoryContextFactory.Create());
        ctx.Database.EnsureCreated();

        var rows = ctx.Set<ReportItemTemplate>().ToList();
        Assert.Equal(24, rows.Count);
        Assert.All(rows, row =>
        {
            Assert.Null(row.LineSpacing);
            Assert.Null(row.LeftOffsetCm);
            Assert.Null(row.MarkerSymbol);
        });
    }

    [Fact]
    public async Task LayoutValues_PersistAndReadBack()
    {
        using var ctx = new ApplicationDbContext(InMemoryContextFactory.Create());
        ctx.Database.EnsureCreated();

        var row = await ctx.Set<ReportItemTemplate>().FindAsync(ReportItemKeys.TestName);
        Assert.NotNull(row);
        row!.Update("التحليل", null, true, null, false, 1.75m, 2.25m, "•");
        await ctx.SaveChangesAsync();

        ctx.ChangeTracker.Clear();
        var reloaded = await ctx.Set<ReportItemTemplate>().FindAsync(ReportItemKeys.TestName);
        Assert.NotNull(reloaded);
        Assert.Equal(1.75m, reloaded!.LineSpacing);
        Assert.Equal(2.25m, reloaded.LeftOffsetCm);
        Assert.Equal("•", reloaded.MarkerSymbol);
    }

    [Fact]
    public async Task NullLayoutValues_PersistAndReadBackAsNull()
    {
        using var ctx = new ApplicationDbContext(InMemoryContextFactory.Create());
        ctx.Database.EnsureCreated();

        var row = await ctx.Set<ReportItemTemplate>().FindAsync(ReportItemKeys.ResultValue);
        Assert.NotNull(row);
        row!.Update("النتيجة", null, true, null, false, 2m, 1m, "*");
        await ctx.SaveChangesAsync();

        row.Update("النتيجة", null, true, null, false, null, null, null);
        await ctx.SaveChangesAsync();

        ctx.ChangeTracker.Clear();
        var reloaded = await ctx.Set<ReportItemTemplate>().FindAsync(ReportItemKeys.ResultValue);
        Assert.NotNull(reloaded);
        Assert.Null(reloaded!.LineSpacing);
        Assert.Null(reloaded.LeftOffsetCm);
        Assert.Null(reloaded.MarkerSymbol);
    }
}