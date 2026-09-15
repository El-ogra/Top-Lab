using TopLab.Domain.Accounting;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using Xunit;

namespace TopLab.Domain.Tests.Accounting;

public class CashMovementTests
{
    private static readonly DateTime Occurred = new(2026, 3, 15, 10, 30, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_Valid_RoundTripsAllFields()
    {
        var entity = ExternalEntityId.Create(7);
        var movement = CashMovement.Create(
            CashMovementId.Create(1),
            MovementType.Deposit,
            125.50m,
            performedByUserId: 3,
            Occurred,
            entity,
            "إيداع نقدي");

        Assert.Equal(1, movement.Id.Value);
        Assert.Equal(MovementType.Deposit, movement.MovementType);
        Assert.Equal(125.50m, movement.Amount);
        Assert.Equal(3, movement.PerformedByUserId);
        Assert.Equal(Occurred, movement.OccurredAtUtc);
        Assert.Equal(7, movement.RelatedExternalEntityId!.Value);
        Assert.Equal("إيداع نقدي", movement.Notes);
    }

    [Fact]
    public void Create_NullEntityAndNotes_Accepted()
    {
        var movement = CashMovement.Create(
            CashMovementId.Create(2),
            MovementType.Disbursement,
            10m,
            1,
            Occurred);

        Assert.Null(movement.RelatedExternalEntityId);
        Assert.Null(movement.Notes);
    }

    [Fact]
    public void Create_ZeroAmount_Throws_ParamNameAmount()
    {
        var ex = Assert.Throws<ArgumentException>(() => CashMovement.Create(
            CashMovementId.Create(1),
            MovementType.Deposit,
            0m,
            1,
            Occurred));

        Assert.Equal("amount", ex.ParamName);
    }

    [Fact]
    public void Create_NegativeAmount_Throws_ParamNameAmount()
    {
        var ex = Assert.Throws<ArgumentException>(() => CashMovement.Create(
            CashMovementId.Create(1),
            MovementType.Disbursement,
            -1m,
            1,
            Occurred));

        Assert.Equal("amount", ex.ParamName);
    }

    [Fact]
    public void Create_NotesAtMaxLength500_Accepted()
    {
        var notes = new string('n', 500);
        var movement = CashMovement.Create(
            CashMovementId.Create(1),
            MovementType.Deposit,
            1m,
            1,
            Occurred,
            notes: notes);

        Assert.Equal(500, movement.Notes!.Length);
    }

    [Fact]
    public void Create_NotesOver500_Throws_ParamNameNotes()
    {
        var notes = new string('n', 501);
        var ex = Assert.Throws<ArgumentException>(() => CashMovement.Create(
            CashMovementId.Create(1),
            MovementType.Deposit,
            1m,
            1,
            Occurred,
            notes: notes));

        Assert.Equal("notes", ex.ParamName);
    }

    [Fact]
    public void Create_DefaultTimestamp_Throws_ParamNameOccurredAtUtc()
    {
        var ex = Assert.Throws<ArgumentException>(() => CashMovement.Create(
            CashMovementId.Create(1),
            MovementType.Deposit,
            1m,
            1,
            default));

        Assert.Equal("occurredAtUtc", ex.ParamName);
    }
}
