using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using TopLab.Application.Common.Interfaces;
using TopLab.Infrastructure.Persistence;
using TopLab.Infrastructure.Persistence.Interceptors;
using TopLab.Infrastructure.Tests.Common.Fakes;

namespace TopLab.Infrastructure.Tests.Common;

/// <summary>
/// Builds an <see cref="DbContextOptions{TContext}"/> wired with the
/// InMemory provider so interceptor and configuration tests can exercise
/// persistence behaviour without a real SQL Server instance.
/// </summary>
public static class InMemoryContextFactory
{
    public static DbContextOptions<ApplicationDbContext> Create(
        ICurrentUserService? currentUser = null,
        IDateTimeProvider? dateTime = null)
    {
        currentUser ??= new FakeCurrentUserService { UserId = 0 };
        dateTime ??= new FakeDateTimeProvider { UtcNow = DateTime.UtcNow };

        var interceptor = new AuditableEntitySaveChangesInterceptor(currentUser, dateTime);

        return new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"TopLab-F4-{Guid.NewGuid()}")
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .AddInterceptors(interceptor)
            .Options;
    }
}
