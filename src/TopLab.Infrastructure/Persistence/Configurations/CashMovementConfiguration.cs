using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TopLab.Domain.Accounting;
using TopLab.Domain.Common.Ids;

namespace TopLab.Infrastructure.Persistence.Configurations;

public sealed class CashMovementConfiguration : IEntityTypeConfiguration<CashMovement>
{
    public void Configure(EntityTypeBuilder<CashMovement> b)
    {
        b.HasKey(e => e.Id);
        b.Property(e => e.Id).HasConversion(v => v.Value, v => CashMovementId.Create(v)).ValueGeneratedOnAdd().HasColumnName("CashMovementId");
        b.Property(e => e.MovementType).HasConversion<int>().HasColumnType("tinyint").IsRequired();
        b.Property(e => e.Amount).HasColumnType("decimal(18,2)").HasPrecision(18,2).IsRequired();
        b.Property(e => e.RelatedExternalEntityId).HasConversion(v => v == null ? (int?)null : v.Value, v => v == null ? null : ExternalEntityId.Create(v.Value)).IsRequired(false);
        b.Property(e => e.PerformedByUserId).IsRequired();
        b.Property(e => e.OccurredAtUtc).HasColumnType("datetime2").IsRequired();
        b.Property(e => e.Notes).HasMaxLength(500).IsRequired(false);
        b.HasOne<TopLab.Domain.ExternalEntities.ExternalEntity>().WithMany().HasForeignKey(e => e.RelatedExternalEntityId).OnDelete(DeleteBehavior.SetNull);
    }
}
