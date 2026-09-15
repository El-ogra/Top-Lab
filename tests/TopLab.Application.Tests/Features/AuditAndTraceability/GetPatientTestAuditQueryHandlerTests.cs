using TopLab.Application.Common.Results;
using TopLab.Application.Features.AuditAndTraceability.Queries.GetPatientTestAudit;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;
using TopLab.Domain.Users;
using Xunit;

namespace TopLab.Application.Tests.Features.AuditAndTraceability;

public class GetPatientTestAuditQueryHandlerTests
{
    private static void AddUser(FakeApplicationDbContext db, int id, string userName)
    {
        db.Users.Add(User.Create(UserId.Create(id), userName, "hash", "winhash"));
    }

    private static void AddTest(FakeApplicationDbContext db, int id, string name)
    {
        db.Tests.Add(Test.Create(TestId.Create(id), name, name, name, $"T{id}", 30, 100m));
    }

    private static PatientTest MakeFullLifecycle(int id = 100, int patientId = 1, int testId = 10)
    {
        var pt = PatientTest.Create(
            PatientTestId.Create(id), PatientId.Create(patientId), TestId.Create(testId), 100m);
        pt.MarkEntered(7, new DateTime(2026, 3, 1, 10, 0, 0, DateTimeKind.Utc));
        pt.MarkReviewed(8, new DateTime(2026, 3, 1, 11, 0, 0, DateTimeKind.Utc));
        pt.MarkPrinted(9, new DateTime(2026, 3, 1, 12, 0, 0, DateTimeKind.Utc));
        pt.MarkPrinted(9, new DateTime(2026, 3, 1, 12, 30, 0, DateTimeKind.Utc));
        pt.MarkDelivered(11, new DateTime(2026, 3, 1, 13, 0, 0, DateTimeKind.Utc));
        return pt;
    }

    [Fact]
    public async Task FullLifecycle_ReturnsAllFieldsWithPrintCount()
    {
        var db = new FakeApplicationDbContext();
        AddTest(db, 10, "CBC");
        AddUser(db, 7, "entrytech");
        AddUser(db, 8, "reviewer");
        AddUser(db, 9, "printer");
        AddUser(db, 11, "deliverer");
        db.PatientTests.Add(MakeFullLifecycle());

        var handler = new GetPatientTestAuditQueryHandler(db);
        var result = await handler.Handle(new GetPatientTestAuditQuery(100), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var audit = result.Value!;
        Assert.Equal(100, audit.PatientTestId);
        Assert.Equal(1, audit.PatientId);
        Assert.Equal("CBC", audit.TestName);
        Assert.Equal(7, audit.EnteredByUserId);
        Assert.Equal("entrytech", audit.EnteredByUserName);
        Assert.Equal(new DateTime(2026, 3, 1, 10, 0, 0, DateTimeKind.Utc), audit.EnteredAtUtc);
        Assert.Equal(8, audit.ReviewedByUserId);
        Assert.Equal("reviewer", audit.ReviewedByUserName);
        Assert.Equal(new DateTime(2026, 3, 1, 11, 0, 0, DateTimeKind.Utc), audit.ReviewedAtUtc);
        Assert.Equal(9, audit.LastPrintedByUserId);
        Assert.Equal("printer", audit.LastPrintedByUserName);
        Assert.Equal(new DateTime(2026, 3, 1, 12, 30, 0, DateTimeKind.Utc), audit.LastPrintedAtUtc);
        Assert.Equal(2, audit.PrintCount);
        Assert.Equal(11, audit.DeliveredByUserId);
        Assert.Equal("deliverer", audit.DeliveredByUserName);
        Assert.Equal(new DateTime(2026, 3, 1, 13, 0, 0, DateTimeKind.Utc), audit.DeliveredAtUtc);
    }

    [Fact]
    public async Task EnteredOnly_LaterFieldsAreNull()
    {
        var db = new FakeApplicationDbContext();
        AddTest(db, 10, "CBC");
        AddUser(db, 7, "entrytech");
        var pt = PatientTest.Create(
            PatientTestId.Create(100), PatientId.Create(1), TestId.Create(10), 100m);
        pt.MarkEntered(7, new DateTime(2026, 3, 1, 10, 0, 0, DateTimeKind.Utc));
        db.PatientTests.Add(pt);

        var handler = new GetPatientTestAuditQueryHandler(db);
        var result = await handler.Handle(new GetPatientTestAuditQuery(100), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var audit = result.Value!;
        Assert.Equal(7, audit.EnteredByUserId);
        Assert.Equal("entrytech", audit.EnteredByUserName);
        Assert.NotNull(audit.EnteredAtUtc);
        Assert.Null(audit.ReviewedByUserId);
        Assert.Null(audit.ReviewedByUserName);
        Assert.Null(audit.ReviewedAtUtc);
        Assert.Null(audit.LastPrintedByUserId);
        Assert.Null(audit.LastPrintedByUserName);
        Assert.Null(audit.LastPrintedAtUtc);
        Assert.Equal(0, audit.PrintCount);
        Assert.Null(audit.DeliveredByUserId);
        Assert.Null(audit.DeliveredByUserName);
        Assert.Null(audit.DeliveredAtUtc);
    }

    [Fact]
    public async Task UnknownTest_ReturnsNotFound()
    {
        var db = new FakeApplicationDbContext();

        var handler = new GetPatientTestAuditQueryHandler(db);
        var result = await handler.Handle(new GetPatientTestAuditQuery(999), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
        Assert.Equal("التحليل غير موجود", result.Error!.Message);
    }

    [Fact]
    public async Task DeletedUsers_FallBackToRawIdStrings()
    {
        var db = new FakeApplicationDbContext();
        AddTest(db, 10, "CBC");
        // No user rows at all: every lifecycle id renders raw.
        var pt = PatientTest.Create(
            PatientTestId.Create(100), PatientId.Create(1), TestId.Create(10), 100m);
        pt.MarkEntered(42, new DateTime(2026, 3, 1, 10, 0, 0, DateTimeKind.Utc));
        pt.MarkReviewed(43, new DateTime(2026, 3, 1, 11, 0, 0, DateTimeKind.Utc));
        pt.MarkPrinted(44, new DateTime(2026, 3, 1, 12, 0, 0, DateTimeKind.Utc));
        pt.MarkDelivered(45, new DateTime(2026, 3, 1, 13, 0, 0, DateTimeKind.Utc));
        db.PatientTests.Add(pt);

        var handler = new GetPatientTestAuditQueryHandler(db);
        var result = await handler.Handle(new GetPatientTestAuditQuery(100), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var audit = result.Value!;
        Assert.Equal("42", audit.EnteredByUserName);
        Assert.Equal("43", audit.ReviewedByUserName);
        Assert.Equal("44", audit.LastPrintedByUserName);
        Assert.Equal("45", audit.DeliveredByUserName);
    }
}
