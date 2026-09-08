using TopLab.Application.Features.ResultsEntry.Commands.MarkResultDelivered;
using TopLab.Application.Features.ResultsEntry.Commands.MarkResultPrinted;
using TopLab.Application.Features.ResultsEntry.Commands.ReviewResult;
using TopLab.Application.Features.ResultsEntry.Commands.UnreviewResult;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Billing;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Tests;
using TopLab.Domain.Users;
using Xunit;

namespace TopLab.Application.Tests.Features.ResultsEntry;

public class ReviewPrintDeliverCommandHandlerTests
{
    private static Patient MakePatient(int id = 1)
    {
        return Patient.Create(PatientId.Create(id), $"P{id}", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow);
    }

    private static void AddSimpleTest(FakeApplicationDbContext db, int id)
    {
        db.Tests.Add(Test.Create(TestId.Create(id), $"T{id}", $"T{id}", $"T{id}", $"T{id}", 30, 100m, ResultKind.Simple));
    }

    private static PatientTest EnteredRow(int ptId = 101, int patientId = 1, int testId = 10)
    {
        var pt = PatientTest.Create(PatientTestId.Create(ptId), PatientId.Create(patientId), TestId.Create(testId), 100m);
        pt.EnterResult("5", ResultFlag.Normal, 1, DateTime.UtcNow);
        return pt;
    }

