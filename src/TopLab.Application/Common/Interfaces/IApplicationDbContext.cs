using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace TopLab.Application.Common.Interfaces;

/// <summary>
/// Application-layer port for the EF Core database context. Declared in Application
/// so handlers depend only on this abstraction; the concrete <c>ApplicationDbContext</c>
/// lives in Infrastructure (Architecture §4.2, §4.3, ADR-0004, ADR-0011).
/// </summary>
public interface IApplicationDbContext
{
    /// <summary>
    /// Returns a <see cref="DbSet{TEntity}"/> for the given entity type so feature
    /// code can request sets by type without naming concrete properties.
    /// </summary>
    DbSet<TEntity> Set<TEntity>()
        where TEntity : class;

    /// <summary>
    /// Begins tracking the entity in the <see cref="EntityState.Added"/> state.
    /// </summary>
    EntityEntry<TEntity> Add<TEntity>(TEntity entity)
        where TEntity : class;

    /// <summary>
    /// Begins tracking the entity in the <see cref="EntityState.Modified"/> state.
    /// </summary>
    EntityEntry<TEntity> Update<TEntity>(TEntity entity)
        where TEntity : class;

    /// <summary>
    /// Begins tracking the entity in the <see cref="EntityState.Removed"/> state.
    /// </summary>
    EntityEntry<TEntity> Remove<TEntity>(TEntity entity)
        where TEntity : class;

    /// <summary>
    /// Persists every pending change to the underlying database. Returns the number
    /// of state entries written to the database.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Exposes the underlying <see cref="DatabaseFacade"/> for the rare cases that
    /// must execute raw SQL or open a transaction directly. Feature code should
    /// avoid this in favour of higher-level handlers.
    /// </summary>
    DatabaseFacade Database { get; }
}
