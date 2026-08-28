using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TopLab.Domain.Tests;
using TopLab.Domain.Common.Ids;

namespace TopLab.Infrastructure.Persistence.Configurations;

public sealed class AntibioticConfiguration : IEntityTypeConfiguration<Antibiotic>
{
    public void Configure(EntityTypeBuilder<Antibiotic> b)
    {
        b.HasKey(e => e.Id);
        b.Property(e => e.Id).HasConversion(v => v.Value, v => AntibioticId.Create(v)).ValueGeneratedOnAdd().HasColumnName("AntibioticId");
        b.Property(e => e.Name).HasMaxLength(150).IsRequired();
        b.Property(e => e.IsPregnancyFlagged).IsRequired();
        b.Property(e => e.IsChildrenFlagged).IsRequired();
    }
}
