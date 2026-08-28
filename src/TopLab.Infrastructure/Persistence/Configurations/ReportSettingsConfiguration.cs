using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Settings;

namespace TopLab.Infrastructure.Persistence.Configurations;

public sealed class ReportSettingsConfiguration : IEntityTypeConfiguration<ReportSettings>
{
    public void Configure(EntityTypeBuilder<ReportSettings> b)
    {
        b.HasKey(e => e.Id);
        b.Property(e => e.Id).ValueGeneratedNever();
        b.Property(e => e.PageMarginLeftCm).HasColumnType("decimal(5,2)").HasPrecision(5,2).IsRequired();
        b.Property(e => e.PageMarginBottomCm).HasColumnType("decimal(5,2)").HasPrecision(5,2).IsRequired();
        b.Property(e => e.ReportTopSpaceCm).HasColumnType("decimal(5,2)").HasPrecision(5,2).IsRequired();
        b.Property(e => e.PaperSize).HasConversion<int>().HasColumnType("tinyint").IsRequired();
        b.Property(e => e.HeaderFooterMode).HasConversion<int>().HasColumnType("tinyint").IsRequired();
        b.Property(e => e.DoctorSignatureEnabled).IsRequired();
        b.Property(e => e.HeaderColor).HasMaxLength(9).IsRequired(false);
        b.Property(e => e.FooterColor).HasMaxLength(9).IsRequired(false);
        b.Property(e => e.HistorySortMode).HasConversion<int>().HasColumnType("tinyint").IsRequired();
        b.Property(e => e.HistoryAutoDisplayEnabled).IsRequired();
        b.ToTable(t => t.HasCheckConstraint("CK_ReportSettings_TopSpace", "[ReportTopSpaceCm] <= 8"));
        b.HasData(new { Id = 1, PageMarginLeftCm = 1.0m, PageMarginBottomCm = 1.0m, ReportTopSpaceCm = 2.0m, PaperSize = PaperSize.A4, HeaderFooterMode = HeaderFooterMode.None, DoctorSignatureEnabled = false, HeaderColor = (string?)null, FooterColor = (string?)null, HistorySortMode = HistorySortMode.ByLabCode, HistoryAutoDisplayEnabled = true });
    }
}
