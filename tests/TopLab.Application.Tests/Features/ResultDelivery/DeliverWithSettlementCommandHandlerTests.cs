using TopLab.Application.Common.Results;
using TopLab.Application.Features.ResultDelivery.Commands.DeliverWithSettlement;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Billing;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;
using Xunit;

namespace TopLab.Application.Tests.Features.ResultDelivery;

public class DeliverWithSettlementCommandHandlerTests
{
    private static readonly DateTime Now = new(2026, 3, 10, 10, 0, 0, DateTimeKind.Utc);

    private static Patient MakePatient(int id = 1)
    {
        return Patient.Create(PatientId.Create(id), $"P{id}", Sex.Male, 30, AgeUnit.Year, Now);
    }

    private static void AddTest(FakeApplicationDbContext db, int id)
    {
        db.Tests.Add(Test.Create(TestId.Create(id), $"T{id}", $"T{id}", $"T{id}-R", $"T{id}", 30, 100m));
    }

    private static PatientTest PrintedRow(int ptId, int patientId, int testId, decimal price = 100m)
    {
        var pt = PatientTest.Create(PatientTestId.Create(ptId), PatientId.Create(patientId), TestId.Create(testId), price);
        pt.EnterResult("5", ResultFlag.Normal, 1, Now);
        pt.MarkReviewed(1, Now);
        pt.MarkPrinted(1, Now);
        return pt;
    }

    private static PatientTest UnprintedRow(int ptId, int patientId, int testId)
    {
        var pt = PatientTest.Create(PatientTestId.Create(ptId), PatientId.Create(patientId), TestId.Create(testId), 100m);
        pt.EnterResult("5", ResultFlag.Normal, 1, Now);
        pt.MarkReviewed(1, Now);
        return pt;
    }

    private static (FakeApplicationDbContext Db, FakeCurrentUserService User, FakeDateTimeProvider Clock) Setup(
        int userId = 7)
    {
        var db = new FakeApplicationDbContext();
        var user = new FakeCurrentUserService { UserId = userId };
        var clock = new FakeDateTimeProvider { UtcNow = Now };
        return (db, user, clock);
    }

    private static decimal Balance(FakeApplicationDbContext db, int patientId)
    {
        return PatientAccountCalculator.Balance(
            db.PatientTests.Where(pt => pt.PatientId.Value == patientId).Select(pt => pt.PriceAtOrderTime).ToList(),
            db.PaymentOperations.Where(o => o.PatientId.Value == patientId).ToList());
    }

    [Fact]
    public async Task HappyDelivery_WritesPerLineAudit_SingleSave()
    {
        var (db, user, clock) = Setup();
        db.Patients.Add(MakePatient());
        AddTest(db, 10);
        AddTest(db, 11);
        db.PatientTests.Add(PrintedRow(100, 1, 10));
        db.PatientTests.Add(PrintedRow(101, 1, 11));

        var handler = new DeliverWithSettlementCommandHandler(db, user, clock);
        var result = await handler.Handle(
            new DeliverWithSettlementCommand(1, new[] { 100, 101 }), CancellationToken.None);

        Assert.True(result.IsSuccess);
        foreach (var pt in db.PatientTests)
        {
            Assert.True(pt.IsDelivered);
            Assert.Equal(7, pt.DeliveredByUserId);
            Assert.Equal(Now, pt.DeliveredAtUtc);
        }
        Assert.Equal(1, db.SaveChangesCallCount);
    }

