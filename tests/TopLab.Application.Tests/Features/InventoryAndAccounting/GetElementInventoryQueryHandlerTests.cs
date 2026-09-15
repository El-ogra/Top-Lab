using TopLab.Application.Features.InventoryAndAccounting.Common;
using TopLab.Application.Features.InventoryAndAccounting.Queries.GetElementInventory;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Billing;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.ExternalEntities;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Users;
using Xunit;

namespace TopLab.Application.Tests.Features.InventoryAndAccounting;

public class GetElementInventoryQueryHandlerTests
{
    private static readonly DateTime Day1 = new(2026, 3, 1, 10, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Day15 = new(2026, 3, 15, 9, 0, 0, DateTimeKind.Utc);

    private static FakeApplicationDbContext NewDb() => new();

    private static GetElementInventoryQueryHandler Handler(FakeApplicationDbContext db)
        => new(db, new FakeDateTimeProvider());

    private static Patient MakePatient(int id, int? referralId = null, int? doctorId = null, AccountType accountType = AccountType.Individual, bool deleted = false)
    {
        var p = Patient.Create(
            PatientId.Create(id),
            $"P{id}",
            Sex.Male,
            30,
            AgeUnit.Year,
            Day1,
            accountType,
            referralEntityId: referralId.HasValue ? ExternalEntityId.Create(referralId.Value) : null,
            treatingDoctorId: doctorId.HasValue ? ExternalEntityId.Create(doctorId.Value) : null);
        if (deleted)
        {
            p.SoftDelete();
        }

        return p;
    }

    private static PatientTest MakeOrder(int id, int patientId, decimal price, int? enteredByUserId = 1)
    {
        var pt = PatientTest.Create(
            PatientTestId.Create(id),
            PatientId.Create(patientId),
            TestId.Create(id),
            price);
        pt.CreatedAtUtc = Day1;
        if (enteredByUserId.HasValue)
        {
            pt.EnterResult("1", null, enteredByUserId.Value, Day1);
        }

        return pt;
    }

    private static PaymentOperation MakeOp(int id, int patientId, decimal amount, int receivedByUserId = 1)
    {
        return PaymentOperation.Create(
            PaymentOperationId.Create(id),
            PatientId.Create(patientId),
            amount,
            receivedByUserId,
            Day15);
    }

    [Fact]
    public async Task UserElement_FiltersByReceiverAndEnterer()
    {
        var db = NewDb();
        db.Users.Add(User.Create(UserId.Create(7), "tech", "h", "w"));
        db.Patients.Add(MakePatient(1));
        db.PatientTests.Add(MakeOrder(1, 1, 100m, enteredByUserId: 7));
        db.PatientTests.Add(MakeOrder(2, 1, 50m, enteredByUserId: 8));
        db.PaymentOperations.Add(MakeOp(1, 1, 80m, receivedByUserId: 7));
        db.PaymentOperations.Add(MakeOp(2, 1, 20m, receivedByUserId: 9));

        var result = await Handler(db).Handle(
            new GetElementInventoryQuery(
                new DateOnly(2026, 3, 1),
                new DateOnly(2026, 3, 31),
                InventoryElementKind.User,
                7,
                null,
                InventoryReportType.Summary),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("tech", result.Value!.ElementName);
        Assert.Equal(1, result.Value.TotalSamplesCount);
        Assert.Equal(100m, result.Value.TotalSamplesAmount);
        Assert.Equal(80m, result.Value.CollectedAmount);
    }

    [Fact]
    public async Task UnknownUser_ReturnsNotFound()
    {
        var db = NewDb();
        var result = await Handler(db).Handle(
            new GetElementInventoryQuery(
                new DateOnly(2026, 3, 1),
                new DateOnly(2026, 3, 31),
                InventoryElementKind.User,
                999,
                null,
                InventoryReportType.Summary),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("المستخدم غير موجود.", result.Error!.Message);
    }

    [Fact]
    public async Task ReferralEntityElement_FiltersPatientsByReferral()
    {
        var db = NewDb();
        db.ExternalEntities.Add(ExternalEntity.Create(
            ExternalEntityId.Create(10),
            EntityType.TreatingDoctor,
            "جهة"));
        db.Patients.Add(MakePatient(1, referralId: 10));
        db.Patients.Add(MakePatient(2, referralId: 11));
        db.PatientTests.Add(MakeOrder(1, 1, 100m));
        db.PatientTests.Add(MakeOrder(2, 2, 400m));

        var result = await Handler(db).Handle(
            new GetElementInventoryQuery(
                new DateOnly(2026, 3, 1),
                new DateOnly(2026, 3, 31),
                InventoryElementKind.ReferralEntity,
                10,
                null,
                InventoryReportType.Summary),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value!.TotalSamplesCount);
        Assert.Equal(100m, result.Value.TotalSamplesAmount);
    }

    [Fact]
    public async Task UnknownEntity_ReturnsNotFound()
    {
        var db = NewDb();
        var result = await Handler(db).Handle(
            new GetElementInventoryQuery(
                new DateOnly(2026, 3, 1),
                new DateOnly(2026, 3, 31),
                InventoryElementKind.ReferralEntity,
                999,
                null,
                InventoryReportType.Summary),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("الجهة الخارجية غير موجودة.", result.Error!.Message);
    }

    [Fact]
    public async Task AccountTypeElement_FiltersPatients()
    {
        var db = NewDb();
        db.Patients.Add(MakePatient(1, accountType: AccountType.Vip));
        db.Patients.Add(MakePatient(2, accountType: AccountType.Individual));
        db.PatientTests.Add(MakeOrder(1, 1, 70m));
        db.PatientTests.Add(MakeOrder(2, 2, 30m));

        var result = await Handler(db).Handle(
            new GetElementInventoryQuery(
                new DateOnly(2026, 3, 1),
                new DateOnly(2026, 3, 31),
                InventoryElementKind.AccountType,
                null,
                AccountType.Vip,
                InventoryReportType.Summary),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Vip", result.Value!.ElementName);
        Assert.Equal(1, result.Value.TotalSamplesCount);
        Assert.Equal(70m, result.Value.TotalSamplesAmount);
    }

    [Fact]
    public async Task SoftDeletedPatient_Excluded()
    {
        var db = NewDb();
        db.Patients.Add(MakePatient(1, deleted: true, accountType: AccountType.Vip));
        db.PatientTests.Add(MakeOrder(1, 1, 70m));

        var result = await Handler(db).Handle(
            new GetElementInventoryQuery(
                new DateOnly(2026, 3, 1),
                new DateOnly(2026, 3, 31),
                InventoryElementKind.AccountType,
                null,
                AccountType.Vip,
                InventoryReportType.Summary),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Value!.TotalSamplesCount);
    }

    [Fact]
    public async Task ReportType_Summary_HasNoLines_DetailedByResults_HasTestLines()
    {
        var db = NewDb();
        db.Patients.Add(MakePatient(1, accountType: AccountType.Individual));
        db.PatientTests.Add(MakeOrder(1, 1, 100m));
        db.PatientTests.Add(MakeOrder(2, 1, 50m));

        var summary = await Handler(db).Handle(
            new GetElementInventoryQuery(
                new DateOnly(2026, 3, 1),
                new DateOnly(2026, 3, 31),
                InventoryElementKind.AccountType,
                null,
                AccountType.Individual,
                InventoryReportType.Summary),
            CancellationToken.None);

        var detailed = await Handler(db).Handle(
            new GetElementInventoryQuery(
                new DateOnly(2026, 3, 1),
                new DateOnly(2026, 3, 31),
                InventoryElementKind.AccountType,
                null,
                AccountType.Individual,
                InventoryReportType.DetailedByResults),
            CancellationToken.None);

        Assert.True(summary.IsSuccess);
        Assert.True(detailed.IsSuccess);
        Assert.Empty(summary.Value!.Lines);
        Assert.Equal(2, detailed.Value!.Lines.Count);
        Assert.Equal(100m, detailed.Value.Lines.Single(l => l.Key == 1).Amount);
    }

    [Fact]
    public void Validator_RejectsInvertedPeriod()
    {
        var validator = new GetElementInventoryQueryValidator();
        var outcome = validator.Validate(new GetElementInventoryQuery(
            new DateOnly(2026, 3, 10),
            new DateOnly(2026, 3, 1),
            InventoryElementKind.User,
            1,
            null,
            InventoryReportType.Summary));

        Assert.False(outcome.IsValid);
        Assert.Contains(outcome.Errors, e => e.ErrorMessage == "بداية الفترة يجب ألا تتجاوز نهايتها.");
    }
}
