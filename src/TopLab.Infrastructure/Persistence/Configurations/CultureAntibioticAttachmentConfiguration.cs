using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TopLab.Domain.Tests;

namespace TopLab.Infrastructure.Persistence.Configurations;

public sealed class CultureAntibioticAttachmentConfiguration : IEntityTypeConfiguration<CultureAntibioticAttachment>
{
    public void Configure(EntityTypeBuilder<CultureAntibioticAttachment> b)
    {
        b.HasKey(e => new { e.TestId, e.AntibioticId });
        b.Property(e => e.TestId).HasConversion(v => v.Value, v => TopLab.Domain.Common.Ids.TestId.Create(v));
        b.Property(e => e.AntibioticId).HasConversion(v => v.Value, v => TopLab.Domain.Common.Ids.AntibioticId.Create(v));
        // FK via convention (removed explicit HasOne to avoid shadow)

        // FK via convention (removed explicit HasOne to avoid shadow)
    }
}
