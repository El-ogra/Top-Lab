using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TopLab.Domain.Patients;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Common.Enums;

namespace TopLab.Infrastructure.Persistence.Configurations;

public sealed class MedicalConditionTypeConfiguration : IEntityTypeConfiguration<MedicalConditionType>
{
    public void Configure(EntityTypeBuilder<MedicalConditionType> b)
    {
        b.HasKey(e => e.Id);
        b.Property(e => e.Id).HasConversion(v => v.Value, v => MedicalConditionTypeId.Create(v)).ValueGeneratedOnAdd().HasColumnName("MedicalConditionTypeId");
        b.Property(e => e.Name).HasMaxLength(100).IsRequired();
        b.Property(e => e.Category).HasConversion<int>().HasColumnType("tinyint").IsRequired();
        b.HasData(
            // STAGE-01 (baseline 1.2.2.3.9): the pregnancy question — pre-existing row, unchanged.
            new { Id = MedicalConditionTypeId.Create(1), Name = "حمل", Category = MedicalConditionCategory.Pregnancy },
            // STAGE-01: the eleven missing catalogue rows of the medical-history block.
            // The Arabic names are the baseline labels (1.2.2.3.3–1.2.2.3.8, 1.2.2.3.11–1.2.2.3.15);
            // the identifiers continue the seeded sequence after the existing id 1.
            new { Id = MedicalConditionTypeId.Create(2), Name = "هل يؤخذ علاج للسكر", Category = MedicalConditionCategory.Medication },
            new { Id = MedicalConditionTypeId.Create(3), Name = "هل يؤخذ علاج لسيولة الدم", Category = MedicalConditionCategory.Medication },
            new { Id = MedicalConditionTypeId.Create(4), Name = "هل يؤخذ علاج للغدد", Category = MedicalConditionCategory.Medication },
            new { Id = MedicalConditionTypeId.Create(5), Name = "هل يؤخذ علاج للفيروسات", Category = MedicalConditionCategory.Medication },
            new { Id = MedicalConditionTypeId.Create(6), Name = "هل يؤخذ مضاد حيوي", Category = MedicalConditionCategory.Medication },
            new { Id = MedicalConditionTypeId.Create(7), Name = "هل يؤخذ علاج للكبد", Category = MedicalConditionCategory.Medication },
            new { Id = MedicalConditionTypeId.Create(8), Name = "مريض بفقر الدم", Category = MedicalConditionCategory.Condition },
            new { Id = MedicalConditionTypeId.Create(9), Name = "مريض بالتهاب المفاصل", Category = MedicalConditionCategory.Condition },
            new { Id = MedicalConditionTypeId.Create(10), Name = "مريض بالضغط الدم", Category = MedicalConditionCategory.Condition },
            new { Id = MedicalConditionTypeId.Create(11), Name = "مريض بالفشل كلوي", Category = MedicalConditionCategory.Condition },
            new { Id = MedicalConditionTypeId.Create(12), Name = "مريض بالذئبة الحمراء", Category = MedicalConditionCategory.Condition });
    }
}
