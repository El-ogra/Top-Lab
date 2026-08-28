using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TopLab.Domain.SentOutSamples;
using TopLab.Domain.Common.Ids;

namespace TopLab.Infrastructure.Persistence.Configurations;

public sealed class SentOutSamplePaymentConfiguration : IEntityTypeConfiguration<SentOutSamplePayment>
{
    public void Configure(EntityTypeBuilder<SentOutSamplePayment> b)
    {
        b.HasKey(e => e.Id);
        b.Property(e => e.Id).HasConversion(v => v.Value, v => SentOutSamplePaymentId.Create(v)).ValueGeneratedOnAdd();
        b.Property(e => e.SentOutSampleId).HasConversion(v => v.Value, v => SentOutSampleId.Create(v)).IsRequired();
        b.Property(e => e.AmountPaid).HasColumnType("decimal(18,2)").HasPrecision(18,2).IsRequired();
        b.Property(e => e.PaidAtUtc).HasColumnType("datetime2").IsRequired();
        b.Property(e => e.PerformedByUserId).IsRequired();
        b.HasOne<SentOutSample>().WithMany().HasForeignKey(e => e.SentOutSampleId).OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(e => e.SentOutSampleId);
    }
}
