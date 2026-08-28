using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TopLab.Domain.Settings;

namespace TopLab.Infrastructure.Persistence.Configurations;

public sealed class EnvelopePrintItemPositionConfiguration : IEntityTypeConfiguration<EnvelopePrintItemPosition>
{
    public void Configure(EntityTypeBuilder<EnvelopePrintItemPosition> b)
    {
        b.HasKey(e => e.ItemName);
        b.Property(e => e.ItemName).HasMaxLength(50).IsRequired();
        b.Property(e => e.IsEnabled).IsRequired();
        b.Property(e => e.LeftOffsetCm).HasColumnType("decimal(5,2)").HasPrecision(5,2).IsRequired();
        b.Property(e => e.TopOffsetCm).HasColumnType("decimal(5,2)").HasPrecision(5,2).IsRequired();
        b.HasData(
            new EnvelopePrintItemPosition("Name", true, 1.0m, 1.0m),
            new EnvelopePrintItemPosition("Code", true, 1.0m, 2.0m),
            new EnvelopePrintItemPosition("ReferralEntity", true, 1.0m, 3.0m),
            new EnvelopePrintItemPosition("Date", true, 1.0m, 4.0m)
        );
    }
}
