using TopLab.Application.Features.InventoryAndAccounting.Queries.GetPatientSamplesDetail;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Billing;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using Xunit;

namespace TopLab.Application.Tests.Features.InventoryAndAccounting;

public class GetPatientSamplesDetailQueryHandlerTests
{
    private static readonly DateTime Day1 = new(2026, 3, 1, 10, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Day15 = new(2026, 3, 15, 9, 0, 0, DateTimeKind.Utc);

    private static FakeApplicationDbContext NewDb() => new();

    private static GetPatientSamplesDetailQueryHandler Handler(FakeApplicationDbContext db)
        => new(db, new FakeDateTimeProvider());

    private static Patient MakePatient(int id, string name, bool deleted = false)
    {
        var p = Patient.Create(PatientId.Create(id), name, Sex.Male, 30, AgeUnit.Year, Day1);
        if (deleted)
        {
            p.SoftDelete();
        }

        return p;
    }

    private static PatientTest MakeOrder(int id, int patientId, decimal price)
    {
        var pt = PatientTest.Create(PatientTestId.Create(id), PatientId.Create(patientId), TestId.Create(id), price);
        pt.CreatedAtUtc = Day1;
        return pt;
    }

    private static PaymentOperation MakeOp(int id, int patientId, decimal amount, decimal? discount = null)
    {
        return PaymentOperation.Create(
            PaymentOperationId.Create(id),
            PatientId.Create(patientId),
            amount,
            1,
            Day15,
            discount);
    }

    [Fact]
    public async Task Rows_ComposeTotalSamplesAggregate()
    {
        var db = NewDb();
        db.Patients.Add(MakePatient(1, "أحمد"));
        db.Patients.Add(MakePatient(2, "سارة"));
        db.PatientTests.Add(MakeOrder(1, 1, 100m));
        db.PatientTests.Add(MakeOrder(2, 1, 50m));
        db.PatientTests.Add(MakeOrder(3, 2, 80m));
        db.PaymentOperations.Add(MakeOp(1, 1, 80m, 10m));

        var result = await Handler(db).Handle(
            new GetPatientSamplesDetailQuery(new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31)),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Count);
        var p1 = result.Value.Single(r => r.PatientId == 1);
        Assert.Equal("أحمد", p1.FullName);
        Assert.Equal(2, p1.TestsCount);
        Assert.Equal(150m, p1.Charged);
        Assert.Equal(90m, p1.Paid);
        Assert.Equal(60m, p1.Balance);
        var p2 = result.Value.Single(r => r.PatientId == 2);
        Assert.Equal(1, p2.TestsCount);
        Assert.Equal(80m, p2.Charged);
        Assert.Equal(0m, p2.Paid);
        Assert.Equal(80m, p2.Balance);
        Assert.Equal(230m, result.Value.Sum(r => r.Charged));
    }

    [Fact]
    public async Task SoftDeletedPatients_AreExcluded()
    {
        var db = NewDb();
        db.Patients.Add(MakePatient(1, "live"));
        db.Patients.Add(MakePatient(2, "gone", deleted: true));
        db.PatientTests.Add(MakeOrder(1, 1, 50m));
        db.PatientTests.Add(MakeOrder(2, 2, 999m));

        var result = await Handler(db).Handle(
            new GetPatientSamplesDetailQuery(new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31)),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value!);
        Assert.Equal(1, result.Value!.Single().PatientId);
    }

    [Fact]
    public async Task EmptyPeriod_ReturnsEmptyList()
    {
        var db = NewDb();
        db.Patients.Add(MakePatient(1, "P"));
        db.PatientTests.Add(MakeOrder(1, 1, 50m));

        var result = await Handler(db).Handle(
            new GetPatientSamplesDetailQuery(new DateOnly(2026, 5, 1), new DateOnly(2026, 5, 31)),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!);
    }

    [Fact]
    public void Validator_RejectsInvertedPeriod()
    {
        var validator = new GetPatientSamplesDetailQueryValidator();
        var outcome = validator.Validate(new GetPatientSamplesDetailQuery(
            new DateOnly(2026, 3, 10),
            new DateOnly(2026, 3, 1)));

        Assert.False(outcome.IsValid);
        Assert.Contains(outcome.Errors, e => e.ErrorMessage == "بداية الفترة يجب ألا تتجاوز نهايتها.");
    }
}
