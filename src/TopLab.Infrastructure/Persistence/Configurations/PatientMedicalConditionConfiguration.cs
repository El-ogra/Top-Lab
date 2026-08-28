using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TopLab.Domain.Patients;

namespace TopLab.Infrastructure.Persistence.Configurations;

public sealed class PatientMedicalConditionConfiguration : IEntityTypeConfiguration<PatientMedicalCondition>
{
    public void Configure(EntityTypeBuilder<PatientMedicalCondition> b)
    {
        b.HasKey(e => new { e.PatientId, e.MedicalConditionTypeId });
        b.Property(e => e.PatientId).HasConversion(v => v.Value, v => TopLab.Domain.Common.Ids.PatientId.Create(v));
        b.Property(e => e.MedicalConditionTypeId).HasConversion(v => v.Value, v => TopLab.Domain.Common.Ids.MedicalConditionTypeId.Create(v));
        // removed FK to avoid shadow - convention
        // removed
    }
}
