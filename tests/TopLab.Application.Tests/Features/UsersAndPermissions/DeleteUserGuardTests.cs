using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.UsersAndPermissions.Commands.DeleteUser;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Users;

using TopLab.Application.Tests.Common.Fakes;

namespace TopLab.Application.Tests.Features.UsersAndPermissions;

/// <summary>
/// Slice 3 (M-04): delete-user reference guard fails closed.
/// A throwing probe yields Unexpected (not Conflict) with zero Remove and zero SaveChangesAsync.
/// </summary>
public class DeleteUserGuardTests
{
    [Fact]
    public async Task ThrowingProbe_YieldsUnexpected_NotConflict()
    {
        var db = new ThrowingProbeDbContext();
        var handler = new DeleteUserCommandHandler(db, new FakeCurrentUserService());
        var user = User.Create(UserId.Create(1), "testuser", "hash", "winhash");
        db.Users.Add(user);

        var result = await handler.Handle(new DeleteUserCommand(1), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Unexpected, result.Error!.Type);
        Assert.Equal(0, db.RemoveCallCount);
        Assert.Equal(0, db.SaveChangesAsyncCallCount);
    }

    [Fact]
    public async Task NoReferences_StillDeletes()
    {
        var db = new SimpleDbContext();
        var handler = new DeleteUserCommandHandler(db, new FakeCurrentUserService());
        var user = User.Create(UserId.Create(1), "testuser", "hash", "winhash");
        db.Users.Add(user);

        var result = await handler.Handle(new DeleteUserCommand(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, db.RemoveCallCount);
    }

    [Fact]
    public async Task ThrowingSet_YieldsUnexpected()
    {
        var db = new ThrowingSetDbContext();
        var handler = new DeleteUserCommandHandler(db, new FakeCurrentUserService());
        var user = User.Create(UserId.Create(1), "testuser", "hash", "winhash");
        db.Users.Add(user);

        var result = await handler.Handle(new DeleteUserCommand(1), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Unexpected, result.Error!.Type);
        Assert.Equal(0, db.SaveChangesAsyncCallCount);
    }

    /// <summary>
    /// DbContext where non-User Set&lt;T&gt;().Any() throws.
    /// </summary>
    private sealed class ThrowingProbeDbContext : IApplicationDbContext
    {
        public List<User> Users { get; } = new();
        public int RemoveCallCount { get; private set; }
        public int SaveChangesAsyncCallCount { get; private set; }

        public IQueryable<TEntity> Set<TEntity>() where TEntity : class
        {
            if (typeof(TEntity) == typeof(User))
                return (IQueryable<TEntity>)Users.AsQueryable();
            throw new InvalidOperationException("Simulated probe failure");
        }

        public void Add<TEntity>(TEntity entity) where TEntity : class { }
        public void Update<TEntity>(TEntity entity) where TEntity : class { }
        public void Remove<TEntity>(TEntity entity) where TEntity : class { RemoveCallCount++; }
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveChangesAsyncCallCount++;
            return Task.FromResult(1);
        }
        public Task<bool> CanConnectAsync(CancellationToken cancellationToken = default) => Task.FromResult(true);
    }

    /// <summary>
    /// DbContext where Set&lt;T&gt;() itself throws.
    /// </summary>
    private sealed class ThrowingSetDbContext : IApplicationDbContext
    {
        public List<User> Users { get; } = new();
        public int SaveChangesAsyncCallCount { get; private set; }

        public IQueryable<TEntity> Set<TEntity>() where TEntity : class
        {
            if (typeof(TEntity) == typeof(User))
                return (IQueryable<TEntity>)Users.AsQueryable();
            throw new InvalidOperationException("Simulated Set failure");
        }

        public void Add<TEntity>(TEntity entity) where TEntity : class { }
        public void Update<TEntity>(TEntity entity) where TEntity : class { }
        public void Remove<TEntity>(TEntity entity) where TEntity : class { }
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveChangesAsyncCallCount++;
            return Task.FromResult(1);
        }
        public Task<bool> CanConnectAsync(CancellationToken cancellationToken = default) => Task.FromResult(true);
    }

    /// <summary>
    /// Simple DbContext with empty sets (no references).
    /// </summary>
    private sealed class SimpleDbContext : IApplicationDbContext
    {
        public List<User> Users { get; } = new();
        public int RemoveCallCount { get; private set; }

        public IQueryable<TEntity> Set<TEntity>() where TEntity : class
        {
            if (typeof(TEntity) == typeof(User))
                return (IQueryable<TEntity>)Users.AsQueryable();
            return Enumerable.Empty<TEntity>().AsQueryable();
        }

        public void Add<TEntity>(TEntity entity) where TEntity : class { }
        public void Update<TEntity>(TEntity entity) where TEntity : class { }
        public void Remove<TEntity>(TEntity entity) where TEntity : class { RemoveCallCount++; }
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(1);
        public Task<bool> CanConnectAsync(CancellationToken cancellationToken = default) => Task.FromResult(true);
    }
}
