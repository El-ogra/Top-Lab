using Microsoft.EntityFrameworkCore;
using TopLab.Application.Common.Interfaces;

namespace TopLab.Infrastructure.Persistence;

/// <summary>
/// The single EF Core database context for Top-Lab (ADR-0004, ADR-0011).
/// Mapped to the single shared SQL Server database; one per scope.
/// </summary>
/// <remarks>
/// F4 only wires the plumbing: the interceptor is registered, the
/// <see cref="IApplicationDbContext"/> contract is implemented, and the
/// model-binder discovers every <see cref="IEntityTypeConfiguration{TEntity}"/>
/// present in this assembly. The concrete <c>DbSet&lt;T&gt;</c> declarations
/// for each entity are added in F5 alongside the corresponding entity types
/// and configurations.
/// </remarks>
public partial class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    IQueryable<TEntity> IApplicationDbContext.Set<TEntity>() => base.Set<TEntity>();

    void IApplicationDbContext.Add<TEntity>(TEntity entity) => base.Add(entity);

    void IApplicationDbContext.Update<TEntity>(TEntity entity) => base.Update(entity);

    void IApplicationDbContext.Remove<TEntity>(TEntity entity) => base.Remove(entity);

    Task<bool> IApplicationDbContext.CanConnectAsync(CancellationToken cancellationToken) => base.Database.CanConnectAsync(cancellationToken);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Discover every Fluent-API configuration in this assembly so adding
        // a new entity + its configuration in F5 requires no edits to this
        // class (ADR-0011: one configuration per entity, registered here).
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
