using Microsoft.EntityFrameworkCore;
using TopLab.Application.Features.AccessAndNavigation.Queries.CheckDatabaseConnectivity;
using TopLab.Infrastructure.Persistence;
using Xunit;

namespace TopLab.Application.Tests.Features.AccessAndNavigation;

public class CheckDatabaseConnectivityQueryHandlerTests
{
    [Fact]
    public async Task Handle_InMemory_ShouldReturnSuccess()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        using var real = new ApplicationDbContext(options);
        var handler = new CheckDatabaseConnectivityQueryHandler(real);
        var result = await handler.Handle(new CheckDatabaseConnectivityQuery(), CancellationToken.None);
        Assert.True(result.IsSuccess);
    }
}
