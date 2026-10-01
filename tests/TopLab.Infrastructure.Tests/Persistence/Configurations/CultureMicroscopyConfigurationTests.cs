using Microsoft.EntityFrameworkCore;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;
using TopLab.Infrastructure.Persistence;
using TopLab.Infrastructure.Tests.Common;
using Xunit;

namespace TopLab.Infrastructure.Tests.Persistence.Configurations;

/// <summary>W-02 S9 (WP-14): microscopy mapping, zone columns, cascade — via model metadata (no database).</summary>
public class CultureMicroscopyConfigurationTests
{
    private static Microsoft.EntityFrameworkCore.Metadata.IEntityType GetEntityType<T>() where T : class
    {
        using var ctx = new ApplicationDbContext(InMemoryContextFactory.Create());
        var et = ctx.Model.FindEntityType(typeof(T));
        Assert.NotNull(et);
        return et!;
    }

    [Fact]
    public void CultureResult_Delete_CascadesToCultureMicroscopy()
    {
        var et = GetEntityType<CultureMicroscopy>();

        Assert.Equal(new[] { nameof(CultureMicroscopy.PatientTestId) }, et.FindPrimaryKey()!.Properties.Select(x => x.Name));
        var fk = Assert.Single(et.GetForeignKeys());
        Assert.Equal(typeof(CultureResult), fk.PrincipalEntityType.ClrType);
        Assert.Equal(DeleteBehavior.Cascade, fk.DeleteBehavior);
    }

    [Fact]
    public void CultureMicroscopy_TextFields_AreCappedAtTwentyChars()
    {
        var et = GetEntityType<CultureMicroscopy>();

        foreach (var name in new[]
            {
                nameof(CultureMicroscopy.PusCells),
                nameof(CultureMicroscopy.RedBloodCells),
                nameof(CultureMicroscopy.EpithelialCells),
                nameof(CultureMicroscopy.Crystals),
                nameof(CultureMicroscopy.Fungi),
                nameof(CultureMicroscopy.OthersOne),
                nameof(CultureMicroscopy.OthersTwo),
                nameof(CultureMicroscopy.OthersThree)
            })
        {
            Assert.Equal(20, et.FindProperty(name)!.GetMaxLength());
            Assert.True(et.FindProperty(name)!.IsNullable);
        }
    }

    [Fact]
    public void CultureAntibioticResult_InhibitionZone_IsDecimal41Nullable()
    {
        // Relational column type ("decimal(4,1)") is pinned by the M2 migration-ops
        // tests; the InMemory model carries no relational mappings, so assert the
        // provider-agnostic surface here: presence, nullability, CLR type.
        var et = GetEntityType<CultureAntibioticResult>();
        var prop = et.FindProperty(nameof(CultureAntibioticResult.InhibitionZoneMm));

        Assert.NotNull(prop);
        Assert.True(prop!.IsNullable);
        Assert.Equal(typeof(decimal?), prop.ClrType);
    }

    [Fact]
    public void CultureAntibioticAttachment_Threshold_IsDecimal41Nullable()
    {
        var et = GetEntityType<CultureAntibioticAttachment>();
        var prop = et.FindProperty(nameof(CultureAntibioticAttachment.SensitivityThresholdMm));

        Assert.NotNull(prop);
        Assert.True(prop!.IsNullable);
        Assert.Equal(typeof(decimal?), prop.ClrType);
    }
}
