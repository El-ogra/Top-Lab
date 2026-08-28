using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TopLab.Domain.Billing;
using TopLab.Domain.Common.Ids;

namespace TopLab.Infrastructure.Persistence.Configurations;

public sealed class PaymentOperationConfiguration : IEntityTypeConfiguration<PaymentOperation>
{
    public void Configure(EntityTypeBuilder<PaymentOperation> b)
    {
        b.HasKey(e => e.Id);
        b.Property(e => e.Id).HasConversion(v => v.Value, v => PaymentOperationId.Create(v)).ValueGeneratedOnAdd();
        b.Property(e => e.PatientId).HasConversion(v => v.Value, v => PatientId.Create(v)).IsRequired();
        b.Property(e => e.Amount).HasColumnType("decimal(18,2)").HasPrecision(18,2).IsRequired();
        b.Property(e => e.DiscountAmount).HasColumnType("decimal(18,2)").HasPrecision(18,2).IsRequired(false);
        b.Property(e => e.IsExtraCharge).IsRequired();
        b.Property(e => e.OperationType).HasConversion<int>().HasColumnType("tinyint").IsRequired();
        b.Property(e => e.ReceivedByUserId).IsRequired();
        b.Property(e => e.OperationAtUtc).HasColumnType("datetime2").IsRequired();
        b.Property(e => e.IsVoided).IsRequired();
        b.HasOne<TopLab.Domain.Patients.Patient>().WithMany().HasForeignKey(e => e.PatientId).OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(e => e.PatientId);
    }
}
