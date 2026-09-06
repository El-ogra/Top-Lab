using Microsoft.EntityFrameworkCore;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.ExternalEntities;
using TopLab.Infrastructure.Persistence;
using TopLab.Infrastructure.Tests.Common;
using TopLab.Infrastructure.Tests.Common.Fakes;
using Xunit;

namespace TopLab.Infrastructure.Tests.Persistence;

public class ExternalEntityAuditGateTests
{
    private static readonly DateTime FixedNow = new(2026, 3, 1, 10, 0, 0, DateTimeKind.Utc);

    private static ApplicationDbContext BuildContext()
    {
        var options = InMemoryContextFactory.Create(
            new FakeCurrentUserService { UserId = 7 },
            new FakeDateTimeProvider { UtcNow = FixedNow });
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task GivenAddedExternalEntity_WhenSaved_ThenAuditColumnsPopulatedByInterceptor()
    {
        using var ctx = BuildContext();
        ctx.Set<ExternalEntity>().Add(ExternalEntity.Create(
            ExternalEntityId.Create(0), EntityType.TreatingDoctor, "Dr. Ahmed"));
        await ctx.SaveChangesAsync();

        var stored = await ctx.Set<ExternalEntity>().SingleAsync();
        Assert.Equal(7, stored.CreatedByUserId);
        Assert.Equal(7, stored.LastModifiedByUserId);
        Assert.Equal(FixedNow, stored.CreatedAtUtc);
        Assert.Equal(FixedNow, stored.LastModifiedAtUtc);
        Assert.Equal(0, stored.ModificationCount);
    }

    [Fact]
    public async Task GivenUpdatedExternalEntity_WhenSaved_ThenCreatedUntouchedAndModifiedAdvanced()
    {
        using var ctx = BuildContext();
        ctx.Set<ExternalEntity>().Add(ExternalEntity.Create(
            ExternalEntityId.Create(0), EntityType.TreatingDoctor, "Dr. Ahmed"));
        await ctx.SaveChangesAsync();

        var stored = await ctx.Set<ExternalEntity>().SingleAsync();
        stored.Update(EntityType.TreatingDoctor, "Dr. Mahmoud");
        ctx.ChangeTracker.DetectChanges();
        await ctx.SaveChangesAsync();

        Assert.Equal(7, stored.CreatedByUserId);
        Assert.Equal(FixedNow, stored.CreatedAtUtc);
        Assert.Equal(7, stored.LastModifiedByUserId);
        Assert.Equal(FixedNow, stored.LastModifiedAtUtc);
        Assert.Equal(1, stored.ModificationCount);
    }
}
