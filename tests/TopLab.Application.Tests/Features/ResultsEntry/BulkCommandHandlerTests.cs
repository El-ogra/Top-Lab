using TopLab.Application.Features.ResultsEntry.Commands.BulkPrint;
using TopLab.Application.Features.ResultsEntry.Commands.MarkAllPatientResultsReviewed;
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

public class BulkCommandHandlerTests
{
    private static Patient MakePatient(int id, DateTime? reg = null)
    {
        return Patient.Create(PatientId.Create(id), $"P{id}", Sex.Male, 30, AgeUnit.Year, reg ?? DateTime.UtcNow);
    }

    private static void AddSimpleTest(FakeApplicationDbContext db, int id)
    {
        if (!db.Tests.Any(t => t.Id.Value == id))
        {
            db.Tests.Add(Test.Create(TestId.Create(id), $"T{id}", $"T{id}", $"T{id}", $"T{id}", 30, 100m, ResultKind.Simple));
        }
    }

    private static PatientTest Entered(int ptId, int patientId, int testId = 10)
    {
        var pt = PatientTest.Create(PatientTestId.Create(ptId), PatientId.Create(patientId), TestId.Create(testId), 100m);
        pt.EnterResult("5", ResultFlag.Normal, 1, DateTime.UtcNow);
        return pt;
    }

    private static PatientTest Reviewed(int ptId, int patientId, int testId = 10)
    {
        var pt = Entered(ptId, patientId, testId);
        pt.MarkReviewed(1, DateTime.UtcNow);
        return pt;
    }

    [Fact]
    public async Task MarkAllReviewed_Skips_Unentered()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient(1));
        AddSimpleTest(db, 10);
        db.PatientTests.Add(Entered(101, 1));
        db.PatientTests.Add(PatientTest.Create(PatientTestId.Create(102), PatientId.Create(1), TestId.Create(10), 100m));

        var handler = new MarkAllPatientResultsReviewedCommandHandler(db, new FakeCurrentUserService(), new FakeDateTimeProvider());
        var result = await handler.Handle(new MarkAllPatientResultsReviewedCommand(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value);
        Assert.True(db.PatientTests.First(p => p.Id.Value == 101).IsReviewed);
        Assert.False(db.PatientTests.First(p => p.Id.Value == 102).IsReviewed);
    }

    [Fact]
    public async Task Preflight_RequiresConfirmation_When_PrintedIncluded()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient(1));
        db.Patients.Add(MakePatient(2));
        AddSimpleTest(db, 10);
        var first = Reviewed(101, 1);
        first.MarkPrinted(1, DateTime.UtcNow);
        db.PatientTests.Add(first);
        db.PatientTests.Add(Reviewed(102, 2));

        var handler = new BulkPrintPreflightQueryHandler(db);
        var result = await handler.Handle(new BulkPrintPreflightQuery(new[] { 1, 2 }), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Count);
        Assert.True(result.Value!.First(c => c.PatientId == 1).RequiresReprintConfirmation);
        Assert.False(result.Value!.First(c => c.PatientId == 2).RequiresReprintConfirmation);
        Assert.Equal(
            "لقد تم طباعه هذا التقرير لهذا المريض من قبل هل ترغب في اعاده الطباعه",
            BulkPrintMessages.ReprintConfirmationMessage);
    }

    [Fact]
    public async Task Execute_Cancel_Skips_EntireReport_And_Continues()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient(1));
        db.Patients.Add(MakePatient(2));
        AddSimpleTest(db, 10);
        var printed = Reviewed(101, 1);
        printed.MarkPrinted(1, DateTime.UtcNow);
        db.PatientTests.Add(printed);
        var fresh = Reviewed(102, 2);
        db.PatientTests.Add(fresh);
        var before = fresh.PrintCount;

        var handler = new ExecuteBulkPrintCommandHandler(db, new FakeCurrentUserService { UserId = 1 }, new FakeDateTimeProvider());
        var result = await handler.Handle(new ExecuteBulkPrintCommand(new[]
        {
            new BulkPrintDecision(1, ConfirmReprint: false),
            new BulkPrintDecision(2, ConfirmReprint: true),
        }), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Count);
        Assert.Equal(BulkPrintOutcomes.Skipped, result.Value!.First(o => o.PatientId == 1).Outcome);
        Assert.Equal(BulkPrintOutcomes.Printed, result.Value!.First(o => o.PatientId == 2).Outcome);
        Assert.Equal(1, printed.PrintCount);
        Assert.Equal(before + 1, fresh.PrintCount);
    }

    [Fact]
    public async Task Execute_Confirm_Reprints_And_Increments()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient(1));
        AddSimpleTest(db, 10);
        var pt = Reviewed(101, 1);
        pt.MarkPrinted(1, DateTime.UtcNow);
        var before = pt.PrintCount;
        db.PatientTests.Add(pt);

        var handler = new ExecuteBulkPrintCommandHandler(db, new FakeCurrentUserService { UserId = 1 }, new FakeDateTimeProvider());
        var result = await handler.Handle(new ExecuteBulkPrintCommand(new[]
        {
            new BulkPrintDecision(1, ConfirmReprint: true),
        }), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(BulkPrintOutcomes.Printed, result.Value![0].Outcome);
        Assert.Equal(before + 1, pt.PrintCount);
    }

    [Fact]
    public async Task Execute_BalanceBlock_Applies_OncePerPatient()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient(1));
        AddSimpleTest(db, 10);
        db.PatientTests.Add(PatientTest.Create(PatientTestId.Create(901), PatientId.Create(1), TestId.Create(10), 100m));
        var row = Reviewed(101, 1);
        db.PatientTests.Add(row);
        var pay = PaymentOperation.Create(PaymentOperationId.Create(1), PatientId.Create(1), 10m, 1, DateTime.UtcNow);
        db.PaymentOperations.Add(pay);
        db.Users.Add(User.Create(UserId.Create(5), "c", "h", "h2", false, 0, true));

        var user = new FakeCurrentUserService { UserId = 5, IsAbsolutePermission = false };
        var handler = new ExecuteBulkPrintCommandHandler(db, user, new FakeDateTimeProvider());
        var result = await handler.Handle(new ExecuteBulkPrintCommand(new[]
        {
            new BulkPrintDecision(1, ConfirmReprint: true),
        }), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(BulkPrintOutcomes.BlockedByBalance, result.Value![0].Outcome);
        Assert.False(row.IsPrinted);
    }
}
