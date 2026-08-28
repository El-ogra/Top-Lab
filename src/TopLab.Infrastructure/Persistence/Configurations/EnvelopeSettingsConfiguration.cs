using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Settings;

namespace TopLab.Infrastructure.Persistence.Configurations;

public sealed class EnvelopeSettingsConfiguration : IEntityTypeConfiguration<EnvelopeSettings>
{
    public void Configure(EntityTypeBuilder<EnvelopeSettings> b)
    {
        b.HasKey(e => e.Id);
        b.Property(e => e.Id).ValueGeneratedNever();
        b.Property(e => e.TopMarginCm).HasColumnType("decimal(5,2)").HasPrecision(5,2).IsRequired();
        b.Property(e => e.HeaderFooterMode).HasConversion<int>().HasColumnType("tinyint").IsRequired();
        b.Property(e => e.SuppressCaptions).IsRequired();
        b.HasData(new { Id = 1, TopMarginCm = 3.0m, HeaderFooterMode = HeaderFooterMode.None, SuppressCaptions = false });
    }
}
