using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TopLab.Domain.Users;
using TopLab.Domain.Common.Ids;

namespace TopLab.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.HasKey(e => e.Id);
        b.Property(e => e.Id).HasConversion(v => v.Value, v => UserId.Create(v)).ValueGeneratedOnAdd().HasColumnName("UserId");
        b.Property(e => e.UserName).HasMaxLength(100).IsRequired();
        b.HasIndex(e => e.UserName).IsUnique();
        b.Property(e => e.PasswordHash).HasMaxLength(300).IsRequired();
        b.Property(e => e.InternalWindowsPasswordHash).HasMaxLength(300).IsRequired();
        b.Property(e => e.IsAbsolutePermission).IsRequired();
        b.Property(e => e.DiscountLimitPercent).HasColumnType("decimal(5,2)").HasPrecision(5,2).IsRequired();
        b.Property(e => e.BlockPrintOnRemainingBalance).IsRequired();
        b.Property(e => e.WorkStartTime).HasColumnType("time").IsRequired(false);
        b.Property(e => e.WorkEndTime).HasColumnType("time").IsRequired(false);
        b.Property(e => e.HasBreakPeriod).IsRequired();
        b.Property(e => e.BreakDurationMinutes).IsRequired(false);
        b.Property(e => e.LastLoginAtUtc).HasColumnType("datetime2").IsRequired(false);
        b.Property(e => e.IsActive).IsRequired();
    }
}
