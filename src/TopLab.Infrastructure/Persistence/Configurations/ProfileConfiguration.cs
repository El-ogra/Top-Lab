using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Tests;

namespace TopLab.Infrastructure.Persistence.Configurations;

public sealed class ProfileConfiguration : IEntityTypeConfiguration<Profile>
{
    public void Configure(EntityTypeBuilder<Profile> b)
    {
        b.HasKey(e => e.Id);
        b.Property(e => e.Id).HasConversion(v => v.Value, v => ProfileId.Create(v)).ValueGeneratedOnAdd().HasColumnName("ProfileId");
        b.Property(e => e.Name).HasMaxLength(150).IsRequired();
        b.Property(e => e.TestId).HasConversion(v => v.Value, v => TestId.Create(v)).IsRequired();
        b.Property(e => e.FixedPrice).HasColumnType("decimal(18,2)").HasPrecision(18, 2).IsRequired();
        b.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);
        b.HasOne<Test>().WithOne().HasForeignKey<Profile>(e => e.TestId).OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(e => e.TestId).IsUnique();
        b.HasIndex(e => e.Name);
    }
}