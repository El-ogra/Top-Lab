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
        b.HasData(new { Id = MedicalConditionTypeId.Create(1), Name = "حمل", Category = MedicalConditionCategory.Pregnancy });
    }
}
