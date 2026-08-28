using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TopLab.Domain.Users;

namespace TopLab.Infrastructure.Persistence.Configurations;

public sealed class UserPermissionGrantConfiguration : IEntityTypeConfiguration<UserPermissionGrant>
{
    public void Configure(EntityTypeBuilder<UserPermissionGrant> b)
    {
        b.HasKey(e => new { e.UserId, e.PermissionId });
        b.Property(e => e.UserId).HasConversion(v => v.Value, v => TopLab.Domain.Common.Ids.UserId.Create(v));
        b.Property(e => e.PermissionId).HasConversion(v => v.Value, v => TopLab.Domain.Common.Ids.PermissionId.Create(v));
        // FK via convention (removed explicit HasOne to avoid shadow)

        // FK via convention (removed explicit HasOne to avoid shadow)
    }
}
