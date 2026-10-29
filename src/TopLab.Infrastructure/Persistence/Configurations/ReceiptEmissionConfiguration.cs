using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TopLab.Domain.Billing;
using TopLab.Domain.Common.Ids;

namespace TopLab.Infrastructure.Persistence.Configurations;

public sealed class ReceiptEmissionConfiguration : IEntityTypeConfiguration<ReceiptEmission>
{
    public void Configure(EntityTypeBuilder<ReceiptEmission> b)
    {
        b.HasKey(e => e.Id);
        b.Property(e => e.Id).HasConversion(v => v.Value, v => ReceiptEmissionId.Create(v)).ValueGeneratedOnAdd().HasColumnName("ReceiptEmissionId");
        b.Property(e => e.PatientId).HasConversion(v => v.Value, v => PatientId.Create(v)).IsRequired();
        b.HasIndex(e => e.PatientId);
        b.Property(e => e.IssuedAtUtc).HasColumnType("datetime2").IsRequired();
        b.Property(e => e.IssuedByUserId).IsRequired();
        b.Property(e => e.TotalCharged).HasColumnType("decimal(18,2)").IsRequired();
        b.Property(e => e.TotalPaid).HasColumnType("decimal(18,2)").IsRequired();
    }
}