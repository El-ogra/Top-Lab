using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;
using TopLab.Infrastructure.Persistence;

namespace TopLab.Persistence.Tests;

/// <summary>
/// S-07 Slice 12 (F-05/M-03): relational integration test fixture.
/// Uses Testcontainers.MsSql 3.10.0 — ephemeral container only.
/// NEVER connects to any database the application is configured to use.
/// Skips cleanly when Docker is unavailable (SD-12).
/// </summary>
public sealed class SqlServerFixture : IAsyncLifetime
{
    private MsSqlContainer? _container;

    public bool IsDockerAvailable { get; private set; }
    public string ConnectionString { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        try
        {
            _container = new MsSqlBuilder()
                .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
                .Build();

            await _container.StartAsync();
            ConnectionString = _container.GetConnectionString();
            IsDockerAvailable = true;
        }
        catch
        {
            // Docker is unavailable — skip cleanly (SD-12)
            IsDockerAvailable = false;
        }
    }

    public async Task DisposeAsync()
    {
        if (_container is not null)
        {
            await _container.DisposeAsync();
        }
    }

    public ApplicationDbContext CreateContext()
    {
        if (!IsDockerAvailable)
        {
            throw new InvalidOperationException("Docker is not available.");
        }

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;

        return new ApplicationDbContext(options);
    }
}
