using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TopLab.Domain.Patients;
using TopLab.Domain.Common.Ids;

namespace TopLab.Infrastructure.Persistence.Configurations;

public sealed class PatientPhoneNumberConfiguration : IEntityTypeConfiguration<PatientPhoneNumber>
{
    public void Configure(EntityTypeBuilder<PatientPhoneNumber> b)
    {
        b.HasKey(e => e.Id);
        b.Property(e => e.Id).HasConversion(v => v.Value, v => PatientPhoneNumberId.Create(v)).ValueGeneratedOnAdd();
        b.Property(e => e.PatientId).HasConversion(v => v.Value, v => PatientId.Create(v)).IsRequired();
        b.Property(e => e.PhoneNumber).HasMaxLength(30).IsRequired();
        b.HasIndex(e => e.PhoneNumber);
        b.Property(e => e.SortOrder).HasColumnType("tinyint").IsRequired();
        b.HasIndex(e => e.PatientId);
        // FK to Patient via convention - no explicit HasOne to avoid shadow
    }
}
