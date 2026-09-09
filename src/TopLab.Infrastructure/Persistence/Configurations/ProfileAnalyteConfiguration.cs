using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Tests;

namespace TopLab.Infrastructure.Persistence.Configurations;

public sealed class ProfileAnalyteConfiguration : IEntityTypeConfiguration<ProfileAnalyte>
{
    public void Configure(EntityTypeBuilder<ProfileAnalyte> b)
    {
        b.HasKey(e => e.Id);
        b.Property(e => e.Id).HasConversion(v => v.Value, v => ProfileAnalyteId.Create(v)).ValueGeneratedOnAdd().HasColumnName("ProfileAnalyteId");
        b.Property(e => e.ProfileId).HasConversion(v => v.Value, v => ProfileId.Create(v)).IsRequired();
        b.Property(e => e.AnalyteId).HasConversion(v => v.Value, v => AnalyteId.Create(v)).IsRequired();
        b.HasOne<Profile>().WithMany(p => p.Analytes).HasForeignKey(e => e.ProfileId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne<Analyte>().WithMany().HasForeignKey(e => e.AnalyteId).OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(e => new { e.ProfileId, e.AnalyteId }).IsUnique();
        b.HasIndex(e => e.AnalyteId);
    }
}