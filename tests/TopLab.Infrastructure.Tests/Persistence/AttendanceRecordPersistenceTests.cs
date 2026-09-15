using Microsoft.EntityFrameworkCore;
using TopLab.Domain.Attendance;
using TopLab.Domain.Common;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Users;
using TopLab.Infrastructure.Persistence;
using TopLab.Infrastructure.Tests.Common;
using TopLab.Infrastructure.Tests.Common.Fakes;
using Xunit;

namespace TopLab.Infrastructure.Tests.Persistence;

public sealed class AttendanceRecordPersistenceTests
{
    private static readonly DateTime FixedNow = new(2026, 9, 15, 6, 0, 0, DateTimeKind.Utc);

    private static DbContextOptions<ApplicationDbContext> NewOptions()
    {
        return InMemoryContextFactory.Create(
            new FakeCurrentUserService { UserId = 7 },
            new FakeDateTimeProvider { UtcNow = FixedNow });
    }

    private static User NewUser(int id = 1)
    {
        return User.Create(UserId.Create(id), $"user{id}", "pwdhash", "internalhash");
    }

    private static AttendanceRecord NewRecord(int userId = 1)
    {
        return AttendanceRecord.Create(
            AttendanceRecordId.Create(0),
            UserId.Create(userId),
            FixedNow,
            latenessMinutes: 5);
    }

    [Fact]
    public async Task StoreRetrieve_FullLifecycle_ReloadMatches()
    {
        var options = NewOptions();
        using var ctx = new ApplicationDbContext(options);
        ctx.Set<User>().Add(NewUser());
        await ctx.SaveChangesAsync();

        var record = NewRecord();
        ctx.Set<AttendanceRecord>().Add(record);
        await ctx.SaveChangesAsync();

        record.StartBreak(FixedNow.AddHours(4));
        record.EndBreak(FixedNow.AddHours(4).AddMinutes(30));
        record.CheckOut(FixedNow.AddHours(8), overtimeMinutes: 10);
        await ctx.SaveChangesAsync();

        using var reread = new ApplicationDbContext(options);
        var stored = await reread.Set<AttendanceRecord>().SingleAsync();
        Assert.Equal(1, stored.UserId.Value);
        Assert.Equal(FixedNow, stored.CheckInAtUtc);
        Assert.Equal(FixedNow.AddHours(4), stored.BreakStartAtUtc);
        Assert.Equal(FixedNow.AddHours(4).AddMinutes(30), stored.BreakEndAtUtc);
        Assert.Equal(FixedNow.AddHours(8), stored.CheckOutAtUtc);
        Assert.Equal(5, stored.LatenessMinutes);
        Assert.Equal(10, stored.OvertimeMinutes);
    }

    [Fact]
    public async Task DeleteUser_CascadesToAttendanceRecords()
    {
        var options = NewOptions();
        using var ctx = new ApplicationDbContext(options);
        var user = NewUser();
        ctx.Set<User>().Add(user);
        await ctx.SaveChangesAsync();

        ctx.Set<AttendanceRecord>().Add(NewRecord());
        await ctx.SaveChangesAsync();

        ctx.Set<User>().Remove(user);
        await ctx.SaveChangesAsync();

        Assert.Empty(ctx.Set<AttendanceRecord>());
    }

    [Fact]
    public void RecordToUser_FK_IsCascade_OnUserId()
    {
        using var ctx = new ApplicationDbContext(NewOptions());
        var fk = ctx.Model.FindEntityType(typeof(AttendanceRecord))!
            .GetForeignKeys()
            .Single(fk => fk.PrincipalEntityType.ClrType == typeof(User));

        Assert.Equal("UserId", Assert.Single(fk.Properties).Name);
        Assert.Equal(DeleteBehavior.Cascade, fk.DeleteBehavior);
    }

    [Fact]
    public void UserId_Index_Present()
    {
        using var ctx = new ApplicationDbContext(NewOptions());
        var entityType = ctx.Model.FindEntityType(typeof(AttendanceRecord))!;

        Assert.Contains(
            entityType.GetIndexes(),
            ix => ix.Properties.Count == 1 && ix.Properties[0].Name == "UserId");
    }

    [Fact]
    public void AttendanceRecord_IsNotAuditable_NoAuditColumnsMapped()
    {
        Assert.False(typeof(IAuditableEntity).IsAssignableFrom(typeof(AttendanceRecord)));

        using var ctx = new ApplicationDbContext(NewOptions());
        var propertyNames = ctx.Model.FindEntityType(typeof(AttendanceRecord))!
            .GetProperties()
            .Select(p => p.Name)
            .ToHashSet();

        Assert.DoesNotContain("CreatedByUserId", propertyNames);
        Assert.DoesNotContain("CreatedAtUtc", propertyNames);
        Assert.DoesNotContain("LastModifiedByUserId", propertyNames);
        Assert.DoesNotContain("LastModifiedAtUtc", propertyNames);
        Assert.DoesNotContain("ModificationCount", propertyNames);
    }
}
