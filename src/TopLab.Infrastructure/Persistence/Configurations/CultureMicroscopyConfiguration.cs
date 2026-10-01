using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TopLab.Domain.Results;

namespace TopLab.Infrastructure.Persistence.Configurations;

public sealed class CultureMicroscopyConfiguration : IEntityTypeConfiguration<CultureMicroscopy>
{
    public void Configure(EntityTypeBuilder<CultureMicroscopy> b)
    {
        b.HasKey(e => e.PatientTestId);
        b.Property(e => e.PatientTestId).HasConversion(v => v.Value, v => TopLab.Domain.Common.Ids.PatientTestId.Create(v));
        b.Property(e => e.PusCells).HasMaxLength(20).IsRequired(false);
        b.Property(e => e.RedBloodCells).HasMaxLength(20).IsRequired(false);
        b.Property(e => e.EpithelialCells).HasMaxLength(20).IsRequired(false);
        b.Property(e => e.Crystals).HasMaxLength(20).IsRequired(false);
        b.Property(e => e.Fungi).HasMaxLength(20).IsRequired(false);
        b.Property(e => e.OthersOne).HasMaxLength(20).IsRequired(false);
        b.Property(e => e.OthersTwo).HasMaxLength(20).IsRequired(false);
        b.Property(e => e.OthersThree).HasMaxLength(20).IsRequired(false);
        b.Property(e => e.IsDirect).IsRequired();
        b.HasOne<CultureResult>().WithOne().HasForeignKey<CultureMicroscopy>(e => e.PatientTestId).OnDelete(DeleteBehavior.Cascade);
    }
}
