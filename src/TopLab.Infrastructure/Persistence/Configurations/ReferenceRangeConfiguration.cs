using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TopLab.Domain.Tests;
using TopLab.Domain.Common.Ids;

namespace TopLab.Infrastructure.Persistence.Configurations;

public sealed class ReferenceRangeConfiguration : IEntityTypeConfiguration<ReferenceRange>
{
    public void Configure(EntityTypeBuilder<ReferenceRange> b)
    {
        b.HasKey(e => e.Id);
        b.Property(e => e.Id).HasConversion(v => v.Value, v => ReferenceRangeId.Create(v)).ValueGeneratedOnAdd();
        b.Property(e => e.TestId).HasConversion(v => v.Value, v => TestId.Create(v)).IsRequired();
        b.Property(e => e.Sex).HasConversion(v => v == null ? (int?)null : (int)v.Value, v => v == null ? null : (TopLab.Domain.Common.Enums.Sex)v.Value).HasColumnType("tinyint").IsRequired(false);
        b.Property(e => e.AgeUnit).HasConversion<int>().HasColumnType("tinyint").IsRequired();
        b.Property(e => e.AgeMin).IsRequired();
        b.Property(e => e.AgeMax).IsRequired();
        b.Property(e => e.MinValue).HasColumnType("decimal(18,4)").HasPrecision(18,4).IsRequired();
        b.Property(e => e.MaxValue).HasColumnType("decimal(18,4)").HasPrecision(18,4).IsRequired();
        b.Property(e => e.LowComment).HasMaxLength(500).IsRequired(false);
        b.Property(e => e.HighComment).HasMaxLength(500).IsRequired(false);
        b.HasOne<Test>().WithMany().HasForeignKey(e => e.TestId).OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(e => e.TestId);
    }
}
