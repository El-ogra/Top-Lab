using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Settings;

namespace TopLab.Infrastructure.Persistence.Configurations;

public sealed class ReceiptSettingsConfiguration : IEntityTypeConfiguration<ReceiptSettings>
{
    public void Configure(EntityTypeBuilder<ReceiptSettings> b)
    {
        b.HasKey(e => e.Id);
        b.Property(e => e.Id).ValueGeneratedNever();
        b.Property(e => e.TopMarginCm).HasColumnType("decimal(5,2)").HasPrecision(5,2).IsRequired();
        b.Property(e => e.Currency).HasMaxLength(10).IsRequired();
        b.Property(e => e.PickupTimeDefault).HasColumnType("time").IsRequired(false);
        b.Property(e => e.PrintOnce).IsRequired();
        b.Property(e => e.TestDetailDisplayMode).HasConversion<int>().HasColumnType("tinyint").IsRequired();
        b.Property(e => e.CashierPrinterEnabled).IsRequired();
        b.Property(e => e.HeaderFooterMode).HasConversion<int>().HasColumnType("tinyint").IsRequired();
        b.HasData(new { Id = 1, TopMarginCm = 1.0m, Currency = "L.E.", PickupTimeDefault = (TimeOnly?)null, PrintOnce = false, TestDetailDisplayMode = TestDetailDisplayMode.Show, CashierPrinterEnabled = false, HeaderFooterMode = HeaderFooterMode.None });
    }
}
