using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TopLab.Domain.Patients;
using TopLab.Domain.Common.Ids;

namespace TopLab.Infrastructure.Persistence.Configurations;

public sealed class PatientTitleConfiguration : IEntityTypeConfiguration<PatientTitle>
{
    public void Configure(EntityTypeBuilder<PatientTitle> b)
    {
        b.HasKey(e => e.Id);
        b.Property(e => e.Id).HasConversion(v => v.Value, v => PatientTitleId.Create(v)).ValueGeneratedOnAdd();
        b.Property(e => e.TitleText).HasMaxLength(50).IsRequired();
        b.Property(e => e.IsDefault).IsRequired();
    }
}
