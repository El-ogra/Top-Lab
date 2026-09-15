using Microsoft.EntityFrameworkCore;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.SentOutSamples;
using TopLab.Infrastructure.Persistence;
using TopLab.Infrastructure.Tests.Common;
using TopLab.Infrastructure.Tests.Common.Fakes;
using Xunit;

namespace TopLab.Infrastructure.Tests.Persistence;

public class SentOutSamplePersistenceTests
{
    private static readonly DateTime FixedNow = new(2026, 3, 15, 10, 0, 0, DateTimeKind.Utc);

    private static ApplicationDbContext BuildContext()
    {
        var options = InMemoryContextFactory.Create(
            new FakeCurrentUserService { UserId = 7 },
            new FakeDateTimeProvider { UtcNow = FixedNow });
        return new ApplicationDbContext(options);
    }

    private static SentOutSample NewSample()
    {
        return SentOutSample.Create(
            SentOutSampleId.Create(0),
            PatientTestId.Create(11),
            ExternalEntityId.Create(22),
            costPrice: 100m,
            patientPrice: 150m,
            FixedNow);
    }

    [Fact]
    public async Task StoreRetrieve_SamplePlusPayments_ReloadMatchesAndCalculatorAgrees()
    {
        var options = InMemoryContextFactory.Create(
            new FakeCurrentUserService { UserId = 7 },
            new FakeDateTimeProvider { UtcNow = FixedNow });
        using var ctx = new ApplicationDbContext(options);
        var sample = NewSample();
        ctx.Set<SentOutSample>().Add(sample);
        await ctx.SaveChangesAsync();

        ctx.Set<SentOutSamplePayment>().Add(SentOutSamplePayment.Create(
            SentOutSamplePaymentId.Create(1), sample.Id, 30m, FixedNow, performedByUserId: 7));
        await ctx.SaveChangesAsync();
        ctx.Set<SentOutSamplePayment>().Add(SentOutSamplePayment.Create(
            SentOutSamplePaymentId.Create(2), sample.Id, 20m, FixedNow, performedByUserId: 7));
        await ctx.SaveChangesAsync();

        using var reread = new ApplicationDbContext(options);
        var stored = await reread.Set<SentOutSample>().SingleAsync();
        Assert.Equal(100m, stored.CostPrice);
        Assert.Equal(150m, stored.PatientPrice);
        Assert.Equal(11, stored.PatientTestId.Value);
        Assert.Equal(22, stored.ExternalLabEntityId.Value);
        Assert.Equal(7, stored.CreatedByUserId);
        Assert.Equal(FixedNow, stored.CreatedAtUtc);

        var payments = await reread.Set<SentOutSamplePayment>()
            .Where(p => p.SentOutSampleId.Value == stored.Id.Value)
            .ToListAsync();
        Assert.Equal(2, payments.Count);
        Assert.Equal(50m, SentOutAccountCalculator.TotalPaid(payments));
        Assert.Equal(50m, SentOutAccountCalculator.Remaining(stored.CostPrice, 50m));
        Assert.False(SentOutAccountCalculator.IsFullySettled(stored.CostPrice, 50m));
    }

    [Fact]
    public async Task DeleteSample_CascadesToPayments()
    {
        using var ctx = BuildContext();
        var sample = NewSample();
        ctx.Set<SentOutSample>().Add(sample);
        await ctx.SaveChangesAsync();

        ctx.Set<SentOutSamplePayment>().Add(SentOutSamplePayment.Create(
            SentOutSamplePaymentId.Create(0), sample.Id, 40m, FixedNow, performedByUserId: 7));
        await ctx.SaveChangesAsync();

        ctx.Set<SentOutSample>().Remove(sample);
        await ctx.SaveChangesAsync();

        Assert.Empty(ctx.Set<SentOutSample>());
        Assert.Empty(ctx.Set<SentOutSamplePayment>());
    }

    [Fact]
    public void PaymentToSample_FK_IsCascade()
    {
        using var ctx = BuildContext();
        var fk = ctx.Model.FindEntityType(typeof(SentOutSamplePayment))!
            .GetForeignKeys()
            .Single(fk => fk.PrincipalEntityType.ClrType == typeof(SentOutSample));

        Assert.Equal("SentOutSampleId", Assert.Single(fk.Properties).Name);
        Assert.Equal(DeleteBehavior.Cascade, fk.DeleteBehavior);
    }

    [Fact]
    public void SampleToEntity_FK_IsRestrict()
    {
        using var ctx = BuildContext();
        var fk = ctx.Model.FindEntityType(typeof(SentOutSample))!
            .GetForeignKeys()
            .Single(fk => fk.PrincipalEntityType.ClrType == typeof(Domain.ExternalEntities.ExternalEntity));

        Assert.Equal("ExternalLabEntityId", Assert.Single(fk.Properties).Name);
        Assert.Equal(DeleteBehavior.Restrict, fk.DeleteBehavior);
    }
}