    [Fact]
    public async Task Review_WithoutEntry_Conflict()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient());
        AddSimpleTest(db, 10);
        db.PatientTests.Add(PatientTest.Create(PatientTestId.Create(101), PatientId.Create(1), TestId.Create(10), 100m));

        var handler = new ReviewResultCommandHandler(db, new FakeCurrentUserService(), new FakeDateTimeProvider());
        var result = await handler.Handle(new ReviewResultCommand(101), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("لا يمكن اعتماد نتيجة غير مدخلة.", result.Error!.Message);
    }

    [Fact]
    public async Task Review_AlreadyReviewed_Idempotent()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient());
        AddSimpleTest(db, 10);
        var pt = EnteredRow();
        pt.MarkReviewed(1, DateTime.UtcNow);
        db.PatientTests.Add(pt);

        var handler = new ReviewResultCommandHandler(db, new FakeCurrentUserService(), new FakeDateTimeProvider());
        var result = await handler.Handle(new ReviewResultCommand(101), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(pt.IsReviewed);
        Assert.Equal(0, db.SaveChangesCallCount);
    }

    [Fact]
    public async Task Unreview_Printed_Conflict()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient());
        AddSimpleTest(db, 10);
        var pt = EnteredRow();
        pt.MarkReviewed(1, DateTime.UtcNow);
        pt.MarkPrinted(1, DateTime.UtcNow);
        db.PatientTests.Add(pt);

        var handler = new UnreviewResultCommandHandler(db);
        var result = await handler.Handle(new UnreviewResultCommand(101), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("لا يمكن إلغاء اعتماد نتيجة مطبوعة أو مسلمة.", result.Error!.Message);
    }

    [Fact]
    public async Task Print_WithoutReview_Conflict()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient());
        AddSimpleTest(db, 10);
        db.PatientTests.Add(EnteredRow());

        var handler = new MarkResultPrintedCommandHandler(db, new FakeCurrentUserService(), new FakeDateTimeProvider());
        var result = await handler.Handle(new MarkResultPrintedCommand(101), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("لا يمكن طباعة نتيجة غير معتمدة.", result.Error!.Message);
    }

    private static void SeedBalanceWorkedExample(FakeApplicationDbContext db, int patientId)
    {
        db.PatientTests.Add(PatientTest.Create(PatientTestId.Create(901), PatientId.Create(patientId), TestId.Create(10), 100m));
        db.PatientTests.Add(PatientTest.Create(PatientTestId.Create(902), PatientId.Create(patientId), TestId.Create(11), 50m));
        var extra = PaymentOperation.Create(PaymentOperationId.Create(911), PatientId.Create(patientId), 20m, 1, DateTime.UtcNow, null, true);
        var pay = PaymentOperation.Create(PaymentOperationId.Create(912), PatientId.Create(patientId), 80m, 1, DateTime.UtcNow, 10m);
        var voided = PaymentOperation.Create(PaymentOperationId.Create(913), PatientId.Create(patientId), 999m, 1, DateTime.UtcNow);
        voided.Void();
        db.PaymentOperations.Add(extra);
        db.PaymentOperations.Add(pay);
        db.PaymentOperations.Add(voided);
    }

    [Fact]
    public async Task Print_Blocked_When_FlagOn_And_BalancePositive()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient(1));
        AddSimpleTest(db, 10);
        db.Tests.Add(Test.Create(TestId.Create(11), "T11", "T11", "T11", "T11", 30, 50m));
        SeedBalanceWorkedExample(db, 1);
        var row = EnteredRow(101, 1, 10);
        row.MarkReviewed(1, DateTime.UtcNow);
        db.PatientTests.Add(row);
        db.Users.Add(User.Create(UserId.Create(5), "cashier", "h", "h2", false, 0, true));

        var user = new FakeCurrentUserService { UserId = 5, IsAbsolutePermission = false };
        var handler = new MarkResultPrintedCommandHandler(db, user, new FakeDateTimeProvider());
        var result = await handler.Handle(new MarkResultPrintedCommand(101), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("يوجد رصيد متبقٍ على حساب المريض؛ لا يمكن الطباعة.", result.Error!.Message);
        Assert.False(row.IsPrinted);
    }

    [Fact]
    public async Task Print_Allowed_When_UserFlagOff()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient(1));
        AddSimpleTest(db, 10);
        db.Tests.Add(Test.Create(TestId.Create(11), "T11", "T11", "T11", "T11", 30, 50m));
        SeedBalanceWorkedExample(db, 1);
        var row = EnteredRow(101, 1, 10);
        row.MarkReviewed(1, DateTime.UtcNow);
        db.PatientTests.Add(row);
        db.Users.Add(User.Create(UserId.Create(5), "cashier", "h", "h2", false, 0, false));

        var user = new FakeCurrentUserService { UserId = 5, IsAbsolutePermission = false };
        var handler = new MarkResultPrintedCommandHandler(db, user, new FakeDateTimeProvider());
        var result = await handler.Handle(new MarkResultPrintedCommand(101), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(row.IsPrinted);
    }

    [Fact]
    public async Task Print_Allowed_When_AbsolutePermission()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient(1));
        AddSimpleTest(db, 10);
        db.Tests.Add(Test.Create(TestId.Create(11), "T11", "T11", "T11", "T11", 30, 50m));
        SeedBalanceWorkedExample(db, 1);
        var row = EnteredRow(101, 1, 10);
        row.MarkReviewed(1, DateTime.UtcNow);
        db.PatientTests.Add(row);
        db.Users.Add(User.Create(UserId.Create(5), "admin", "h", "h2", true, 0, true));

        var user = new FakeCurrentUserService { UserId = 5, IsAbsolutePermission = true };
        var handler = new MarkResultPrintedCommandHandler(db, user, new FakeDateTimeProvider());
        var result = await handler.Handle(new MarkResultPrintedCommand(101), CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Print_Allowed_When_BalanceSettled()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient(1));
        AddSimpleTest(db, 10);
        var row = EnteredRow(101, 1, 10);
        row.MarkReviewed(1, DateTime.UtcNow);
        db.PatientTests.Add(row);
        db.PatientTests.Add(PatientTest.Create(PatientTestId.Create(902), PatientId.Create(1), TestId.Create(10), 100m));
        var pay = PaymentOperation.Create(PaymentOperationId.Create(912), PatientId.Create(1), 200m, 1, DateTime.UtcNow);
        db.PaymentOperations.Add(pay);
        db.Users.Add(User.Create(UserId.Create(5), "cashier", "h", "h2", false, 0, true));

        var user = new FakeCurrentUserService { UserId = 5, IsAbsolutePermission = false };
        var handler = new MarkResultPrintedCommandHandler(db, user, new FakeDateTimeProvider());
        var result = await handler.Handle(new MarkResultPrintedCommand(101), CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Deliver_WithoutPrint_Conflict()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient());
        AddSimpleTest(db, 10);
        var pt = EnteredRow();
        pt.MarkReviewed(1, DateTime.UtcNow);
        db.PatientTests.Add(pt);

        var handler = new MarkResultDeliveredCommandHandler(db, new FakeCurrentUserService(), new FakeDateTimeProvider());
        var result = await handler.Handle(new MarkResultDeliveredCommand(101), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("لا يمكن تسليم نتيجة غير مطبوعة.", result.Error!.Message);
    }

    [Fact]
    public async Task Deliver_NotBalanceGated_EvenWithPositiveBalance()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient(1));
        AddSimpleTest(db, 10);
        db.Tests.Add(Test.Create(TestId.Create(11), "T11", "T11", "T11", "T11", 30, 50m));
        SeedBalanceWorkedExample(db, 1);
        var row = EnteredRow(101, 1, 10);
        row.MarkReviewed(1, DateTime.UtcNow);
        row.MarkPrinted(1, DateTime.UtcNow);
        db.PatientTests.Add(row);
        db.Users.Add(User.Create(UserId.Create(5), "cashier", "h", "h2", false, 0, true));

        var user = new FakeCurrentUserService { UserId = 5, IsAbsolutePermission = false };
        var handler = new MarkResultDeliveredCommandHandler(db, user, new FakeDateTimeProvider());
        var result = await handler.Handle(new MarkResultDeliveredCommand(101), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(row.IsDelivered);
    }
}
