using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientBilling.Commands.RecordPayment;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Users;
using Xunit;

namespace TopLab.Application.Tests.Features.PatientBilling;

public class RecordPaymentCommandHandlerTests
{
    private static void SeedPatient(FakeApplicationDbContext db, int id = 1)
    {
        db.Patients.Add(Patient.Create(PatientId.Create(id), "Ahmed", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow));
    }

    private static void SeedUser(FakeApplicationDbContext db, int id, decimal limitPercent)
    {
        db.Users.Add(User.Create(UserId.Create(id), $"cashier{id}", "hash", "winhash", discountLimitPercent: limitPercent));
    }

    private static (FakeApplicationDbContext Db, FakeCurrentUserService User, FakeDateTimeProvider Clock) Setup(
        int userId = 7, bool absolute = false)
    {
        return (new FakeApplicationDbContext(),
            new FakeCurrentUserService { UserId = userId, IsAbsolutePermission = absolute },
            new FakeDateTimeProvider());
    }

    [Fact]
    public async Task HappyPath_NoDiscount_SavesPayment()
    {
        var (db, user, clock) = Setup();
        SeedPatient(db);

        var handler = new RecordPaymentCommandHandler(db, user, clock);
        var result = await handler.Handle(new RecordPaymentCommand(1, 200m), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var saved = Assert.Single(db.PaymentOperations);
        Assert.Equal(200m, saved.Amount);
        Assert.Null(saved.DiscountAmount);
        Assert.False(saved.IsExtraCharge);
        Assert.Equal(TopLab.Domain.Common.Enums.OperationType.Payment, saved.OperationType);
        Assert.Equal(7, saved.ReceivedByUserId);
        Assert.Equal(1, db.SaveChangesCallCount);
    }

    [Fact]
    public async Task HappyPath_DiscountWithinLimit_SavesWithDiscount()
    {
        var (db, user, clock) = Setup();
        SeedPatient(db);
        SeedUser(db, 7, 20m);

        var handler = new RecordPaymentCommandHandler(db, user, clock);
        var result = await handler.Handle(new RecordPaymentCommand(1, 100m, 10m), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(10m, Assert.Single(db.PaymentOperations).DiscountAmount);
    }

    [Fact]
    public async Task DiscountBreach_ReturnsValidation_WithFrozenMessage()
    {
        var (db, user, clock) = Setup();
        SeedPatient(db);
        SeedUser(db, 7, 10m);

        var handler = new RecordPaymentCommandHandler(db, user, clock);
        var result = await handler.Handle(new RecordPaymentCommand(1, 100m, 15m), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error!.Type);
        Assert.Equal("الخصم يتجاوز الحد المسموح به لهذا المستخدم.", result.Error!.Message);
        Assert.Empty(db.PaymentOperations);
    }

    [Fact]
    public async Task AbsoluteUser_BypassesCap_WithoutUserRow()
    {
        var (db, user, clock) = Setup(absolute: true);
        SeedPatient(db);

        var handler = new RecordPaymentCommandHandler(db, user, clock);
        var result = await handler.Handle(new RecordPaymentCommand(1, 100m, 90m), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(90m, Assert.Single(db.PaymentOperations).DiscountAmount);
    }

    [Fact]
    public async Task MissingUserRow_WithDiscount_TreatedAsZeroLimit()
    {
        var (db, user, clock) = Setup();
        SeedPatient(db);

        var handler = new RecordPaymentCommandHandler(db, user, clock);
        var result = await handler.Handle(new RecordPaymentCommand(1, 100m, 5m), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error!.Type);
        Assert.Equal("الخصم يتجاوز الحد المسموح به لهذا المستخدم.", result.Error!.Message);
    }

    [Fact]
    public async Task UnknownPatient_ReturnsNotFound()
    {
        var (db, user, clock) = Setup();

        var handler = new RecordPaymentCommandHandler(db, user, clock);
        var result = await handler.Handle(new RecordPaymentCommand(99, 100m), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
        Assert.Equal("المريض غير موجود.", result.Error!.Message);
    }

    [Fact]
    public async Task SoftDeletedPatient_ReturnsNotFound()
    {
        var (db, user, clock) = Setup();
        var patient = Patient.Create(PatientId.Create(1), "Ahmed", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow);
        patient.SoftDelete();
        db.Patients.Add(patient);

        var handler = new RecordPaymentCommandHandler(db, user, clock);
        var result = await handler.Handle(new RecordPaymentCommand(1, 100m), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
    }
}
