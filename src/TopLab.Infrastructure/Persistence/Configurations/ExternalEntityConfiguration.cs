using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TopLab.Domain.ExternalEntities;
using TopLab.Domain.Common.Ids;

namespace TopLab.Infrastructure.Persistence.Configurations;

public sealed class ExternalEntityConfiguration : IEntityTypeConfiguration<ExternalEntity>
{
    public void Configure(EntityTypeBuilder<ExternalEntity> b)
    {
        b.HasKey(e => e.Id);
        b.Property(e => e.Id).HasConversion(v => v.Value, v => ExternalEntityId.Create(v)).ValueGeneratedOnAdd();
        b.Property(e => e.EntityType).HasConversion<int>().HasColumnType("tinyint").IsRequired();
        b.Property(e => e.Name).HasMaxLength(200).IsRequired();
        b.Property(e => e.City).HasMaxLength(100).IsRequired(false);
        b.Property(e => e.Address).HasMaxLength(300).IsRequired(false);
        b.Property(e => e.Phone).HasMaxLength(30).IsRequired(false);
        b.Property(e => e.Fax).HasMaxLength(30).IsRequired(false);
        b.Property(e => e.ResponsiblePersonName).HasMaxLength(150).IsRequired(false);
        b.Property(e => e.ResponsiblePersonPhone).HasMaxLength(30).IsRequired(false);
        b.Property(e => e.PriceListId).HasConversion(v => v == null ? (int?)null : v.Value, v => v == null ? null : PriceListId.Create(v.Value)).IsRequired(false);
        b.Property(e => e.DiscountOrCommissionPercent).HasColumnType("decimal(5,2)").HasPrecision(5,2).IsRequired(false);
        b.Property(e => e.GeneratedIdCode).HasMaxLength(50).IsRequired(false);
        b.HasOne<TopLab.Domain.Billing.PriceList>().WithMany().HasForeignKey(e => e.PriceListId).OnDelete(DeleteBehavior.SetNull);
        b.HasIndex(e => e.EntityType);
    }
}
