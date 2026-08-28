using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TopLab.Domain.Billing;

namespace TopLab.Infrastructure.Persistence.Configurations;

public sealed class PriceListItemConfiguration : IEntityTypeConfiguration<PriceListItem>
{
    public void Configure(EntityTypeBuilder<PriceListItem> b)
    {
        b.HasKey(e => new { e.PriceListId, e.TestId });
        b.Property(e => e.PriceListId).HasConversion(v => v.Value, v => TopLab.Domain.Common.Ids.PriceListId.Create(v));
        b.Property(e => e.TestId).HasConversion(v => v.Value, v => TopLab.Domain.Common.Ids.TestId.Create(v));
        b.Property(e => e.Price).HasColumnType("decimal(18,2)").HasPrecision(18,2).IsRequired();
        // removed
        // removed
    }
}
