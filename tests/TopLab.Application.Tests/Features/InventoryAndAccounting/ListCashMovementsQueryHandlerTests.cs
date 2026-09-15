using TopLab.Application.Features.InventoryAndAccounting.Queries.ListCashMovements;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Accounting;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.ExternalEntities;
using TopLab.Domain.Users;
using Xunit;

namespace TopLab.Application.Tests.Features.InventoryAndAccounting;

public class ListCashMovementsQueryHandlerTests
{
    private static readonly DateTime Day1 = new(2026, 3, 1, 10, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Day15 = new(2026, 3, 15, 9, 0, 0, DateTimeKind.Utc);

    private static ListCashMovementsQueryHandler Handler(FakeApplicationDbContext db)
        => new(db, new FakeDateTimeProvider());

    [Fact]
    public async Task PeriodFilter_AndDictionaryNames_WithRawIdFallback()
    {
        var db = new FakeApplicationDbContext();
        db.Users.Add(User.Create(UserId.Create(7), "cashier", "h", "w"));
        db.ExternalEntities.Add(ExternalEntity.Create(
            ExternalEntityId.Create(10),
            EntityType.TreatingDoctor,
            "جهة"));
        db.CashMovements.Add(CashMovement.Create(
            CashMovementId.Create(1),
            MovementType.Deposit,
            100m,
            7,
            Day1,
            ExternalEntityId.Create(10),
            "ملاحظة"));
        db.CashMovements.Add(CashMovement.Create(
            CashMovementId.Create(2),
            MovementType.Disbursement,
            30m,
            99,
            Day15));

        var result = await Handler(db).Handle(
            new ListCashMovementsQuery(new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31)),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Count);
        var named = result.Value.Single(m => m.Id == 1);
        Assert.Equal("جهة", named.RelatedExternalEntityName);
        Assert.Equal("cashier", named.PerformedByName);
        Assert.Equal("ملاحظة", named.Notes);
        var fallback = result.Value.Single(m => m.Id == 2);
        Assert.Null(fallback.RelatedExternalEntityName);
        Assert.Equal("99", fallback.PerformedByName);
    }

    [Fact]
    public async Task OutsidePeriod_Excluded()
    {
        var db = new FakeApplicationDbContext();
        db.CashMovements.Add(CashMovement.Create(
            CashMovementId.Create(1),
            MovementType.Deposit,
            100m,
            1,
            new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)));

        var result = await Handler(db).Handle(
            new ListCashMovementsQuery(new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31)),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!);
    }

    [Fact]
    public void Validator_RejectsInvertedPeriod()
    {
        var validator = new ListCashMovementsQueryValidator();
        var outcome = validator.Validate(new ListCashMovementsQuery(
            new DateOnly(2026, 3, 10),
            new DateOnly(2026, 3, 1)));

        Assert.False(outcome.IsValid);
        Assert.Contains(outcome.Errors, e => e.ErrorMessage == "بداية الفترة يجب ألا تتجاوز نهايتها.");
    }
}
