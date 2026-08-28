using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TopLab.Domain.SentOutSamples;
using TopLab.Domain.Common.Ids;

namespace TopLab.Infrastructure.Persistence.Configurations;

public sealed class SentOutSampleConfiguration : IEntityTypeConfiguration<SentOutSample>
{
    public void Configure(EntityTypeBuilder<SentOutSample> b)
    {
        b.HasKey(e => e.Id);
        b.Property(e => e.Id).HasConversion(v => v.Value, v => SentOutSampleId.Create(v)).ValueGeneratedOnAdd();
        b.Property(e => e.PatientTestId).HasConversion(v => v.Value, v => PatientTestId.Create(v)).IsRequired();
        b.Property(e => e.ExternalLabEntityId).HasConversion(v => v.Value, v => ExternalEntityId.Create(v)).IsRequired();
        b.Property(e => e.CostPrice).HasColumnType("decimal(18,2)").HasPrecision(18,2).IsRequired();
        b.Property(e => e.PatientPrice).HasColumnType("decimal(18,2)").HasPrecision(18,2).IsRequired();
        b.Property(e => e.SentAtUtc).HasColumnType("datetime2").IsRequired();
        b.HasOne<TopLab.Domain.Results.PatientTest>().WithMany().HasForeignKey(e => e.PatientTestId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne<TopLab.Domain.ExternalEntities.ExternalEntity>().WithMany().HasForeignKey(e => e.ExternalLabEntityId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(e => e.PatientTestId);
        b.HasIndex(e => e.ExternalLabEntityId);
    }
}
