using TopLab.Application.Common.Interfaces;
using TopLab.Domain.Users;

namespace TopLab.Application.Tests.Common.Fakes;

public sealed class FakeApplicationDbContext : IApplicationDbContext
{
    public List<User> Users { get; } = new();
    public List<Permission> Permissions { get; } = new();
    public List<UserPermissionGrant> UserPermissionGrants { get; } = new();

    public int SaveChangesCallCount { get; private set; }

    public IQueryable<TEntity> Set<TEntity>() where TEntity : class
    {
        if (typeof(TEntity) == typeof(User))
        {
            return (IQueryable<TEntity>)(object)Users.AsQueryable();
        }

        if (typeof(TEntity) == typeof(Permission))
        {
            return (IQueryable<TEntity>)(object)Permissions.AsQueryable();
        }

        if (typeof(TEntity) == typeof(UserPermissionGrant))
        {
            return (IQueryable<TEntity>)(object)UserPermissionGrants.AsQueryable();
        }

        return Enumerable.Empty<TEntity>().AsQueryable();
    }

    public void Add<TEntity>(TEntity entity) where TEntity : class
    {
        if (entity is User u) Users.Add(u);
        else if (entity is Permission p) Permissions.Add(p);
        else if (entity is UserPermissionGrant g) UserPermissionGrants.Add(g);
        else throw new NotSupportedException($"Add not supported for {typeof(TEntity).Name}");
    }

    public void Update<TEntity>(TEntity entity) where TEntity : class
    {
        // In-memory list: no action needed, entity is already reference-tracked.
    }

    public void Remove<TEntity>(TEntity entity) where TEntity : class
    {
        if (entity is User u) Users.Remove(u);
        else if (entity is Permission p) Permissions.Remove(p);
        else if (entity is UserPermissionGrant g) UserPermissionGrants.Remove(g);
        else throw new NotSupportedException($"Remove not supported for {typeof(TEntity).Name}");
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveChangesCallCount++;
        return Task.FromResult(1);
    }

    public Task<bool> CanConnectAsync(CancellationToken cancellationToken = default) => Task.FromResult(true);
}
