using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TopLab.Domain.Patients;
using TopLab.Domain.Common.Ids;

namespace TopLab.Infrastructure.Persistence.Configurations;

public sealed class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> b)
    {
        b.HasKey(e => e.Id);
        b.Property(e => e.Id).HasConversion(v => v.Value, v => PatientId.Create(v)).ValueGeneratedOnAdd().HasColumnName("PatientId");
        b.Property(e => e.LabId)
            .HasConversion(v => v == null ? (string?)null : v.Value, v => v == null ? null : LabId.Create(v))
            .HasMaxLength(30)
            .IsRequired(false);
        b.HasIndex(e => e.LabId);
        b.Property(e => e.Title).HasMaxLength(50).IsRequired(false);
        b.Property(e => e.FullName).HasMaxLength(200).IsRequired();
        b.HasIndex(e => e.FullName);
        b.Property(e => e.Sex).HasConversion<int>().HasColumnType("tinyint").IsRequired();
        b.Property(e => e.AgeValue).IsRequired();
        b.Property(e => e.AgeUnit).HasConversion<int>().HasColumnType("tinyint").IsRequired();
        b.Property(e => e.NationalId).HasMaxLength(30).IsRequired(false);
        b.HasIndex(e => e.NationalId);
        b.Property(e => e.Address).HasMaxLength(300).IsRequired(false);
        b.Property(e => e.TreatingDoctorId).HasConversion(v => v == null ? (int?)null : v.Value, v => v == null ? null : ExternalEntityId.Create(v.Value)).IsRequired(false);
        b.Property(e => e.ReferralEntityId).HasConversion(v => v == null ? (int?)null : v.Value, v => v == null ? null : ExternalEntityId.Create(v.Value)).IsRequired(false);
        b.Property(e => e.AccountType).HasConversion<int>().HasColumnType("tinyint").IsRequired();
        b.Property(e => e.IsVip).IsRequired();
        b.Property(e => e.RegistrationDateUtc).HasColumnType("datetime2").IsRequired();
        b.HasIndex(e => e.RegistrationDateUtc);
        b.Property(e => e.PickupDateUtc).HasColumnType("datetime2").IsRequired(false);
        b.Property(e => e.IsFastingIndicated).IsRequired();
        b.Property(e => e.FastingHours).IsRequired(false);
        b.Property(e => e.RecentContrastImaging).IsRequired();
        b.Property(e => e.Notes).HasMaxLength(1000).IsRequired(false);
    }
}
