using Microsoft.EntityFrameworkCore;
using TopLab.Infrastructure.Persistence;

namespace TopLab.Persistence.Tests;

/// <summary>
/// S-07 Slice 12 (F-05/M-03): never-connect static assertions.
/// The connection string comes ONLY from the ephemeral container.
/// Never reads IConfiguration, appsettings.json, environment variables,
/// or IWorkstationConnectionSettingsProvider.
/// </summary>
public class NeverConnectGuardTests
{
    [Fact]
    public void NoLocalDb_ConnectionString_InPersistenceTests()
    {
        var srcDir = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "tests", "TopLab.Persistence.Tests");
        var files = Directory.GetFiles(srcDir, "*.cs", SearchOption.AllDirectories).Where(f => !f.Contains("NeverConnectGuardTests") && !f.Contains("SqlServerFixture")).ToList();

        foreach (var file in files)
        {
            var content = File.ReadAllText(file);
            Assert.DoesNotContain("localdb", content, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void NoConfigurationBuilder_InPersistenceTests()
    {
        var srcDir = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "tests", "TopLab.Persistence.Tests");
        var files = Directory.GetFiles(srcDir, "*.cs", SearchOption.AllDirectories).Where(f => !f.Contains("NeverConnectGuardTests") && !f.Contains("SqlServerFixture")).ToList();

        foreach (var file in files)
        {
            var content = File.ReadAllText(file);
            Assert.DoesNotContain("ConfigurationBuilder", content);
        }
    }

    [Fact]
    public void NoGetConnectionString_InPersistenceTests()
    {
        var srcDir = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "tests", "TopLab.Persistence.Tests");
        var files = Directory.GetFiles(srcDir, "*.cs", SearchOption.AllDirectories).Where(f => !f.Contains("NeverConnectGuardTests") && !f.Contains("SqlServerFixture")).ToList();

        foreach (var file in files)
        {
            var content = File.ReadAllText(file);
            Assert.DoesNotContain("GetConnectionString(", content);
        }
    }
}
