using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TopLab.Domain.Tests;

namespace TopLab.Infrastructure.Persistence.Configurations;

public sealed class CustomGroupItemConfiguration : IEntityTypeConfiguration<CustomGroupItem>
{
    public void Configure(EntityTypeBuilder<CustomGroupItem> b)
    {
        b.HasKey(e => new { e.CustomGroupId, e.TestId });
        b.Property(e => e.CustomGroupId).HasConversion(v => v.Value, v => TopLab.Domain.Common.Ids.CustomGroupId.Create(v));
        b.Property(e => e.TestId).HasConversion(v => v.Value, v => TopLab.Domain.Common.Ids.TestId.Create(v));
        b.Property(e => e.Price).HasColumnType("decimal(18,2)").HasPrecision(18,2).IsRequired();
        // FK via convention (removed explicit HasOne to avoid shadow)

        // FK via convention (removed explicit HasOne to avoid shadow)
    }
}
