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
        b.Property(e => e.Id).ValueGeneratedNever().HasColumnName("ReceiptSettingsId");
        b.Property(e => e.TopMarginCm).HasColumnType("decimal(5,2)").HasPrecision(5,2).IsRequired();
        b.Property(e => e.Currency).HasMaxLength(10).IsRequired();
        b.Property(e => e.PickupTimeDefault).HasColumnType("time").IsRequired(false);
        b.Property(e => e.PrintOnce).IsRequired();
        b.Property(e => e.TestDetailDisplayMode).HasConversion<int>().HasColumnType("tinyint").IsRequired();
        b.Property(e => e.CashierPrinterEnabled).IsRequired();
        b.Property(e => e.HeaderFooterMode).HasConversion<int>().HasColumnType("tinyint").IsRequired();
        b.Property(e => e.HeaderFontFamily).HasMaxLength(100).IsRequired(false);
        b.Property(e => e.HeaderFontSizePt).HasColumnType("decimal(4,1)").HasPrecision(4,1).IsRequired(false);
        b.Property(e => e.HeaderFontColor).HasMaxLength(9).IsRequired(false);
        b.Property(e => e.FooterFontFamily).HasMaxLength(100).IsRequired(false);
        b.Property(e => e.FooterFontSizePt).HasColumnType("decimal(4,1)").HasPrecision(4,1).IsRequired(false);
        b.Property(e => e.FooterFontColor).HasMaxLength(9).IsRequired(false);
        b.Property(e => e.LogoImagePath).HasMaxLength(300).IsRequired(false);
        b.HasData(new { Id = 1, TopMarginCm = 1.0m, Currency = "L.E.", PickupTimeDefault = (TimeOnly?)null, PrintOnce = false, TestDetailDisplayMode = TestDetailDisplayMode.Show, CashierPrinterEnabled = false, HeaderFooterMode = HeaderFooterMode.None, HeaderFontFamily = (string?)null, HeaderFontSizePt = (decimal?)null, HeaderFontColor = (string?)null, FooterFontFamily = (string?)null, FooterFontSizePt = (decimal?)null, FooterFontColor = (string?)null, LogoImagePath = (string?)null });
    }
}
