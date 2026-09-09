using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using TopLab.Application.Features.PatientRegistration.Commands.AddProfileToVisit;
using TopLab.Application.Features.ProfileResults.Commands.AmendProfileResult;
using TopLab.Application.Features.ProfileResults.Queries.GetProfileReport;
using TopLab.Domain.Billing;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;
using TopLab.Infrastructure.Persistence;
using TopLab.Infrastructure.Persistence.Interceptors;
using TopLab.Infrastructure.Tests.Common;
using TopLab.Infrastructure.Tests.Common.Fakes;
using Xunit;

namespace TopLab.Infrastructure.Tests.Persistence;

/// <summary>
/// M-05 relational proofs on the real <see cref="ApplicationDbContext"/> over the
/// InMemory provider: the central-calculator charge stored by the profile order
/// handler, the atomic amendment+audit rollback (effectively a single SaveChanges),
/// and the reprint surface reading the frozen snapshot across contexts when the
/// live analyte range changes between saves.
/// </summary>
public class ProfileDomainPersistenceTests
{
    private static DbContextOptions<ApplicationDbContext> CreateOptions(string dbName)
    {
        return new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(dbName)
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .AddInterceptors(new AuditableEntitySaveChangesInterceptor(
                new FakeCurrentUserService { UserId = 7 },
                new FakeDateTimeProvider { UtcNow = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc) }))
            .Options;
    }

    [Fact]
    public async Task ProfileOrder_StoresCentralCalculatorCharge_ThroughRealContext()
    {
        var options = InMemoryContextFactory.Create();
        await using var ctx = new ApplicationDbContext(options);
        ctx.Database.EnsureCreated();

        ctx.Patients.Add(Patient.Create(PatientId.Create(1), "P1", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow));
        ctx.Tests.Add(Test.Create(TestId.Create(10), "Pro10", "Pro10", "Pro10", "T10", 60, 500m, ResultKind.SpecializedProfile));
        ctx.Profiles.Add(Profile.Create(ProfileId.Create(1), "Pro", TestId.Create(10), ResultKind.SpecializedProfile, 300m));
        await ctx.SaveChangesAsync();

        var handler = new AddProfileToVisitCommandHandler(ctx);
        var result = await handler.Handle(
            new AddProfileToVisitCommand(PatientId: 1, ProfileId: 1),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        var stored = await ctx.PatientTests.AsNoTracking().SingleAsync();
        Assert.Equal(PatientAccountCalculator.ProfileSelectionCharge(300m), stored.PriceAtOrderTime);
        Assert.Equal(300m, stored.PriceAtOrderTime);
    }

    [Fact]
    public async Task AmendAndAudit_AtomicRollback_WhenSaveFails()
    {
        var dbName = $"TopLab-ProfileRollback-{Guid.NewGuid()}";

        async Task Seed()
        {
            await using var seed = new ApplicationDbContext(CreateOptions(dbName));
            seed.Database.EnsureCreated();
            seed.Patients.Add(Patient.Create(PatientId.Create(1), "P1", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow));
            seed.Tests.Add(Test.Create(TestId.Create(10), "Pro10", "Pro10", "Pro10", "T10", 60, 500m, ResultKind.SpecializedProfile));
            seed.Profiles.Add(Profile.Create(ProfileId.Create(1), "Pro", TestId.Create(10), ResultKind.SpecializedProfile, 300m));
            var pt = PatientTest.Create(PatientTestId.Create(0), PatientId.Create(1), TestId.Create(10), 300m);
            seed.PatientTests.Add(pt);
            await seed.SaveChangesAsync();
            var tracked = await seed.PatientTests.SingleAsync();
            seed.ProfileResultItems.Add(ProfileResultItem.Create(
                ProfileResultItemId.Create(0),
                tracked.Id,
                AnalyteId.Create(5),
                "4",
                "mg",
                ProfileResultFlag.Low,
                isVerified: true,
                isPrinted: true));
            await seed.SaveChangesAsync();
        }

        await Seed();

        var failingOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(dbName)
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .AddInterceptors(new AuditableEntitySaveChangesInterceptor(
                new FakeCurrentUserService { UserId = 7 },
                new FakeDateTimeProvider { UtcNow = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc) }))
            .AddInterceptors(new ThrowingSaveChangesInterceptor())
            .Options;

        var threw = false;
        await using (var amendCtx = new ApplicationDbContext(failingOptions))
        {
            var itemId = amendCtx.ProfileResultItems.AsNoTracking().Single().Id.Value;
            var handler = new AmendProfileResultCommandHandler(
                amendCtx,
                new FakeCurrentUserService { UserId = 7 },
                new FakeDateTimeProvider { UtcNow = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc) });
            try
            {
                await handler.Handle(
                    new AmendProfileResultCommand(itemId, "9", "mg", (int)ProfileResultFlag.High, "re-check"),
                    CancellationToken.None);
            }
            catch (InvalidOperationException)
            {
                threw = true;
            }
        }
        Assert.True(threw, "save must fail on the forced interceptor");

        await using (var verify = new ApplicationDbContext(CreateOptions(dbName)))
        {
            var row = await verify.ProfileResultItems.AsNoTracking().SingleAsync();
            Assert.Equal("4", row.ResultValue);
            Assert.Equal(ProfileResultFlag.Low, row.Flag);
            Assert.Equal(0, row.PrintCount);
            Assert.Empty(verify.ProfileResultAmendments);
        }
    }

    [Fact]
    public async Task Report_UsesFrozenRanges_AcrossContexts_AfterLiveRangeChange()
    {
        var dbName = $"TopLab-ProfileReprint-{Guid.NewGuid()}";

        await using (var seed = new ApplicationDbContext(CreateOptions(dbName)))
        {
            seed.Database.EnsureCreated();
            seed.Patients.Add(Patient.Create(PatientId.Create(1), "P1", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow));
            seed.Tests.Add(Test.Create(TestId.Create(10), "Pro10", "Pro10", "Pro10", "T10", 60, 500m, ResultKind.SpecializedProfile));
            seed.Profiles.Add(Profile.Create(ProfileId.Create(1), "Pro", TestId.Create(10), ResultKind.SpecializedProfile, 300m));
            var (analyte, range, band) = SeedAnalyte(seed, 5, 1m, 5m);
            _ = (analyte, range);

            var pt = PatientTest.Create(PatientTestId.Create(0), PatientId.Create(1), TestId.Create(10), 300m);
            seed.PatientTests.Add(pt);
            await seed.SaveChangesAsync();
            var tracked = await seed.PatientTests.SingleAsync();
            var item = ProfileResultItem.Create(
                ProfileResultItemId.Create(0),
                tracked.Id,
                analyte.Id,
                "3",
                "mg",
                ProfileResultFlag.Low,
                isVerified: true,
                isPrinted: true);
            seed.ProfileResultItems.Add(item);
            seed.ProfileResultItemReferenceRangeSnapshots.Add(ProfileResultItemReferenceRangeSnapshot.Create(
                item.Id,
                analyte.Id,
                Sex.Male,
                AgeUnit.Year,
                0,
                100,
                1m,
                5m,
                null,
                null,
                DateTimeOffset.UtcNow));
            await seed.SaveChangesAsync();
        }

        await using (var changer = new ApplicationDbContext(CreateOptions(dbName)))
        {
            var band = await changer.AnalyteReferenceRangeBands.SingleAsync();
            changer.AnalyteReferenceRangeBands.Remove(band);
            changer.AnalyteReferenceRangeBands.Add(AnalyteReferenceRangeBand.Create(
                AnalyteReferenceRangeBandId.Create(42),
                band.AnalyteReferenceRangeId,
                AgeUnit.Year,
                0,
                100,
                8m,
                12m,
                null));
            await changer.SaveChangesAsync();
        }

        await using (var reader = new ApplicationDbContext(CreateOptions(dbName)))
        {
            var ptId = (await reader.PatientTests.AsNoTracking().SingleAsync()).Id.Value;
            var handler = new GetProfileReportQueryHandler(reader);
            var result = await handler.Handle(new GetProfileReportQuery(PatientTestId: ptId), CancellationToken.None);

            Assert.True(result.IsSuccess, result.Error?.Message);
            var line = Assert.Single(result.Value!.Lines);
            Assert.Equal("3", line.ResultValue);
            Assert.NotNull(line.FrozenRange);
            Assert.Equal(1m, line.FrozenRange!.MinValue);
            Assert.Equal(5m, line.FrozenRange.MaxValue);
        }
    }

    private static (Analyte Analyte, AnalyteReferenceRange Range, AnalyteReferenceRangeBand Band) SeedAnalyte(
        ApplicationDbContext db,
        int analyteId,
        decimal min,
        decimal max)
    {
        var analyte = Analyte.Create(AnalyteId.Create(analyteId), $"A{analyteId}", $"Report A{analyteId}");
        var range = AnalyteReferenceRange.Create(AnalyteReferenceRangeId.Create(analyteId + 10000), analyte.Id);
        var band = AnalyteReferenceRangeBand.Create(
            AnalyteReferenceRangeBandId.Create(analyteId + 20000),
            range.Id,
            AgeUnit.Year,
            0,
            100,
            min,
            max,
            null);
        db.Analytes.Add(analyte);
        db.AnalyteReferenceRanges.Add(range);
        db.AnalyteReferenceRangeBands.Add(band);
        return (analyte, range, band);
    }

    private sealed class ThrowingSaveChangesInterceptor : ISaveChangesInterceptor
    {
        public InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            throw new InvalidOperationException("forced save failure");
        }

        public ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("forced save failure");
        }
    }
}