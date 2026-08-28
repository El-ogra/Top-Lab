using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TopLab.Domain.Results;

namespace TopLab.Infrastructure.Persistence.Configurations;

public sealed class CultureResultConfiguration : IEntityTypeConfiguration<CultureResult>
{
    public void Configure(EntityTypeBuilder<CultureResult> b)
    {
        b.HasKey(e => e.PatientTestId);
        b.Property(e => e.PatientTestId).HasConversion(v => v.Value, v => TopLab.Domain.Common.Ids.PatientTestId.Create(v));
        b.Property(e => e.Sample).HasMaxLength(100).IsRequired(false);
        b.Property(e => e.OrganismA).HasMaxLength(150).IsRequired(false);
        b.Property(e => e.OrganismB).HasMaxLength(150).IsRequired(false);
        b.Property(e => e.OrganismC).HasMaxLength(150).IsRequired(false);
        b.Property(e => e.CultureCondition).HasMaxLength(200).IsRequired(false);
        b.Property(e => e.ColonyCount).HasMaxLength(50).IsRequired(false);
        b.HasOne<PatientTest>().WithOne().HasForeignKey<CultureResult>(e => e.PatientTestId).OnDelete(DeleteBehavior.Cascade);
    }
}
