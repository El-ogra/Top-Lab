using TopLab.Application.Common.Interfaces;

namespace TopLab.Application.Tests.Common.Fakes;

/// <summary>
/// IApplicationDbContext that wraps <see cref="FakeApplicationDbContext"/> and can be
/// told to throw a simulated SQL Server reference conflict on the next save, mirroring
/// the message used by <c>DeleteAntibioticCommandHandler</c>'s IsReferenceConflict catch.
/// </summary>
public sealed class ReferenceConflictFakeApplicationDbContext : IApplicationDbContext
{
    private readonly FakeApplicationDbContext _inner = new();

    public bool ThrowReferenceConflict { get; set; }

    public FakeApplicationDbContext Inner => _inner;

    public int SaveChangesCallCount => _inner.SaveChangesCallCount;

    public IQueryable<TEntity> Set<TEntity>() where TEntity : class => _inner.Set<TEntity>();

    public void Add<TEntity>(TEntity entity) where TEntity : class => _inner.Add(entity);

    public void Update<TEntity>(TEntity entity) where TEntity : class => _inner.Update(entity);

    public void Remove<TEntity>(TEntity entity) where TEntity : class => _inner.Remove(entity);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        if (ThrowReferenceConflict)
        {
            throw new Exception(
                "The DELETE statement conflicted with the REFERENCE constraint 'FK_CultureAntibioticResults_Antibiotics_AntibioticId'.");
        }

        return _inner.SaveChangesAsync(cancellationToken);
    }

    public Task<bool> CanConnectAsync(CancellationToken cancellationToken = default)
        => _inner.CanConnectAsync(cancellationToken);
}