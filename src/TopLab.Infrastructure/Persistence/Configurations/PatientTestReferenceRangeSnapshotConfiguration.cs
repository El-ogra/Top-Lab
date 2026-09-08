using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TopLab.Domain.Results;

namespace TopLab.Infrastructure.Persistence.Configurations;

public sealed class PatientTestReferenceRangeSnapshotConfiguration : IEntityTypeConfiguration<PatientTestReferenceRangeSnapshot>
{
    public void Configure(EntityTypeBuilder<PatientTestReferenceRangeSnapshot> b)
    {
        b.HasKey(e => e.PatientTestId);
        b.Property(e => e.PatientTestId).HasConversion(v => v.Value, v => TopLab.Domain.Common.Ids.PatientTestId.Create(v));
        b.Property(e => e.TestId).IsRequired();
        b.Property(e => e.Sex).HasConversion(v => v == null ? (int?)null : (int)v.Value, v => v == null ? null : (TopLab.Domain.Common.Enums.Sex)v.Value).HasColumnType("tinyint").IsRequired(false);
        b.Property(e => e.AgeUnit).HasConversion<int>().HasColumnType("tinyint").IsRequired();
        b.Property(e => e.AgeMin).IsRequired();
        b.Property(e => e.AgeMax).IsRequired();
        b.Property(e => e.MinValue).HasColumnType("decimal(18,4)").HasPrecision(18, 4).IsRequired();
        b.Property(e => e.MaxValue).HasColumnType("decimal(18,4)").HasPrecision(18, 4).IsRequired();
        b.Property(e => e.LowComment).HasMaxLength(500).IsRequired(false);
        b.Property(e => e.HighComment).HasMaxLength(500).IsRequired(false);
        b.Property(e => e.CapturedAtUtc).HasColumnType("datetimeoffset").IsRequired();
        b.HasOne<PatientTest>().WithOne().HasForeignKey<PatientTestReferenceRangeSnapshot>(e => e.PatientTestId).OnDelete(DeleteBehavior.Cascade);
    }
}
