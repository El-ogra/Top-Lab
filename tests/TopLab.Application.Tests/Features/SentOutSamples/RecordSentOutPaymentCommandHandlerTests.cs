using TopLab.Application.Features.SentOutSamples.Commands.RecordSentOutPayment;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.SentOutSamples;
using Xunit;

namespace TopLab.Application.Tests.Features.SentOutSamples;

public class RecordSentOutPaymentCommandHandlerTests
{
    private static (FakeApplicationDbContext db, FakeCurrentUserService user, FakeDateTimeProvider time) NewCtx()
        => (new FakeApplicationDbContext(), new FakeCurrentUserService(), new FakeDateTimeProvider());

    private static SentOutSample SeedSample(FakeApplicationDbContext db, int id, decimal cost = 100m)
    {
        var s = SentOutSample.Create(
            SentOutSampleId.Create(id),
            Domain.Common.Ids.PatientTestId.Create(id),
            Domain.Common.Ids.ExternalEntityId.Create(1),
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
    public async Task PartialPayment_Success_AuditFieldsWritten()
    {
        var (db, user, time) = NewCtx();
        user.UserId = 42;
        var sample = SeedSample(db, 1);

        var handler = new RecordSentOutPaymentCommandHandler(db, user, time);
        var result = await handler.Handle(new RecordSentOutPaymentCommand(1, 40m), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var stored = Assert.Single(db.SentOutSamplePayments);
        Assert.Equal(40m, stored.AmountPaid);
        Assert.Equal(42, stored.PerformedByUserId);
        Assert.Equal(time.UtcNow, stored.PaidAtUtc);
        Assert.Equal(sample.Id, stored.SentOutSampleId);
    }

    [Fact]
    public async Task OverRemaining_Rejected()
    {
        var (db, user, time) = NewCtx();
        SeedSample(db, 1, cost: 100m);

        var handler = new RecordSentOutPaymentCommandHandler(db, user, time);
        var result = await handler.Handle(new RecordSentOutPaymentCommand(1, 120m), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("مبلغ الدفع يتجاوز المتبقي.", result.Error!.Message);
        Assert.Empty(db.SentOutSamplePayments);
    }

    [Fact]
    public async Task RemainingRecomputed_FromPriorPayments()
    {
        var (db, user, time) = NewCtx();
        var sample = SeedSample(db, 1, cost: 100m);
        SeedPayment(db, sample, 60m);

        var handler = new RecordSentOutPaymentCommandHandler(db, user, time);

        var over = await handler.Handle(new RecordSentOutPaymentCommand(1, 50m), CancellationToken.None);
        Assert.False(over.IsSuccess);
        Assert.Equal("مبلغ الدفع يتجاوز المتبقي.", over.Error!.Message);

        var exact = await handler.Handle(new RecordSentOutPaymentCommand(1, 40m), CancellationToken.None);
        Assert.True(exact.IsSuccess);
        Assert.Equal(2, db.SentOutSamplePayments.Count);
    }

    [Fact]
    public async Task UnknownSample_ReturnsNotFound()
    {
        var (db, user, time) = NewCtx();

        var handler = new RecordSentOutPaymentCommandHandler(db, user, time);
        var result = await handler.Handle(new RecordSentOutPaymentCommand(9, 10m), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("العينة المُرسَلة غير موجودة.", result.Error!.Message);
    }

    [Fact]
    public async Task ZeroAmount_RejectedByDomain()
    {
        var (db, user, time) = NewCtx();
        SeedSample(db, 1);

        var handler = new RecordSentOutPaymentCommandHandler(db, user, time);
        var result = await handler.Handle(new RecordSentOutPaymentCommand(1, 0m), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("مبلغ الدفع يجب أن يكون أكبر من صفر.", result.Error!.Message);
    }

    [Fact]
    public void Validator_Rules()
    {
        var validator = new RecordSentOutPaymentCommandValidator();

        Assert.False(validator.Validate(new RecordSentOutPaymentCommand(0, 10m)).IsValid);

        var amount = validator.Validate(new RecordSentOutPaymentCommand(1, 0m));
        Assert.False(amount.IsValid);
        Assert.Equal("مبلغ الدفع يجب أن يكون أكبر من صفر.", Assert.Single(amount.Errors).ErrorMessage);

        Assert.True(validator.Validate(new RecordSentOutPaymentCommand(1, 10m)).IsValid);
    }
}
