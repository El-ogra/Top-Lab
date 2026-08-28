using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TopLab.Domain.Tests;
using TopLab.Domain.Common.Ids;

namespace TopLab.Infrastructure.Persistence.Configurations;

public sealed class TestGroupConfiguration : IEntityTypeConfiguration<TestGroup>
{
    public void Configure(EntityTypeBuilder<TestGroup> b)
    {
        b.HasKey(e => e.Id);
        b.Property(e => e.Id).HasConversion(v => v.Value, v => TestGroupId.Create(v)).ValueGeneratedOnAdd();
        b.Property(e => e.Name).HasMaxLength(150).IsRequired();
    }
}
