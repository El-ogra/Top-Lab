using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Tests;

namespace TopLab.Infrastructure.Persistence.Configurations;

public sealed class AnalyteReferenceRangeConfiguration : IEntityTypeConfiguration<AnalyteReferenceRange>
{
    public void Configure(EntityTypeBuilder<AnalyteReferenceRange> b)
    {
        b.HasKey(e => e.Id);
        b.Property(e => e.Id).HasConversion(v => v.Value, v => AnalyteReferenceRangeId.Create(v)).ValueGeneratedOnAdd().HasColumnName("AnalyteReferenceRangeId");
        b.Property(e => e.AnalyteId).HasConversion(v => v.Value, v => AnalyteId.Create(v)).IsRequired();
        b.HasIndex(e => e.AnalyteId).IsUnique();
    }
}