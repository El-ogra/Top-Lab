using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TopLab.Domain.Tests;
using TopLab.Domain.Common.Ids;

namespace TopLab.Infrastructure.Persistence.Configurations;

public sealed class TestConfiguration : IEntityTypeConfiguration<Test>
{
    public void Configure(EntityTypeBuilder<Test> b)
    {
        b.HasKey(e => e.Id);
        b.Property(e => e.Id).HasConversion(v => v.Value, v => TestId.Create(v)).ValueGeneratedOnAdd();
        b.Property(e => e.Name).HasMaxLength(150).IsRequired();
        b.Property(e => e.ReportName).HasMaxLength(150).IsRequired();
        b.Property(e => e.ReceiptName).HasMaxLength(150).IsRequired();
        b.Property(e => e.TestGroupId).HasConversion(v => v == null ? (int?)null : v.Value, v => v == null ? null : TestGroupId.Create(v.Value)).IsRequired(false);
        b.Property(e => e.Barcode).HasMaxLength(50).IsRequired(false);
        b.Property(e => e.CompletionDurationMinutes).IsRequired();
        b.Property(e => e.IsSentOut).IsRequired();
        b.Property(e => e.SentOutCostPrice).HasColumnType("decimal(18,2)").HasPrecision(18,2).IsRequired(false);
        b.Property(e => e.PatientPrice).HasColumnType("decimal(18,2)").HasPrecision(18,2).IsRequired();
        b.Property(e => e.LabToLabPrice).HasColumnType("decimal(18,2)").HasPrecision(18,2).IsRequired(false);
        b.Property(e => e.ResultKind).HasConversion<int>().HasColumnType("tinyint").IsRequired();
        b.Property(e => e.IsCultureType).IsRequired();
        b.HasOne<TestGroup>().WithMany().HasForeignKey(e => e.TestGroupId).OnDelete(DeleteBehavior.SetNull);
        b.HasIndex(e => e.TestGroupId);
        b.HasIndex(e => e.Name);
    }
}