    [Fact]
    public async Task UnprintedLine_ConflictWithTranslatedMessage_NoSave()
    {
        var (db, user, clock) = Setup();
        db.Patients.Add(MakePatient());
        AddTest(db, 10);
        db.PatientTests.Add(UnprintedRow(100, 1, 10));

        var handler = new DeliverWithSettlementCommandHandler(db, user, clock);
        var result = await handler.Handle(
            new DeliverWithSettlementCommand(1, new[] { 100 }), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
        Assert.Equal("النتيجة غير مطبوعة.", result.Error!.Message);
        Assert.Equal(0, db.SaveChangesCallCount);
        Assert.False(db.PatientTests.Single().IsDelivered);
    }

    [Fact]
    public async Task PartialPayment_ReducesBalanceByExactlyTheAmount()
    {
        var (db, user, clock) = Setup();
        db.Patients.Add(MakePatient());
        AddTest(db, 10);
        db.PatientTests.Add(PrintedRow(100, 1, 10, price: 200m));
        Assert.Equal(200m, Balance(db, 1));

        var handler = new DeliverWithSettlementCommandHandler(db, user, clock);
        var result = await handler.Handle(
            new DeliverWithSettlementCommand(1, new[] { 100 }, SettleAmount: 70m), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(130m, Balance(db, 1));
        var op = Assert.Single(db.PaymentOperations);
        Assert.Equal(70m, op.Amount);
        Assert.Equal(OperationType.Payment, op.OperationType);
        Assert.Equal(7, op.ReceivedByUserId);
        Assert.True(db.PatientTests.Single().IsDelivered);
        Assert.Equal(1, db.SaveChangesCallCount);
    }

    [Fact]
    public async Task SettleInFull_ZeroesBalance_WithFullSettlementOperation()
    {
        var (db, user, clock) = Setup();
        db.Patients.Add(MakePatient());
        AddTest(db, 10);
        db.PatientTests.Add(PrintedRow(100, 1, 10, price: 200m));

        var handler = new DeliverWithSettlementCommandHandler(db, user, clock);
        var result = await handler.Handle(
            new DeliverWithSettlementCommand(1, new[] { 100 }, SettleInFull: true), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0m, Balance(db, 1));
        var op = Assert.Single(db.PaymentOperations);
        Assert.Equal(200m, op.Amount);
        Assert.Equal(OperationType.FullSettlement, op.OperationType);
    }

    [Theory]
    [InlineData(100)] // exact payment: zero balance
    [InlineData(150)] // overpayment: negative balance (patient credit)
    public async Task SettleInFull_NothingDue_Conflict(decimal alreadyPaid)
    {
        var (db, user, clock) = Setup();
        db.Patients.Add(MakePatient());
        AddTest(db, 10);
        db.PatientTests.Add(PrintedRow(100, 1, 10, price: 100m));
        db.PaymentOperations.Add(PaymentOperation.Create(
            PaymentOperationId.Create(1), PatientId.Create(1), alreadyPaid, 7, Now));

        var handler = new DeliverWithSettlementCommandHandler(db, user, clock);
        var result = await handler.Handle(
            new DeliverWithSettlementCommand(1, new[] { 100 }, SettleInFull: true), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
        Assert.Equal("لا يوجد رصيد مستحق للتسوية.", result.Error!.Message);
    }

    [Fact]
    public async Task NoSettlement_BalanceUnchanged()
    {
        var (db, user, clock) = Setup();
        db.Patients.Add(MakePatient());
        AddTest(db, 10);
        db.PatientTests.Add(PrintedRow(100, 1, 10, price: 200m));

        var handler = new DeliverWithSettlementCommandHandler(db, user, clock);
        var result = await handler.Handle(
            new DeliverWithSettlementCommand(1, new[] { 100 }), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(200m, Balance(db, 1));
        Assert.Empty(db.PaymentOperations);
    }

    [Fact]
    public async Task DeliveryWithRemainingBalance_Succeeds_NoGate()
    {
        var (db, user, clock) = Setup();
        db.Patients.Add(MakePatient());
        AddTest(db, 10);
        db.PatientTests.Add(PrintedRow(100, 1, 10, price: 500m));

        var handler = new DeliverWithSettlementCommandHandler(db, user, clock);
        var result = await handler.Handle(
            new DeliverWithSettlementCommand(1, new[] { 100 }), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(db.PatientTests.Single().IsDelivered);
        Assert.Equal(500m, Balance(db, 1));
    }

    [Fact]
    public async Task UnknownPatient_ReturnsNotFound()
    {
        var (db, user, clock) = Setup();

        var handler = new DeliverWithSettlementCommandHandler(db, user, clock);
        var result = await handler.Handle(
            new DeliverWithSettlementCommand(99, new[] { 100 }), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
        Assert.Equal("المريض غير موجود.", result.Error!.Message);
    }

    [Fact]
    public async Task UnknownLineId_ReturnsNotFound()
    {
        var (db, user, clock) = Setup();
        db.Patients.Add(MakePatient());
        AddTest(db, 10);
        db.PatientTests.Add(PrintedRow(100, 1, 10));

        var handler = new DeliverWithSettlementCommandHandler(db, user, clock);
        var result = await handler.Handle(
            new DeliverWithSettlementCommand(1, new[] { 999 }), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
        Assert.Equal("التحليل غير موجود", result.Error!.Message);
    }

    [Fact]
    public async Task LineOfAnotherPatient_ReturnsNotFound()
    {
        var (db, user, clock) = Setup();
        db.Patients.Add(MakePatient(1));
        db.Patients.Add(MakePatient(2));
        AddTest(db, 10);
        db.PatientTests.Add(PrintedRow(100, 2, 10));

        var handler = new DeliverWithSettlementCommandHandler(db, user, clock);
        var result = await handler.Handle(
            new DeliverWithSettlementCommand(1, new[] { 100 }), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
        Assert.Equal("التحليل غير موجود", result.Error!.Message);
    }

    [Fact]
    public void Validator_EmptyList_Rejects()
    {
        var validator = new DeliverWithSettlementCommandValidator();
        var result = validator.Validate(new DeliverWithSettlementCommand(1, Array.Empty<int>()));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "حدد نتيجة واحدة على الأقل للتسليم.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Validator_NonPositiveAmount_Rejects(decimal amount)
    {
        var validator = new DeliverWithSettlementCommandValidator();
        var result = validator.Validate(new DeliverWithSettlementCommand(1, new[] { 100 }, SettleAmount: amount));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "مبلغ التسوية غير صالح.");
    }

    [Fact]
    public void Validator_FullPlusPartial_AreMutuallyExclusive()
    {
        var validator = new DeliverWithSettlementCommandValidator();
        var result = validator.Validate(new DeliverWithSettlementCommand(1, new[] { 100 }, SettleAmount: 10m, SettleInFull: true));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "مبلغ التسوية غير صالح.");
    }
}
