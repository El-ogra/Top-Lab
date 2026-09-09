using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Tests;

namespace TopLab.Infrastructure.Persistence.Configurations;

public sealed class AnalyteConfiguration : IEntityTypeConfiguration<Analyte>
{
    public void Configure(EntityTypeBuilder<Analyte> b)
    {
        b.HasKey(e => e.Id);
        b.Property(e => e.Id).HasConversion(v => v.Value, v => AnalyteId.Create(v)).ValueGeneratedOnAdd().HasColumnName("AnalyteId");
        b.Property(e => e.Name).HasMaxLength(150).IsRequired();
        b.Property(e => e.ReportName).HasMaxLength(150).IsRequired();
        b.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);
        b.HasOne(e => e.CurrentRange)
            .WithOne()
            .HasForeignKey<AnalyteReferenceRange>(r => r.AnalyteId)
            .OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(e => e.Name).IsUnique();
    }
}