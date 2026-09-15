using TopLab.Application.Common.Results;
using TopLab.Application.Features.AuditAndTraceability.Queries.GetPatientAudit;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Billing;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Users;
using Xunit;

namespace TopLab.Application.Tests.Features.AuditAndTraceability;

public class GetPatientAuditQueryHandlerTests
{
    private static Patient MakePatient(
        int id,
        int createdBy = 5,
        int lastModifiedBy = 6,
        int modificationCount = 3,
        string name = "Ahmed")
    {
        var patient = Patient.Create(PatientId.Create(id), name, Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow);
        patient.CreatedByUserId = createdBy;
        patient.CreatedAtUtc = new DateTime(2026, 1, 10, 8, 0, 0, DateTimeKind.Utc);
        patient.LastModifiedByUserId = lastModifiedBy;
        patient.LastModifiedAtUtc = new DateTime(2026, 2, 11, 9, 30, 0, DateTimeKind.Utc);
        patient.ModificationCount = modificationCount;
        return patient;
    }

    private static void AddUser(FakeApplicationDbContext db, int id, string userName)
    {
        db.Users.Add(User.Create(UserId.Create(id), userName, "hash", "winhash"));
    }

    private static PaymentOperation MakeOp(
        int id, int patientId, decimal amount, int userId, DateTime atUtc, bool voided = false)
    {
        var op = PaymentOperation.Create(
            PaymentOperationId.Create(id), PatientId.Create(patientId), amount, userId, atUtc);
        if (voided)
        {
            op.Void();
        }

        return op;
    }

    [Fact]
    public async Task HappyPath_ReturnsAllPFields()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient(1));
        AddUser(db, 5, "registrar");
        AddUser(db, 6, "editor");
        AddUser(db, 7, "cashier1");
        db.PaymentOperations.Add(MakeOp(1, 1, 50m, 7, new DateTime(2026, 3, 1, 10, 0, 0, DateTimeKind.Utc)));

        var handler = new GetPatientAuditQueryHandler(db);
        var result = await handler.Handle(new GetPatientAuditQuery(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var audit = result.Value!;
        Assert.Equal(1, audit.PatientId);
        Assert.Equal("Ahmed", audit.FullName);
        Assert.Equal(5, audit.CreatedByUserId);
        Assert.Equal("registrar", audit.CreatedByUserName);
        Assert.Equal(new DateTime(2026, 1, 10, 8, 0, 0, DateTimeKind.Utc), audit.CreatedAtUtc);
        Assert.Equal(3, audit.ModificationCount);
        Assert.Equal(6, audit.LastModifiedByUserId);
        Assert.Equal("editor", audit.LastModifiedByUserName);
        Assert.Equal(new DateTime(2026, 2, 11, 9, 30, 0, DateTimeKind.Utc), audit.LastModifiedAtUtc);
        var receiver = Assert.Single(audit.PaymentReceivers);
        Assert.Equal(7, receiver.UserId);
        Assert.Equal("cashier1", receiver.UserName);
        Assert.Equal(new DateTime(2026, 3, 1, 10, 0, 0, DateTimeKind.Utc), receiver.OperationAtUtc);
    }

    [Fact]
    public async Task MultipleReceivers_AreDistinctAndTimeOrdered()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient(1));
        AddUser(db, 5, "registrar");
        AddUser(db, 6, "editor");
        AddUser(db, 7, "cashier1");
        AddUser(db, 8, "cashier2");
        db.PaymentOperations.Add(MakeOp(1, 1, 20m, 8, new DateTime(2026, 3, 5, 10, 0, 0, DateTimeKind.Utc)));
        db.PaymentOperations.Add(MakeOp(2, 1, 30m, 7, new DateTime(2026, 3, 1, 10, 0, 0, DateTimeKind.Utc)));
        db.PaymentOperations.Add(MakeOp(3, 1, 40m, 7, new DateTime(2026, 3, 3, 10, 0, 0, DateTimeKind.Utc)));

        var handler = new GetPatientAuditQueryHandler(db);
        var result = await handler.Handle(new GetPatientAuditQuery(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var receivers = result.Value!.PaymentReceivers;
        Assert.Equal(2, receivers.Count);
        // Ordered ascending by latest receipt time: user 7 (Mar 3) before user 8 (Mar 5).
        Assert.Equal(7, receivers[0].UserId);
        Assert.Equal("cashier1", receivers[0].UserName);
        Assert.Equal(new DateTime(2026, 3, 3, 10, 0, 0, DateTimeKind.Utc), receivers[0].OperationAtUtc);
        Assert.Equal(8, receivers[1].UserId);
        Assert.Equal("cashier2", receivers[1].UserName);
    }

    [Fact]
    public async Task VoidedOperations_AreIncluded()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient(1));
        AddUser(db, 5, "registrar");
        AddUser(db, 6, "editor");
        AddUser(db, 7, "cashier1");
        db.PaymentOperations.Add(MakeOp(1, 1, 999m, 7, new DateTime(2026, 3, 1, 10, 0, 0, DateTimeKind.Utc), voided: true));

        var handler = new GetPatientAuditQueryHandler(db);
        var result = await handler.Handle(new GetPatientAuditQuery(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var receiver = Assert.Single(result.Value!.PaymentReceivers);
        Assert.Equal(7, receiver.UserId);
        Assert.Equal("cashier1", receiver.UserName);
    }

    [Fact]
    public async Task PatientWithNoPayments_ReturnsEmptyReceiverList()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient(1));
        AddUser(db, 5, "registrar");
        AddUser(db, 6, "editor");

        var handler = new GetPatientAuditQueryHandler(db);
        var result = await handler.Handle(new GetPatientAuditQuery(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!.PaymentReceivers);
        Assert.Equal("registrar", result.Value.CreatedByUserName);
    }

    [Fact]
    public async Task UnknownPatient_ReturnsNotFound()
    {
        var db = new FakeApplicationDbContext();

        var handler = new GetPatientAuditQueryHandler(db);
        var result = await handler.Handle(new GetPatientAuditQuery(99), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
        Assert.Equal("المريض غير موجود.", result.Error!.Message);
    }

    [Fact]
    public async Task SoftDeletedPatient_IsStillReturned()
    {
        var db = new FakeApplicationDbContext();
        var patient = MakePatient(1);
        patient.SoftDelete();
        db.Patients.Add(patient);
        AddUser(db, 5, "registrar");
        AddUser(db, 6, "editor");

        var handler = new GetPatientAuditQueryHandler(db);
        var result = await handler.Handle(new GetPatientAuditQuery(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value!.PatientId);
    }

    [Fact]
    public async Task DeletedUser_FallsBackToRawIdString()
    {
        var db = new FakeApplicationDbContext();
        // Creator id 42 has no user row; receiver id 43 has no user row either.
        var patient = MakePatient(1, createdBy: 42, lastModifiedBy: 42, modificationCount: 1);
        db.Patients.Add(patient);
        db.PaymentOperations.Add(MakeOp(1, 1, 50m, 43, new DateTime(2026, 3, 1, 10, 0, 0, DateTimeKind.Utc)));

        var handler = new GetPatientAuditQueryHandler(db);
        var result = await handler.Handle(new GetPatientAuditQuery(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("42", result.Value!.CreatedByUserName);
        Assert.Equal("42", result.Value.LastModifiedByUserName);
        Assert.Equal("43", Assert.Single(result.Value.PaymentReceivers).UserName);
    }
}
