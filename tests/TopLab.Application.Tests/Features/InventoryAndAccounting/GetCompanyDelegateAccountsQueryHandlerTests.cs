using TopLab.Application.Features.InventoryAndAccounting.Queries.GetCompanyDelegateAccounts;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Accounting;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.ExternalEntities;
using TopLab.Domain.Results;
using TopLab.Domain.SentOutSamples;
using Xunit;

namespace TopLab.Application.Tests.Features.InventoryAndAccounting;

public class GetCompanyDelegateAccountsQueryHandlerTests
{
    private static readonly DateTime Day1 = new(2026, 3, 1, 10, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Day15 = new(2026, 3, 15, 9, 0, 0, DateTimeKind.Utc);

    private static GetCompanyDelegateAccountsQueryHandler Handler(FakeApplicationDbContext db)
        => new(db, new FakeDateTimeProvider());

    private static ExternalEntity MakeLab(int id, string name)
        => ExternalEntity.Create(ExternalEntityId.Create(id), EntityType.PartnerLab, name);

    [Fact]
    public async Task PerEntityTotals_NumberForNumber()
    {
        var db = new FakeApplicationDbContext();
        db.ExternalEntities.Add(MakeLab(5, "معمل أ"));
        db.CashMovements.Add(CashMovement.Create(CashMovementId.Create(1), MovementType.Deposit, 200m, 1, Day1, ExternalEntityId.Create(5)));
        db.CashMovements.Add(CashMovement.Create(CashMovementId.Create(2), MovementType.Deposit, 50m, 1, Day15, ExternalEntityId.Create(5)));
        db.CashMovements.Add(CashMovement.Create(CashMovementId.Create(3), MovementType.Disbursement, 30m, 1, Day15, ExternalEntityId.Create(5)));
        db.SentOutSamples.Add(SentOutSample.Create(
            SentOutSampleId.Create(1),
            PatientTestId.Create(1),
            ExternalEntityId.Create(5),
            60m,
            70m,
            Day15));
        db.SentOutSamplePayments.Add(SentOutSamplePayment.Create(
            SentOutSamplePaymentId.Create(1),
            SentOutSampleId.Create(1),
            25m,
            Day15,
            1));

        var result = await Handler(db).Handle(
            new GetCompanyDelegateAccountsQuery(new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31), null),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        var row = result.Value!.Single();
        Assert.Equal(5, row.EntityId);
        Assert.Equal("معمل أ", row.EntityName);
        Assert.Equal(250m, row.Deposits);
        Assert.Equal(30m, row.Disbursements);
        Assert.Equal(220m, row.Net);
        Assert.Equal(1, row.SentOutCount);
        Assert.Equal(60m, row.SentOutTotalCost);
        Assert.Equal(25m, row.SentOutTotalPaid);
        Assert.Equal(35m, row.SentOutRemaining);
    }

    [Fact]
    public async Task EntityFilter_OnlyThatEntity()
    {
        var db = new FakeApplicationDbContext();
        db.ExternalEntities.Add(MakeLab(5, "A"));
        db.ExternalEntities.Add(MakeLab(6, "B"));
        db.CashMovements.Add(CashMovement.Create(CashMovementId.Create(1), MovementType.Deposit, 100m, 1, Day1, ExternalEntityId.Create(5)));
        db.CashMovements.Add(CashMovement.Create(CashMovementId.Create(2), MovementType.Deposit, 80m, 1, Day1, ExternalEntityId.Create(6)));

        var result = await Handler(db).Handle(
            new GetCompanyDelegateAccountsQuery(new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31), 6),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!);
        Assert.Equal(6, result.Value!.Single().EntityId);
        Assert.Equal(80m, result.Value!.Single().Deposits);
    }

    [Fact]
    public async Task UnknownEntityFilter_ReturnsNotFound()
    {
        var db = new FakeApplicationDbContext();
        var result = await Handler(db).Handle(
            new GetCompanyDelegateAccountsQuery(new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31), 999),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("الجهة الخارجية غير موجودة.", result.Error!.Message);
    }

    [Fact]
    public void Validator_RejectsInvertedPeriod()
    {
        var validator = new GetCompanyDelegateAccountsQueryValidator();
        var outcome = validator.Validate(new GetCompanyDelegateAccountsQuery(
            new DateOnly(2026, 3, 10),
            new DateOnly(2026, 3, 1),
            null));

        Assert.False(outcome.IsValid);
        Assert.Contains(outcome.Errors, e => e.ErrorMessage == "بداية الفترة يجب ألا تتجاوز نهايتها.");
    }
}
