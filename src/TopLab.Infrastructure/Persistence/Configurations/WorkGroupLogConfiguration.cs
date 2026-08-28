using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TopLab.Domain.Tests;
using TopLab.Domain.Common.Ids;

namespace TopLab.Infrastructure.Persistence.Configurations;

public sealed class WorkGroupLogConfiguration : IEntityTypeConfiguration<WorkGroupLog>
{
    public void Configure(EntityTypeBuilder<WorkGroupLog> b)
    {
        b.HasKey(e => e.Id);
        b.Property(e => e.Id).HasConversion(v => v.Value, v => WorkGroupLogId.Create(v)).ValueGeneratedOnAdd();
        b.Property(e => e.Name).HasMaxLength(150).IsRequired();
    }
}
