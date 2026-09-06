using TopLab.Application.Common.Interfaces;

namespace TopLab.Application.Tests.Common.Fakes;

/// <summary>
/// IApplicationDbContext that wraps <see cref="FakeApplicationDbContext"/> and can be
/// told to throw a simulated unique-index violation on the next save, mirroring
/// SQL Server behavior for the <c>IX_Tests_TestCode</c> index. Used to test the
/// residual unique-violation catch in command handlers (M-12 §4.3).
/// </summary>
public sealed class UniqueViolationFakeApplicationDbContext : IApplicationDbContext
{
    private readonly FakeApplicationDbContext _inner = new();

    public bool ThrowUniqueViolation { get; set; }

    public FakeApplicationDbContext Inner => _inner;

    public int SaveChangesCallCount => _inner.SaveChangesCallCount;

    public IQueryable<TEntity> Set<TEntity>() where TEntity : class => _inner.Set<TEntity>();

    public void Add<TEntity>(TEntity entity) where TEntity : class => _inner.Add(entity);

    public void Update<TEntity>(TEntity entity) where TEntity : class => _inner.Update(entity);

    public void Remove<TEntity>(TEntity entity) where TEntity : class => _inner.Remove(entity);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        if (ThrowUniqueViolation)
        {
            throw new Exception(
                "The INSERT statement conflicted with the UNIQUE INDEX 'IX_Tests_TestCode'. duplicate key value is (CBC).");
        }

        return _inner.SaveChangesAsync(cancellationToken);
    }

    public Task<bool> CanConnectAsync(CancellationToken cancellationToken = default)
        => _inner.CanConnectAsync(cancellationToken);
}