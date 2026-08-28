using Microsoft.EntityFrameworkCore;
using TopLab.Application.Common.Interfaces;
using TopLab.Infrastructure.Persistence;
using TopLab.Infrastructure.Tests.Common;
using TopLab.Infrastructure.Tests.Common.Fakes;

namespace TopLab.Infrastructure.Tests.Persistence;

public class ApplicationDbContextTests
{
    [Fact]
    public void DbContext_ImplementsIApplicationDbContext()
    {
        var ctx = new ApplicationDbContext(InMemoryContextFactory.Create());

        Assert.IsAssignableFrom<IApplicationDbContext>(ctx);
    }

    [Fact]
    public async Task IApplicationDbContext_AddAndSaveChanges_WorksThroughInterface()
    {
        IApplicationDbContext ctx = new TestApplicationDbContext(InMemoryContextFactory.Create());
        var entry = ctx.Add(new AuditableTestEntity(10)
        {
            Name = "via-interface",
        });

        Assert.Equal(EntityState.Added, entry.State);

        var written = await ctx.SaveChangesAsync();
        Assert.Equal(1, written);
    }
}
