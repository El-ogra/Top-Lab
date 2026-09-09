using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TopLab.Domain.Results;
using TopLab.Domain.Common.Ids;

namespace TopLab.Infrastructure.Persistence.Configurations;

public sealed class ProfileResultItemConfiguration : IEntityTypeConfiguration<ProfileResultItem>
{
    public void Configure(EntityTypeBuilder<ProfileResultItem> b)
    {
        b.HasKey(e => e.Id);
        b.Property(e => e.Id).HasConversion(v => v.Value, v => ProfileResultItemId.Create(v)).ValueGeneratedOnAdd().HasColumnName("ProfileResultItemId");
        b.Property(e => e.PatientTestId).HasConversion(v => v.Value, v => PatientTestId.Create(v)).IsRequired();
        b.Property(e => e.AnalyteId).HasConversion(v => v.Value, v => AnalyteId.Create(v)).IsRequired();
        b.Property(e => e.ResultValue).HasMaxLength(100).IsRequired();
        b.Property(e => e.Unit).HasMaxLength(30).IsRequired(false);
        b.Property(e => e.Flag).HasConversion(v => v == null ? (int?)null : (int)v.Value, v => v == null ? null : (TopLab.Domain.Common.Enums.ProfileResultFlag)v.Value).HasColumnType("tinyint").IsRequired(false);
        b.Property(e => e.IsVerified).IsRequired();
        b.Property(e => e.IsPrinted).IsRequired();
        b.Property(e => e.PrintCount).IsRequired();
        b.Property(e => e.LastPrintedByUserId).IsRequired(false);
        b.Property(e => e.LastPrintedAtUtc).HasColumnType("datetime2").IsRequired(false);
        b.HasOne<PatientTest>().WithMany().HasForeignKey(e => e.PatientTestId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne<TopLab.Domain.Tests.Analyte>().WithMany().HasForeignKey(e => e.AnalyteId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(e => e.PatientTestId);
        b.HasIndex(e => e.AnalyteId);
    }
}