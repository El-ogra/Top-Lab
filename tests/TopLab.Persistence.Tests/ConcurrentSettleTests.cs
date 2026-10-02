using Microsoft.EntityFrameworkCore;
using TopLab.Application.Common.Interfaces;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientBilling.Commands.SettleAccountInFull;
using TopLab.Domain.Billing;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;
using TopLab.Infrastructure.Persistence;

namespace TopLab.Persistence.Tests;

/// <summary>W-02 S14 (WP-29): settlement serialisation on a real engine.
/// Docker-gated: honest skip when unavailable (S-07 SD-12 pattern).</summary>
public class ConcurrentSettleTests : IClassFixture<SqlServerFixture>
{
    private readonly SqlServerFixture _fixture;

    public ConcurrentSettleTests(SqlServerFixture fixture)
    {
        _fixture = fixture;
    }

    private sealed class StubUser : ICurrentUserService
    {
        public bool IsAuthenticated => true;
        public int UserId => 7;
        public string UserName => "cashier";
        public bool IsAbsolutePermission => false;
        public bool HasPermission(string code) => false;
        public void SetSession(int userId, string userName, bool isAbsolutePermission, IEnumerable<string> grantedPermissions) { }
        public void ClearSession() { }
    }

    private sealed class StubClock : IDateTimeProvider
    {
        public DateTime UtcNow => new(2026, 5, 1, 12, 0, 0, DateTimeKind.Utc);
    }

    [DockerFact]
    public async Task Settle_ConcurrentCalls_InsertOnePayment()
    {
        if (!_fixture.IsDockerAvailable)
        {
            throw global::Xunit.Sdk.SkipException.ForSkip("Docker container unavailable — live settlement race skipped.");
        }

        await using (var seed = _fixture.CreateContext())
        {
            await seed.Database.MigrateAsync();
            seed.Patients.Add(Patient.Create(PatientId.Create(1), "P", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow));
            seed.Tests.Add(Test.Create(TestId.Create(10), "T", "T", "T", "T10", 1, 100m, ResultKind.Simple));
            seed.PatientTests.Add(PatientTest.Create(PatientTestId.Create(100), PatientId.Create(1), TestId.Create(10), 100m));
            await seed.SaveChangesAsync();
        }

        async Task<Result<int>> SettleOnceAsync()
        {
            await using var ctx = _fixture.CreateContext();
            var uow = new AppUnitOfWork(ctx);
            return await new SettleAccountInFullCommandHandler(
                ctx, new StubUser(), new StubClock(), uow).Handle(
                new SettleAccountInFullCommand(1), CancellationToken.None);
        }

        var results = await Task.WhenAll(SettleOnceAsync(), SettleOnceAsync());

        Assert.Equal(1, results.Count(r => r.IsSuccess));
        Assert.Equal(1, results.Count(r => !r.IsSuccess));

        await using var verify = _fixture.CreateContext();
        Assert.Equal(1, await verify.Set<PaymentOperation>().CountAsync());
    }
}
