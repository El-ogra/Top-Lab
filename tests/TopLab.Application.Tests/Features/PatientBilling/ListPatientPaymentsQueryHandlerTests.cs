using TopLab.Application.Features.PatientBilling.Queries.ListPatientPayments;
using TopLab.Application.Common.Results;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Billing;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using Xunit;

namespace TopLab.Application.Tests.Features.PatientBilling;

public class ListPatientPaymentsQueryHandlerTests
{
    private static readonly DateTime T0 = new(2026, 9, 1, 10, 0, 0, DateTimeKind.Utc);

    private static void SeedPatient(FakeApplicationDbContext db, int id = 1)
    {
        db.Patients.Add(Patient.Create(PatientId.Create(id), "Karim", Sex.Male, 40, AgeUnit.Year, DateTime.UtcNow));
    }

    private static PaymentOperation MakeOp(int id, decimal amount, DateTime at, bool voided = false)
    {
        var op = PaymentOperation.Create(
            PaymentOperationId.Create(id), PatientId.Create(1), amount, 7, at);
        if (voided)
            op.Void();
        return op;
    }

    [Fact]
    public async Task OrdersNewestFirst_WithIdTieBreak_AndIncludesVoided()
    {
        var db = new FakeApplicationDbContext();
        SeedPatient(db);
        db.PaymentOperations.Add(MakeOp(1, 10m, T0));
        db.PaymentOperations.Add(MakeOp(2, 20m, T0.AddHours(1)));
        db.PaymentOperations.Add(MakeOp(3, 30m, T0.AddHours(1)));
        db.PaymentOperations.Add(MakeOp(4, 999m, T0.AddHours(2), voided: true));

        var handler = new ListPatientPaymentsQueryHandler(db);
        var result = await handler.Handle(new ListPatientPaymentsQuery(1, 1, 500), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var ids = result.Value!.Select(o => o.PaymentOperationId).ToList();
        Assert.Equal(new List<int> { 4, 3, 2, 1 }, ids);
        Assert.True(result.Value!.First().IsVoided);
    }

    [Fact]
    public async Task Pagination_SkipsAndTakes()
    {
        var db = new FakeApplicationDbContext();
        SeedPatient(db);
        for (var i = 1; i <= 5; i++)
            db.PaymentOperations.Add(MakeOp(i, i * 10m, T0.AddMinutes(i)));

        var handler = new ListPatientPaymentsQueryHandler(db);
        var page2 = await handler.Handle(new ListPatientPaymentsQuery(1, 2, 2), CancellationToken.None);

        Assert.True(page2.IsSuccess);
        Assert.Equal(new List<int> { 3, 2 }, page2.Value!.Select(o => o.PaymentOperationId).ToList());
    }

    [Fact]
    public async Task UnknownPatient_ReturnsNotFound()
    {
        var db = new FakeApplicationDbContext();

        var handler = new ListPatientPaymentsQueryHandler(db);
        var result = await handler.Handle(new ListPatientPaymentsQuery(55, 1, 10), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
        Assert.Equal("المريض غير موجود.", result.Error!.Message);
    }

    [Fact]
    public async Task SoftDeletedPatient_ReturnsNotFound()
    {
        var db = new FakeApplicationDbContext();
        var patient = Patient.Create(PatientId.Create(1), "Karim", Sex.Male, 40, AgeUnit.Year, DateTime.UtcNow);
        patient.SoftDelete();
        db.Patients.Add(patient);

        var handler = new ListPatientPaymentsQueryHandler(db);
        var result = await handler.Handle(new ListPatientPaymentsQuery(1, 1, 10), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
    }
}
