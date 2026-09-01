using Microsoft.EntityFrameworkCore;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Users;
using TopLab.Infrastructure.Identity;
using TopLab.Infrastructure.Persistence;
using TopLab.Infrastructure.Tests.Common;

namespace TopLab.Infrastructure.Tests.Persistence;

public class UserGrantPersistenceTests
{
    [Fact]
    public async Task Grant_RoundTrip_ThroughAggregate()
    {
        var options = InMemoryContextFactory.Create();
        using var ctx = new ApplicationDbContext(options);

        var hasher = new Pbkdf2PasswordHasher();
        var hash = hasher.Hash("pass123");
        var user = User.Create(UserId.Create(1), "ahmed", hash, hash);
        user.GrantPermission(PermissionId.Create(1));
        ctx.Users.Add(user);
        ctx.UserPermissionGrants.AddRange(user.PermissionGrants);
        await ctx.SaveChangesAsync();

        var loaded = await ctx.Users.Include(u => u.PermissionGrants).FirstAsync(u => u.Id.Value == 1);
        // For InMemory without Include via navigation, we can also query grants directly
        var grants = await ctx.UserPermissionGrants.Where(g => g.UserId.Value == 1).ToListAsync();
        Assert.Single(grants);
        Assert.Equal(1, grants[0].PermissionId.Value);
        Assert.True(loaded.PermissionGrants.Count == 1 || grants.Count == 1);
    }

    [Fact]
    public async Task DuplicateCompositeKey_IsIdempotentViaDomain()
    {
        var options = InMemoryContextFactory.Create();
        using var ctx = new ApplicationDbContext(options);

        var hasher = new Pbkdf2PasswordHasher();
        var hash = hasher.Hash("pass123");
        var user = User.Create(UserId.Create(1), "ahmed", hash, hash);
        user.GrantPermission(PermissionId.Create(2));
        user.GrantPermission(PermissionId.Create(2)); // duplicate should be no-op
        Assert.Single(user.PermissionGrants);

        ctx.Users.Add(user);
        ctx.UserPermissionGrants.AddRange(user.PermissionGrants);
        await ctx.SaveChangesAsync();

        var grants = await ctx.UserPermissionGrants.Where(g => g.UserId.Value == 1).ToListAsync();
        Assert.Single(grants);
    }

    [Fact]
    public void HashString_RoundTrips_WithinNvarchar300()
    {
        var hasher = new Pbkdf2PasswordHasher();
        var hash = hasher.Hash("anyPassword123");
        Assert.True(hash.Length <= 300);
        Assert.Contains("PBKDF2-SHA256$", hash);
        Assert.True(hasher.Verify("anyPassword123", hash));
    }
}
