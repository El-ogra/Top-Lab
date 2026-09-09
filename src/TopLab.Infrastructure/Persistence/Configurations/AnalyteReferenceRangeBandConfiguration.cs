using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Tests;

namespace TopLab.Infrastructure.Persistence.Configurations;

public sealed class AnalyteReferenceRangeBandConfiguration : IEntityTypeConfiguration<AnalyteReferenceRangeBand>
{
    public void Configure(EntityTypeBuilder<AnalyteReferenceRangeBand> b)
    {
        b.HasKey(e => e.Id);
        b.Property(e => e.Id).HasConversion(v => v.Value, v => AnalyteReferenceRangeBandId.Create(v)).ValueGeneratedOnAdd().HasColumnName("AnalyteReferenceRangeBandId");
        b.Property(e => e.AnalyteReferenceRangeId).HasConversion(v => v.Value, v => AnalyteReferenceRangeId.Create(v)).IsRequired();
        b.Property(e => e.Sex).HasConversion(v => v == null ? (int?)null : (int)v.Value, v => v == null ? null : (Sex)v.Value).HasColumnType("tinyint").IsRequired(false);
        b.Property(e => e.AgeUnit).HasConversion<int>().HasColumnType("tinyint").IsRequired();
        b.Property(e => e.AgeMin).IsRequired();
        b.Property(e => e.AgeMax).IsRequired();
        b.Property(e => e.MinValue).HasColumnType("decimal(18,4)").HasPrecision(18, 4).IsRequired();
        b.Property(e => e.MaxValue).HasColumnType("decimal(18,4)").HasPrecision(18, 4).IsRequired();
        b.Property(e => e.LowComment).HasMaxLength(500).IsRequired(false);
        b.Property(e => e.HighComment).HasMaxLength(500).IsRequired(false);
        b.HasOne<AnalyteReferenceRange>().WithMany(r => r.Bands).HasForeignKey(e => e.AnalyteReferenceRangeId).OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(e => e.AnalyteReferenceRangeId);
    }
}