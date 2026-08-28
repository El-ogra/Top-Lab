using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TopLab.Domain.Tests;

namespace TopLab.Infrastructure.Persistence.Configurations;

public sealed class WorkGroupLogItemConfiguration : IEntityTypeConfiguration<WorkGroupLogItem>
{
    public void Configure(EntityTypeBuilder<WorkGroupLogItem> b)
    {
        b.HasKey(e => new { e.WorkGroupLogId, e.TestId });
        b.Property(e => e.WorkGroupLogId).HasConversion(v => v.Value, v => TopLab.Domain.Common.Ids.WorkGroupLogId.Create(v));
        b.Property(e => e.TestId).HasConversion(v => v.Value, v => TopLab.Domain.Common.Ids.TestId.Create(v));
        // FK via convention (removed explicit HasOne to avoid shadow)

        // FK via convention (removed explicit HasOne to avoid shadow)
    }
}
