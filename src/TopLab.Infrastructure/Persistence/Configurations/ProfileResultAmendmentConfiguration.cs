using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Results;

namespace TopLab.Infrastructure.Persistence.Configurations;

public sealed class ProfileResultAmendmentConfiguration : IEntityTypeConfiguration<ProfileResultAmendment>
{
    public void Configure(EntityTypeBuilder<ProfileResultAmendment> b)
    {
        b.HasKey(e => e.Id);
        b.Property(e => e.Id).HasConversion(v => v.Value, v => ProfileResultAmendmentId.Create(v)).ValueGeneratedOnAdd().HasColumnName("ProfileResultAmendmentId");
        b.Property(e => e.ProfileResultItemId).HasConversion(v => v.Value, v => ProfileResultItemId.Create(v)).IsRequired();
        b.Property(e => e.AmendedByUserId).IsRequired();
        b.Property(e => e.AmendedAtUtc).HasColumnType("datetime2").IsRequired();
        b.Property(e => e.OldResultValue).HasMaxLength(100).IsRequired();
        b.Property(e => e.OldUnit).HasMaxLength(30).IsRequired(false);
        b.Property(e => e.OldFlag).HasConversion(v => v == null ? (int?)null : (int)v.Value, v => v == null ? null : (ProfileResultFlag)v.Value).HasColumnType("tinyint").IsRequired(false);
        b.Property(e => e.NewResultValue).HasMaxLength(100).IsRequired();
        b.Property(e => e.NewUnit).HasMaxLength(30).IsRequired(false);
        b.Property(e => e.NewFlag).HasConversion(v => v == null ? (int?)null : (int)v.Value, v => v == null ? null : (ProfileResultFlag)v.Value).HasColumnType("tinyint").IsRequired(false);
        b.Property(e => e.Reason).HasMaxLength(500).IsRequired(false);
        b.HasOne<ProfileResultItem>().WithMany().HasForeignKey(e => e.ProfileResultItemId).OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(e => e.ProfileResultItemId);
    }
}