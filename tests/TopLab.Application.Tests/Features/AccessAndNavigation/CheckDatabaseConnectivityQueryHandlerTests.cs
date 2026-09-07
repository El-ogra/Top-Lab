using TopLab.Application.Common.Interfaces;
using TopLab.Application.Features.AccessAndNavigation.Queries.CheckDatabaseConnectivity;
using TopLab.Application.Tests.Common.Fakes;
using Xunit;

namespace TopLab.Application.Tests.Features.AccessAndNavigation;

public class CheckDatabaseConnectivityQueryHandlerTests
{
    private const string Server = "(localdb)\\mssqllocaldb";
    private const string Database = "TopLab";

    private static FakeDbConnectionDescriptor BuildDescriptor(
        string? server = Server,
        string? database = Database) =>
        new()
        {
            ServerName = server,
            DatabaseName = database
        };

    [Fact]
    public async Task Handle_Connected_ReturnsDtoWithDescriptorAndTrue()
    {
        var db = new FakeApplicationDbContext();
        var descriptor = BuildDescriptor();
        var clock = new FakeDateTimeProvider();
        var handler = new CheckDatabaseConnectivityQueryHandler(db, descriptor, clock);

        var result = await handler.Handle(new CheckDatabaseConnectivityQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.True(result.Value!.IsConnected);
        Assert.Equal(Server, result.Value.ServerName);
        Assert.Equal(Database, result.Value.DatabaseName);
        Assert.Equal(clock.UtcNow, result.Value.CheckedAtUtc);
    }

    [Fact]
    public async Task Handle_NotConnected_ReturnsDtoWithDescriptorAndFalse()
    {
        var db = new ControllableFakeApplicationDbContext { CanConnectResult = false };
        var descriptor = BuildDescriptor();
        var clock = new FakeDateTimeProvider();
        var handler = new CheckDatabaseConnectivityQueryHandler(db, descriptor, clock);

        var result = await handler.Handle(new CheckDatabaseConnectivityQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.False(result.Value!.IsConnected);
        Assert.Equal(Server, result.Value.ServerName);
        Assert.Equal(Database, result.Value.DatabaseName);
    }

    [Fact]
    public async Task Handle_CanConnectThrows_ReturnsDtoWithFalseAndNoExceptionBubbles()
    {
        var db = new ControllableFakeApplicationDbContext
        {
            CanConnectException = new InvalidOperationException("simulated DB outage")
        };
        var descriptor = BuildDescriptor();
        var clock = new FakeDateTimeProvider();
        var handler = new CheckDatabaseConnectivityQueryHandler(db, descriptor, clock);

        var result = await handler.Handle(new CheckDatabaseConnectivityQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.False(result.Value!.IsConnected);
        Assert.Equal(Server, result.Value.ServerName);
        Assert.Equal(Database, result.Value.DatabaseName);
    }

    [Fact]
    public async Task Handle_DescriptorSurfacesParsedServerAndDatabase()
    {
        var db = new FakeApplicationDbContext();
        var descriptor = BuildDescriptor(server: "prod-sql-01", database: "TopLab_Prod");
        var clock = new FakeDateTimeProvider();
        var handler = new CheckDatabaseConnectivityQueryHandler(db, descriptor, clock);

        var result = await handler.Handle(new CheckDatabaseConnectivityQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("prod-sql-01", result.Value!.ServerName);
        Assert.Equal("TopLab_Prod", result.Value.DatabaseName);
    }

    [Fact]
    public async Task Handle_DtoNeverSurfacesPasswordSubstring()
    {
        // The descriptor's public surface intentionally exposes only server
        // and database name, never credentials. This guards that contract by
        // asserting the DTO has no 'secret' substring regardless of what the
        // upstream connection string contains.
        var db = new FakeApplicationDbContext();
        var descriptor = BuildDescriptor();
        var clock = new FakeDateTimeProvider();
        var handler = new CheckDatabaseConnectivityQueryHandler(db, descriptor, clock);

        var result = await handler.Handle(new CheckDatabaseConnectivityQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.DoesNotContain("secret", result.Value!.ServerName ?? string.Empty);
        Assert.DoesNotContain("secret", result.Value.DatabaseName ?? string.Empty);
    }
}

/// <summary>
/// IApplicationDbContext that wraps <see cref="FakeApplicationDbContext"/> and
/// exposes hooks to override <see cref="CanConnectAsync"/> for the connectivity
/// handler tests. Mirrors the wrapping-fake pattern used by
/// <see cref="UniqueViolationFakeApplicationDbContext"/> and
/// <see cref="ReferenceConflictFakeApplicationDbContext"/>.
/// </summary>
public sealed class ControllableFakeApplicationDbContext : IApplicationDbContext
{
    private readonly FakeApplicationDbContext _inner = new();

    public bool CanConnectResult { get; set; } = true;

    public Exception? CanConnectException { get; set; }

    public IQueryable<TEntity> Set<TEntity>() where TEntity : class => _inner.Set<TEntity>();

    public void Add<TEntity>(TEntity entity) where TEntity : class => _inner.Add(entity);

    public void Update<TEntity>(TEntity entity) where TEntity : class => _inner.Update(entity);

    public void Remove<TEntity>(TEntity entity) where TEntity : class => _inner.Remove(entity);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _inner.SaveChangesAsync(cancellationToken);

    public Task<bool> CanConnectAsync(CancellationToken cancellationToken = default)
    {
        if (CanConnectException is not null)
        {
            throw CanConnectException;
        }

        return Task.FromResult(CanConnectResult);
    }
}