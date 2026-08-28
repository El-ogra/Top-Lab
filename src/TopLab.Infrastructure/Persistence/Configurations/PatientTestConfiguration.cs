using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TopLab.Domain.Results;
using TopLab.Domain.Common.Ids;

namespace TopLab.Infrastructure.Persistence.Configurations;

public sealed class PatientTestConfiguration : IEntityTypeConfiguration<PatientTest>
{
    public void Configure(EntityTypeBuilder<PatientTest> b)
    {
        b.HasKey(e => e.Id);
        b.Property(e => e.Id).HasConversion(v => v.Value, v => PatientTestId.Create(v)).ValueGeneratedOnAdd();
        b.Property(e => e.PatientId).HasConversion(v => v.Value, v => PatientId.Create(v)).IsRequired();
        b.Property(e => e.TestId).HasConversion(v => v.Value, v => TestId.Create(v)).IsRequired();
        b.Property(e => e.PriceAtOrderTime).HasColumnType("decimal(18,2)").HasPrecision(18,2).IsRequired();
        b.Property(e => e.IsUrine).IsRequired();
        b.Property(e => e.IsStool).IsRequired();
        b.Property(e => e.IsBlood).IsRequired();
        b.Property(e => e.IsSemen).IsRequired();
        b.Property(e => e.IsCsf).IsRequired();
        b.Property(e => e.IsTakenOutsideLab).IsRequired();
        b.Property(e => e.IsSampleDrawn).IsRequired();
        b.Property(e => e.SampleDrawnAtUtc).HasColumnType("datetime2").IsRequired(false);
        b.Property(e => e.ResultValue).HasMaxLength(200).IsRequired(false);
        b.Property(e => e.ResultFlag).HasConversion(v => v == null ? (int?)null : (int)v.Value, v => v == null ? null : (TopLab.Domain.Common.Enums.ResultFlag)v.Value).HasColumnType("tinyint").IsRequired(false);
        b.Property(e => e.Notes).HasMaxLength(500).IsRequired(false);
        b.Property(e => e.EnteredByUserId).IsRequired(false);
        b.Property(e => e.EnteredAtUtc).HasColumnType("datetime2").IsRequired(false);
        b.Property(e => e.IsReviewed).IsRequired();
        b.Property(e => e.ReviewedByUserId).IsRequired(false);
        b.Property(e => e.ReviewedAtUtc).HasColumnType("datetime2").IsRequired(false);
        b.Property(e => e.IsPrinted).IsRequired();
        b.Property(e => e.PrintCount).IsRequired();
        b.Property(e => e.LastPrintedByUserId).IsRequired(false);
        b.Property(e => e.LastPrintedAtUtc).HasColumnType("datetime2").IsRequired(false);
        b.Property(e => e.IsDelivered).IsRequired();
        b.Property(e => e.DeliveredByUserId).IsRequired(false);
        b.Property(e => e.DeliveredAtUtc).HasColumnType("datetime2").IsRequired(false);
        b.Property(e => e.IsExported).IsRequired();
        b.Property(e => e.ExportedAtUtc).HasColumnType("datetime2").IsRequired(false);
        b.HasOne<TopLab.Domain.Patients.Patient>().WithMany().HasForeignKey(e => e.PatientId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne<TopLab.Domain.Tests.Test>().WithMany().HasForeignKey(e => e.TestId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(e => e.PatientId);
        b.HasIndex(e => e.TestId);
        b.HasIndex(e => new { e.IsReviewed, e.IsPrinted, e.IsDelivered });
    }
}
