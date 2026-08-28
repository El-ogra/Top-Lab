using Microsoft.EntityFrameworkCore;
using TopLab.Infrastructure.Persistence;
using TopLab.Infrastructure.Persistence.Interceptors;
using TopLab.Infrastructure.Tests.Common;
using TopLab.Infrastructure.Tests.Common.Fakes;

namespace TopLab.Infrastructure.Tests.Persistence.Interceptors;

public class AuditableEntitySaveChangesInterceptorTests
{
    private static readonly DateTime FixedNow = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    private static TestApplicationDbContext BuildContext()
    {
        var options = InMemoryContextFactory.Create(
            new FakeCurrentUserService { UserId = 42 },
            new FakeDateTimeProvider { UtcNow = FixedNow });
        return new TestApplicationDbContext(options);
    }

    private static AuditableEntitySaveChangesInterceptor BuildInterceptor()
    {
        return new AuditableEntitySaveChangesInterceptor(
            new FakeCurrentUserService { UserId = 42 },
            new FakeDateTimeProvider { UtcNow = FixedNow });
    }

    [Fact]
    public async Task GivenAddedAuditableEntity_WhenSaved_ThenCreatedAndModifiedColumnsArePopulated()
    {
        using var ctx = BuildContext();
        var entity = new AuditableTestEntity(1) { Name = "Alpha" };
        ctx.AuditableEntities.Add(entity);

        ctx.ChangeTracker.DetectChanges();
        await ctx.SaveChangesAsync();

        var stored = await ctx.AuditableEntities.SingleAsync();
        Assert.Equal(42, stored.CreatedByUserId);
        Assert.Equal(42, stored.LastModifiedByUserId);
        Assert.Equal(FixedNow, stored.CreatedAtUtc);
        Assert.Equal(FixedNow, stored.LastModifiedAtUtc);
        Assert.Equal(0, stored.ModificationCount);
    }

    [Fact]
    public async Task GivenModifiedAuditableEntity_WhenSaved_ThenLastModifiedUpdatesAndCountIncrements()
    {
        using var ctx = BuildContext();
        ctx.AuditableEntities.Add(new AuditableTestEntity(2) { Name = "Alpha" });
        await ctx.SaveChangesAsync();

        var stored = await ctx.AuditableEntities.SingleAsync();
        stored.Name = "Beta";

        // Force DetectChanges so the InMemory provider marks the entity Modified
        // before SaveChanges dispatches the interceptor.
        ctx.ChangeTracker.DetectChanges();
        await ctx.SaveChangesAsync();

        Assert.Equal(42, stored.CreatedByUserId); // unchanged
        Assert.Equal(FixedNow, stored.CreatedAtUtc); // unchanged
        Assert.Equal(42, stored.LastModifiedByUserId);
        Assert.Equal(FixedNow, stored.LastModifiedAtUtc);
        Assert.Equal(1, stored.ModificationCount);
    }

    [Fact]
    public async Task GivenTwoModifications_WhenSaved_ThenCountIsTwo()
    {
        using var ctx = BuildContext();
        ctx.AuditableEntities.Add(new AuditableTestEntity(3) { Name = "v1" });
        await ctx.SaveChangesAsync();

        var stored = await ctx.AuditableEntities.SingleAsync();
        stored.Name = "v2";
        ctx.ChangeTracker.DetectChanges();
        await ctx.SaveChangesAsync();

        stored.Name = "v3";
        ctx.ChangeTracker.DetectChanges();
        await ctx.SaveChangesAsync();

        Assert.Equal(2, stored.ModificationCount);
    }

    [Fact]
    public async Task GivenNonAuditableEntity_WhenSaved_ThenInterceptorLeavesItAlone()
    {
        // The interceptor must not crash and must not attempt to mutate the
        // audit columns of entities that don't implement IAuditable.
        using var ctx = BuildContext();
        ctx.NonAuditableEntities.Add(new NonAuditableTestEntity(1) { Name = "x" });

        await ctx.SaveChangesAsync();

        var stored = await ctx.NonAuditableEntities.SingleAsync();
        Assert.Equal("x", stored.Name);
    }
}
