using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TopLab.Domain.Attendance;
using TopLab.Domain.Common.Ids;

namespace TopLab.Infrastructure.Persistence.Configurations;

public sealed class AttendanceRecordConfiguration : IEntityTypeConfiguration<AttendanceRecord>
{
    public void Configure(EntityTypeBuilder<AttendanceRecord> b)
    {
        b.HasKey(e => e.Id);
        b.Property(e => e.Id).HasConversion(v => v.Value, v => AttendanceRecordId.Create(v)).ValueGeneratedOnAdd();
        b.Property(e => e.UserId).HasConversion(v => v.Value, v => UserId.Create(v)).IsRequired();
        b.Property(e => e.CheckInAtUtc).HasColumnType("datetime2").IsRequired();
        b.Property(e => e.BreakStartAtUtc).HasColumnType("datetime2").IsRequired(false);
        b.Property(e => e.BreakEndAtUtc).HasColumnType("datetime2").IsRequired(false);
        b.Property(e => e.CheckOutAtUtc).HasColumnType("datetime2").IsRequired(false);
        b.Property(e => e.OvertimeMinutes).IsRequired(false);
        b.Property(e => e.LatenessMinutes).IsRequired(false);
        b.HasOne<TopLab.Domain.Users.User>().WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(e => e.UserId);
    }
}
