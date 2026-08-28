using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TopLab.Domain.Results;
using TopLab.Domain.Common.Ids;

namespace TopLab.Infrastructure.Persistence.Configurations;

public sealed class CultureAntibioticResultConfiguration : IEntityTypeConfiguration<CultureAntibioticResult>
{
    public void Configure(EntityTypeBuilder<CultureAntibioticResult> b)
    {
        b.HasKey(e => e.Id);
        b.Property(e => e.Id).HasConversion(v => v.Value, v => CultureAntibioticResultId.Create(v)).ValueGeneratedOnAdd();
        b.Property(e => e.PatientTestId).HasConversion(v => v.Value, v => PatientTestId.Create(v)).IsRequired();
        b.Property(e => e.AntibioticId).HasConversion(v => v.Value, v => AntibioticId.Create(v)).IsRequired();
        b.Property(e => e.SensitivityCategory).HasConversion<int>().HasColumnType("tinyint").IsRequired();
        b.HasOne<CultureResult>().WithMany().HasForeignKey(e => e.PatientTestId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne<TopLab.Domain.Tests.Antibiotic>().WithMany().HasForeignKey(e => e.AntibioticId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(e => e.PatientTestId);
    }
}
