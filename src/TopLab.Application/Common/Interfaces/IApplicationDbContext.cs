namespace TopLab.Application.Common.Interfaces;

/// <summary>
/// Application-layer port for persistence. Declared in Application so handlers
/// depend only on this abstraction; the concrete <c>ApplicationDbContext</c>
/// lives in Infrastructure (Architecture §4.2, §4.3). This interface exposes
/// no EF Core types — handlers work with <c>IQueryable</c> and plain methods,
/// keeping the Application layer free of EF Core coupling (Coding Standards §3.1, §5.2).
/// </summary>
public interface IApplicationDbContext
{
    IQueryable<TEntity> Set<TEntity>()
        where TEntity : class;

    void Add<TEntity>(TEntity entity)
        where TEntity : class;

    void Update<TEntity>(TEntity entity)
        where TEntity : class;

    void Remove<TEntity>(TEntity entity)
        where TEntity : class;

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    Task<bool> CanConnectAsync(CancellationToken cancellationToken = default);
}
