using TopLab.Application.Features.InventoryAndAccounting.Commands.RecordCashDeposit;
using TopLab.Application.Features.InventoryAndAccounting.Commands.RecordCashDisbursement;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.ExternalEntities;
using Xunit;

namespace TopLab.Application.Tests.Features.InventoryAndAccounting;

public class RecordCashDepositCommandHandlerTests
{
    private static readonly DateTime Now = new(2026, 3, 15, 12, 0, 0, DateTimeKind.Utc);

    private static (RecordCashDepositCommandHandler Handler, FakeApplicationDbContext Db, FakeDateTimeProvider Clock)
        Handler(int userId = 7)
    {
        var db = new FakeApplicationDbContext();
        var clock = new FakeDateTimeProvider { UtcNow = Now };
        var user = new FakeCurrentUserService { UserId = userId, UserName = "cashier" };
        return (new RecordCashDepositCommandHandler(db, user, clock), db, clock);
    }

    [Fact]
    public async Task HappyPath_PersistsRow_WithStampedFields()
    {
        var (handler, db, _) = Handler(userId: 7);
        db.ExternalEntities.Add(ExternalEntity.Create(
            ExternalEntityId.Create(10),
            EntityType.TreatingDoctor,
            "جهة"));

        var result = await handler.Handle(
            new RecordCashDepositCommand(250m, 10, "إيداع"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        var row = db.CashMovements.Single();
        Assert.Equal(MovementType.Deposit, row.MovementType);
        Assert.Equal(250m, row.Amount);
        Assert.Equal(10, row.RelatedExternalEntityId!.Value);
        Assert.Equal(7, row.PerformedByUserId);
        Assert.Equal(Now, row.OccurredAtUtc);
        Assert.Equal("إيداع", row.Notes);
        Assert.Equal(1, db.SaveChangesCallCount);
    }

    [Fact]
    public async Task UnknownEntity_ReturnsNotFound_AndDoesNotPersist()
    {
        var (handler, db, _) = Handler();

        var result = await handler.Handle(
            new RecordCashDepositCommand(10m, 999, null),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("الجهة الخارجية غير موجودة.", result.Error!.Message);
        Assert.Empty(db.CashMovements);
        Assert.Equal(0, db.SaveChangesCallCount);
    }

    [Fact]
    public void Validator_RejectsNonPositiveAmount_WithFrozenMessage()
    {
        var validator = new RecordCashDepositCommandValidator();
        var outcome = validator.Validate(new RecordCashDepositCommand(0m, null, null));
        Assert.False(outcome.IsValid);
        Assert.Contains(outcome.Errors, e => e.ErrorMessage == "المبلغ يجب أن يكون أكبر من صفر.");
    }

    [Fact]
    public void Validator_RejectsOverlongNotes()
    {
        var validator = new RecordCashDepositCommandValidator();
        var outcome = validator.Validate(new RecordCashDepositCommand(1m, null, new string('n', 501)));
        Assert.False(outcome.IsValid);
    }
}

public class RecordCashDisbursementCommandHandlerTests
{
    private static readonly DateTime Now = new(2026, 3, 15, 12, 0, 0, DateTimeKind.Utc);

    private static RecordCashDisbursementCommandHandler Handler(FakeApplicationDbContext db, int userId = 4)
    {
        var clock = new FakeDateTimeProvider { UtcNow = Now };
        var user = new FakeCurrentUserService { UserId = userId };
        return new RecordCashDisbursementCommandHandler(db, user, clock);
    }

    [Fact]
    public async Task HappyPath_PersistsDisbursementRow()
    {
        var db = new FakeApplicationDbContext();
        var result = await Handler(db).Handle(
            new RecordCashDisbursementCommand(40m, null, "مصروف"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        var row = db.CashMovements.Single();
        Assert.Equal(MovementType.Disbursement, row.MovementType);
        Assert.Equal(40m, row.Amount);
        Assert.Equal(4, row.PerformedByUserId);
        Assert.Equal(Now, row.OccurredAtUtc);
    }

    [Fact]
    public async Task UnknownEntity_ReturnsNotFound()
    {
        var db = new FakeApplicationDbContext();
        var result = await Handler(db).Handle(
            new RecordCashDisbursementCommand(10m, 55, null),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("الجهة الخارجية غير موجودة.", result.Error!.Message);
        Assert.Empty(db.CashMovements);
    }

    [Fact]
    public void Validator_RejectsNonPositiveAmount()
    {
        var validator = new RecordCashDisbursementCommandValidator();
        var outcome = validator.Validate(new RecordCashDisbursementCommand(-5m, null, null));
        Assert.False(outcome.IsValid);
        Assert.Contains(outcome.Errors, e => e.ErrorMessage == "المبلغ يجب أن يكون أكبر من صفر.");
    }
}
