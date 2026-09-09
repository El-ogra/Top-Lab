using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Results;

namespace TopLab.Infrastructure.Persistence.Configurations;

public sealed class ProfileResultItemReferenceRangeSnapshotConfiguration : IEntityTypeConfiguration<ProfileResultItemReferenceRangeSnapshot>
{
    public void Configure(EntityTypeBuilder<ProfileResultItemReferenceRangeSnapshot> b)
    {
        b.HasKey(e => e.ProfileResultItemId);
        b.Property(e => e.ProfileResultItemId).HasConversion(v => v.Value, v => ProfileResultItemId.Create(v));
        b.Property(e => e.AnalyteId).HasConversion(v => v.Value, v => AnalyteId.Create(v)).IsRequired();
        b.Property(e => e.Sex).HasConversion(v => v == null ? (int?)null : (int)v.Value, v => v == null ? null : (Sex)v.Value).HasColumnType("tinyint").IsRequired(false);
        b.Property(e => e.AgeUnit).HasConversion<int>().HasColumnType("tinyint").IsRequired();
        b.Property(e => e.AgeMin).IsRequired();
        b.Property(e => e.AgeMax).IsRequired();
        b.Property(e => e.MinValue).HasColumnType("decimal(18,4)").HasPrecision(18, 4).IsRequired();
        b.Property(e => e.MaxValue).HasColumnType("decimal(18,4)").HasPrecision(18, 4).IsRequired();
        b.Property(e => e.LowComment).HasMaxLength(500).IsRequired(false);
        b.Property(e => e.HighComment).HasMaxLength(500).IsRequired(false);
        b.Property(e => e.CapturedAtUtc).HasColumnType("datetimeoffset").IsRequired();
        b.HasOne<ProfileResultItem>().WithOne().HasForeignKey<ProfileResultItemReferenceRangeSnapshot>(e => e.ProfileResultItemId).OnDelete(DeleteBehavior.Cascade);
    }
}