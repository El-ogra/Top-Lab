using Microsoft.EntityFrameworkCore;
using TopLab.Infrastructure.Identity;

namespace TopLab.Infrastructure.Tests.Identity;

public class SystemDateTimeProviderTests
{
    [Fact]
    public void UtcNow_ReturnsCurrentUtcTime()
    {
        var provider = new SystemDateTimeProvider();

        var before = DateTime.UtcNow;
        var actual = provider.UtcNow;
        var after = DateTime.UtcNow;

        Assert.InRange(actual, before.AddSeconds(-1), after.AddSeconds(1));
        Assert.Equal(DateTimeKind.Utc, actual.Kind);
    }
}
