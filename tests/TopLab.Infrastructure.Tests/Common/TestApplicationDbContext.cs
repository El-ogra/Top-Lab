using Microsoft.EntityFrameworkCore;
using TopLab.Infrastructure.Persistence;
using TopLab.Infrastructure.Tests.Common.Fakes;

namespace TopLab.Infrastructure.Tests.Common;

/// <summary>
/// In-memory test <see cref="Microsoft.EntityFrameworkCore.DbContext"/> exposing
/// the test entity types as <c>DbSet</c> properties so EF Core will register
/// them in the model. The interceptor tests use this context to exercise
/// audit-column population without requiring a real SQL Server instance.
/// </summary>
public sealed class TestApplicationDbContext : ApplicationDbContext
{
    public TestApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<AuditableTestEntity> AuditableEntities => Set<AuditableTestEntity>();

    public DbSet<NonAuditableTestEntity> NonAuditableEntities => Set<NonAuditableTestEntity>();
}
