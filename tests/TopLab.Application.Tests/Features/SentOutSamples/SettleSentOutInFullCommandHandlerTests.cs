using TopLab.Application.Features.SentOutSamples.Commands.SettleSentOutInFull;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.SentOutSamples;
using Xunit;

namespace TopLab.Application.Tests.Features.SentOutSamples;

public class SettleSentOutInFullCommandHandlerTests
{
    private static (FakeApplicationDbContext db, FakeCurrentUserService user, FakeDateTimeProvider time) NewCtx()
        => (new FakeApplicationDbContext(), new FakeCurrentUserService(), new FakeDateTimeProvider());

    private static SentOutSample SeedSample(FakeApplicationDbContext db, int id, decimal cost = 100m)
    {
        var s = SentOutSample.Create(
            SentOutSampleId.Create(id),
            PatientTestId.Create(id),
            ExternalEntityId.Create(1),
            cost,
            patientPrice: 150m,
            DateTime.UtcNow);
        db.SentOutSamples.Add(s);
        return s;
    }

    private static void SeedPayment(FakeApplicationDbContext db, SentOutSample sample, decimal amount)
    {
        db.SentOutSamplePayments.Add(SentOutSamplePayment.Create(
            SentOutSamplePaymentId.Create(db.SentOutSamplePayments.Count + 1),
            sample.Id,
            amount,
            DateTime.UtcNow,
            performedByUserId: 1));
    }

    [Fact]
    public async Task SettlesExactlyTheRemaining_AuditFieldsWritten()
    {
        var (db, user, time) = NewCtx();
        user.UserId = 7;
        var sample = SeedSample(db, 1, cost: 100m);
        SeedPayment(db, sample, 30m);

        var handler = new SettleSentOutInFullCommandHandler(db, user, time);
        var result = await handler.Handle(new SettleSentOutInFullCommand(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, db.SentOutSamplePayments.Count);
        var settlement = db.SentOutSamplePayments.Last();
        Assert.Equal(70m, settlement.AmountPaid);
        Assert.Equal(7, settlement.PerformedByUserId);
        Assert.Equal(time.UtcNow, settlement.PaidAtUtc);
        Assert.Equal(100m, db.SentOutSamplePayments.Sum(p => p.AmountPaid));
    }

    [Fact]
    public async Task NothingDue_ReturnsConflict()
    {
        var (db, user, time) = NewCtx();
        var sample = SeedSample(db, 1, cost: 100m);
        SeedPayment(db, sample, 100m);

        var handler = new SettleSentOutInFullCommandHandler(db, user, time);
        var result = await handler.Handle(new SettleSentOutInFullCommand(1), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("لا يوجد رصيد مستحق للتسوية.", result.Error!.Message);
        Assert.Single(db.SentOutSamplePayments);
    }

    [Fact]
    public async Task UnknownSample_ReturnsNotFound()
    {
        var (db, user, time) = NewCtx();

        var handler = new SettleSentOutInFullCommandHandler(db, user, time);
        var result = await handler.Handle(new SettleSentOutInFullCommand(9), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("العينة المُرسَلة غير موجودة.", result.Error!.Message);
    }

    [Fact]
    public void Validator_ZeroId_Rejected()
    {
        var validator = new SettleSentOutInFullCommandValidator();

        Assert.False(validator.Validate(new SettleSentOutInFullCommand(0)).IsValid);
        Assert.True(validator.Validate(new SettleSentOutInFullCommand(1)).IsValid);
    }
}
