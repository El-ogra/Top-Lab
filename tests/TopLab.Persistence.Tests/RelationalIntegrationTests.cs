using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using TopLab.Infrastructure.Persistence;

namespace TopLab.Persistence.Tests;

/// <summary>
/// Discovery-time Docker gate: sets <see cref="FactAttribute.Skip"/> when the Docker
/// engine is unavailable so xunit/VSTest reports Skipped (not Passed, not Failed).
/// Runtime <see cref="SqlServerFixture.IsDockerAvailable"/> remains the live authority.
/// </summary>
public sealed class DockerFactAttribute : FactAttribute
{
    public DockerFactAttribute()
    {
        if (!DockerProbe.IsAvailable)
        {
            Skip = "Docker unavailable (SD-12) — relational integration test skipped.";
        }
    }
}

internal static class DockerProbe
{
    private static readonly Lazy<bool> Available = new(Probe, isThreadSafe: true);

    public static bool IsAvailable => Available.Value;

    private static bool Probe()
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "docker",
                Arguments = "info",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var process = Process.Start(psi);
            if (process is null)
            {
                return false;
            }
            process.WaitForExit(5000);
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }
}

/// <summary>
/// S-07 Slice 12 (F-05/M-03): relational integration tests.
/// Model-level asserts use EF metadata and do NOT require Docker.
/// Live migrate/connect runs only when the ephemeral container is available.
/// </summary>
public class RelationalIntegrationTests : IClassFixture<SqlServerFixture>
{
    private readonly SqlServerFixture _fixture;

    public RelationalIntegrationTests(SqlServerFixture fixture)
    {
        _fixture = fixture;
    }

    /// <summary>Design-time model context — builds the EF model without opening a SQL connection.</summary>
    private static ApplicationDbContext CreateModelOnlyContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer("Server=.;Database=TopLab_RelationalModelOnly;Integrated Security=true;TrustServerCertificate=true;Connect Timeout=1")
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public void MigrationChain_ModelContainsExpectedEntitiesAndTables()
    {
        using var context = CreateModelOnlyContext();
        var model = context.Model;

        foreach (var clr in new[]
                 {
                     typeof(Domain.Users.User),
                     typeof(Domain.Billing.PaymentOperation),
                     typeof(Domain.Results.PatientTest),
                     typeof(Domain.Patients.Patient)
                 })
        {
            var entity = model.FindEntityType(clr);
            Assert.NotNull(entity);
            Assert.False(string.IsNullOrWhiteSpace(entity!.GetTableName()),
                $"Entity {clr.Name} has no table name in the model.");
        }
    }

    [DockerFact]
    public async Task MigrationChain_AppliesToEmptyDatabase()
    {
        if (!_fixture.IsDockerAvailable)
        {
            // Defence in depth: discovery gate passed but the container failed to start.
            throw global::Xunit.Sdk.SkipException.ForSkip("Docker container unavailable (SD-12) — live migration apply skipped.");
        }

        await using var context = _fixture.CreateContext();
        await context.Database.MigrateAsync();

        Assert.True(await context.Database.CanConnectAsync());

        var model = context.Model;
        Assert.NotNull(model.FindEntityType(typeof(Domain.Users.User)));
        Assert.NotNull(model.FindEntityType(typeof(Domain.Billing.PaymentOperation)));
        Assert.NotNull(model.FindEntityType(typeof(Domain.Results.PatientTest)));
        Assert.NotNull(model.FindEntityType(typeof(Domain.Patients.Patient)));
    }

    [Fact]
    public void UniqueIndex_UserName_IsEnforced()
    {
        using var context = CreateModelOnlyContext();
        var model = context.Model;

        var userEntity = model.FindEntityType(typeof(Domain.Users.User));
        Assert.NotNull(userEntity);

        var userNameIndex = userEntity!.GetIndexes()
            .FirstOrDefault(i => i.Properties.Any(p => p.Name == "UserName"));
        Assert.NotNull(userNameIndex);
        Assert.True(userNameIndex!.IsUnique);
    }

    [Fact]
    public void DecimalPrecision_IsCorrect()
    {
        using var context = CreateModelOnlyContext();
        var model = context.Model;

        var paymentEntity = model.FindEntityType(typeof(Domain.Billing.PaymentOperation));
        Assert.NotNull(paymentEntity);

        var amountProperty = paymentEntity!.FindProperty("Amount");
        Assert.NotNull(amountProperty);

        // Real precision/scale contract (not merely NotNull).
        // Current configuration: PaymentOperation.Amount is decimal(18,2).
        Assert.Equal(18, amountProperty!.GetPrecision());
        Assert.Equal(2, amountProperty.GetScale());
    }

    [Fact]
    public void CascadeBehaviour_IsCorrect()
    {
        using var context = CreateModelOnlyContext();
        var model = context.Model;

        var patientTestEntity = model.FindEntityType(typeof(Domain.Results.PatientTest));
        Assert.NotNull(patientTestEntity);

        var fk = patientTestEntity!.GetForeignKeys()
            .FirstOrDefault(f => f.PrincipalEntityType.ClrType == typeof(Domain.Patients.Patient));
        Assert.NotNull(fk);
        Assert.Equal(DeleteBehavior.Cascade, fk!.DeleteBehavior);
    }
}