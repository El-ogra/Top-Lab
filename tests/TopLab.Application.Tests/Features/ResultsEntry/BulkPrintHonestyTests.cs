using TopLab.Application.Features.ResultsEntry.Commands.BulkPrint;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Users;
using Xunit;

namespace TopLab.Application.Tests.Features.ResultsEntry;

/// <summary>
/// W-02 S6 (WP-06): bulk print must report what actually happened. Before this slice it always
/// answered <c>Printed</c> without producing a single sheet.
/// </summary>
public class BulkPrintHonestyTests
{
    [Fact]
    public async Task BulkPrint_AllPatientsPrint_ReportsPrinted()
    {
        var db = new FakeApplicationDbContext();
        AddPatient(db, 1);
        AddPatient(db, 2);
        db.PatientTests.Add(Reviewed(101, 1));
        db.PatientTests.Add(Reviewed(102, 2));
        var coordinator = new FakeResultPrintCoordinator();

        var result = await new ExecuteBulkPrintCommandHandler(db, new FakeCurrentUserService(), coordinator)
            .Handle(new ExecuteBulkPrintCommand(new[]
            {
                new BulkPrintDecision(1, true),
                new BulkPrintDecision(2, true)
            }), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.All(result.Value!, o => Assert.Equal(BulkPrintOutcomes.Printed, o.Outcome));
        Assert.Equal(2, coordinator.Calls.Count);
    }

    /// <summary>The decisive case: one patient's failure must not sink the batch, and must not be
    /// reported as success.</summary>
    [Fact]
    public async Task BulkPrint_OnePatientFails_ReportsFailedForThatPatientOnly()
    {
        var db = new FakeApplicationDbContext();
        AddPatient(db, 1);
        AddPatient(db, 2);
        db.PatientTests.Add(Reviewed(101, 1));
        db.PatientTests.Add(Reviewed(102, 2));
        var coordinator = new FakeResultPrintCoordinator
        {
            // Fail only patient 1's test.
            FailForPatientTestId = 101
        };

        var result = await new ExecuteBulkPrintCommandHandler(db, new FakeCurrentUserService(), coordinator)
            .Handle(new ExecuteBulkPrintCommand(new[]
            {
                new BulkPrintDecision(1, true),
                new BulkPrintDecision(2, true)
            }), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var first = result.Value![0];
        var second = result.Value![1];
        Assert.Equal(BulkPrintOutcomes.Failed, first.Outcome);
        Assert.Equal(BulkPrintOutcomes.Printed, second.Outcome);
        // The other patient still got their sheet.
        Assert.Equal(1, second.PrintedCount);
    }

    [Fact]
    public async Task BulkPrint_PrintServiceFails_ReportsFailedForEveryPatient()
    {
        var db = new FakeApplicationDbContext();
        AddPatient(db, 1);
        AddPatient(db, 2);
        db.PatientTests.Add(Reviewed(101, 1));
        db.PatientTests.Add(Reviewed(102, 2));
        var coordinator = new FakeResultPrintCoordinator { Printed = false };

        var result = await new ExecuteBulkPrintCommandHandler(db, new FakeCurrentUserService(), coordinator)
            .Handle(new ExecuteBulkPrintCommand(new[]
            {
                new BulkPrintDecision(1, true),
                new BulkPrintDecision(2, true)
            }), CancellationToken.None);

        Assert.All(result.Value!, o => Assert.Equal(BulkPrintOutcomes.Failed, o.Outcome));
        Assert.All(result.Value!, o => Assert.Equal(0, o.PrintedCount));
    }

    /// <summary>SD-1: bulk print must not write IsPrinted/PrintCount any more.</summary>
    [Fact]
    public async Task BulkPrint_NeverMarksPrinted()
    {
        var db = new FakeApplicationDbContext();
        AddPatient(db, 1);
        var row = Reviewed(101, 1);
        db.PatientTests.Add(row);

        await new ExecuteBulkPrintCommandHandler(db, new FakeCurrentUserService(), new FakeResultPrintCoordinator())
            .Handle(new ExecuteBulkPrintCommand(new[] { new BulkPrintDecision(1, true) }), CancellationToken.None);

        Assert.False(row.IsPrinted);
        Assert.Equal(0, row.PrintCount);
    }

    [Fact]
    public async Task BulkPrint_NoVerifiedResults_StillReportsNoVerifiedResults()
    {
        var db = new FakeApplicationDbContext();
        AddPatient(db, 1);

        var result = await new ExecuteBulkPrintCommandHandler(db, new FakeCurrentUserService(), new FakeResultPrintCoordinator())
            .Handle(new ExecuteBulkPrintCommand(new[] { new BulkPrintDecision(1, true) }), CancellationToken.None);

        Assert.Equal(BulkPrintOutcomes.NoVerifiedResults, result.Value![0].Outcome);
    }

    [Fact]
    public async Task BulkPrint_RequiresReprintConfirmation_Unchanged()
    {
        var db = new FakeApplicationDbContext();
        AddPatient(db, 1);
        var row = Reviewed(101, 1);
        row.MarkPrinted(1, DateTime.UtcNow);
        db.PatientTests.Add(row);

        var result = await new ExecuteBulkPrintCommandHandler(db, new FakeCurrentUserService(), new FakeResultPrintCoordinator())
            .Handle(new ExecuteBulkPrintCommand(new[] { new BulkPrintDecision(1, false) }), CancellationToken.None);

        Assert.Equal(BulkPrintOutcomes.Skipped, result.Value![0].Outcome);
    }

    [Fact]
    public async Task BulkPrint_PatientNotFound_StillReported()
    {
        var db = new FakeApplicationDbContext();

        var result = await new ExecuteBulkPrintCommandHandler(db, new FakeCurrentUserService(), new FakeResultPrintCoordinator())
            .Handle(new ExecuteBulkPrintCommand(new[] { new BulkPrintDecision(99, true) }), CancellationToken.None);

        Assert.Equal(BulkPrintOutcomes.PatientNotFound, result.Value![0].Outcome);
    }

    /// <summary>W-02 S8 (WP-13): the lab can suppress the reprint confirmation.</summary>
    [Fact]
    public async Task BulkPrint_SuppressReprint_SkipsConfirmation()
    {
        var db = new FakeApplicationDbContext();
        AddPatient(db, 1);
        var row = Reviewed(101, 1);
        row.MarkPrinted(1, DateTime.UtcNow);
        db.PatientTests.Add(row);
        var settings = TopLab.Domain.Settings.ReportSettings.CreateDefault();
        settings.SetPrintOptions(false, true);
        db.ReportSettings.Add(settings);

        var result = await new ExecuteBulkPrintCommandHandler(db, new FakeCurrentUserService(), new FakeResultPrintCoordinator())
            .Handle(new ExecuteBulkPrintCommand(new[] { new BulkPrintDecision(1, false) }), CancellationToken.None);

        Assert.Equal(BulkPrintOutcomes.Printed, result.Value![0].Outcome);
    }

    [Fact]
    public async Task BulkPrint_DefaultBehaviour_ShowsConfirmation()
    {
        var db = new FakeApplicationDbContext();
        AddPatient(db, 1);
        var row = Reviewed(101, 1);
        row.MarkPrinted(1, DateTime.UtcNow);
        db.PatientTests.Add(row);
        db.ReportSettings.Add(TopLab.Domain.Settings.ReportSettings.CreateDefault());

        var result = await new ExecuteBulkPrintCommandHandler(db, new FakeCurrentUserService(), new FakeResultPrintCoordinator())
            .Handle(new ExecuteBulkPrintCommand(new[] { new BulkPrintDecision(1, false) }), CancellationToken.None);

        Assert.Equal(BulkPrintOutcomes.Skipped, result.Value![0].Outcome);
    }

    private static void AddPatient(FakeApplicationDbContext db, int id) =>
        db.Patients.Add(Patient.Create(PatientId.Create(id), $"P{id}", Sex.Male, 30, AgeUnit.Year, DateTime.UtcNow));

    private static PatientTest Reviewed(int ptId, int patientId)
    {
        var pt = PatientTest.Create(PatientTestId.Create(ptId), PatientId.Create(patientId), TestId.Create(1), 100m);
        pt.EnterResult("5", ResultFlag.Normal, 1, DateTime.UtcNow);
        pt.MarkReviewed(1, DateTime.UtcNow);
        return pt;
    }
}
