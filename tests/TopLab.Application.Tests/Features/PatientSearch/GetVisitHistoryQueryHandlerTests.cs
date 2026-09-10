using TopLab.Application.Common.Results;
using TopLab.Application.Features.PatientSearch.Queries.GetVisitHistory;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Domain.Billing;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Settings;
using Xunit;

namespace TopLab.Application.Tests.Features.PatientSearch;

public class GetVisitHistoryQueryHandlerTests
{
    private static Patient MakePatient(int id, string name, string? labId, DateTime when)
    {
        var p = Patient.Create(PatientId.Create(id), name, Sex.Male, 30, AgeUnit.Year, when);
        if (labId != null)
        {
            p.AssignLabId(LabId.Create(labId));
        }
        return p;
    }

    [Fact]
    public async Task MultiVisit_GroupedBySharedLabId_OrderedDesc()
    {
        var db = new FakeApplicationDbContext();
        db.ReportSettings.Add(ReportSettings.CreateDefault());
        var v1 = MakePatient(1, "V1", "LAB-1", DateTime.UtcNow.AddDays(-2));
        var v2 = MakePatient(2, "V2", "LAB-1", DateTime.UtcNow.AddDays(-1));
        var unrelated = MakePatient(3, "Unrelated", "OTHER-1", DateTime.UtcNow);
        db.Patients.Add(v1);
        db.Patients.Add(v2);
        db.Patients.Add(unrelated);

        var handler = new GetVisitHistoryQueryHandler(db);
        var result = await handler.Handle(new GetVisitHistoryQuery(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var history = result.Value!;
        Assert.Equal("LAB-1", history.LabId);
        Assert.Equal("V1", history.PatientFullName);
        Assert.Equal(2, history.Visits.Count);
        Assert.Equal(2, history.Visits[0].PatientId);
        Assert.Equal(1, history.Visits[1].PatientId);
    }

    [Fact]
    public async Task NullLabId_SingleVisitHistory()
    {
        var db = new FakeApplicationDbContext();
        db.ReportSettings.Add(ReportSettings.CreateDefault());
        var noLab = MakePatient(1, "NoLab", null, DateTime.UtcNow);
        var other = MakePatient(2, "Other", "ANY-LAB", DateTime.UtcNow);
        db.Patients.Add(noLab);
        db.Patients.Add(other);

        var handler = new GetVisitHistoryQueryHandler(db);
        var result = await handler.Handle(new GetVisitHistoryQuery(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var visit = Assert.Single(result.Value!.Visits);
        Assert.Equal(1, visit.PatientId);
        Assert.Null(visit.LabId);
    }

    [Fact]
    public async Task MissingOrDeleted_ReturnsNotFound()
    {
        var db = new FakeApplicationDbContext();
        db.ReportSettings.Add(ReportSettings.CreateDefault());
        var deleted = MakePatient(1, "Gone", null, DateTime.UtcNow);
        deleted.SoftDelete();
        db.Patients.Add(deleted);

        var handler = new GetVisitHistoryQueryHandler(db);

        var missing = await handler.Handle(new GetVisitHistoryQuery(999), CancellationToken.None);
        Assert.False(missing.IsSuccess);
        Assert.Equal(ErrorType.NotFound, missing.Error!.Type);
        Assert.Equal("المريض غير موجود.", missing.Error!.Message);

        var deletedResult = await handler.Handle(new GetVisitHistoryQuery(1), CancellationToken.None);
        Assert.False(deletedResult.IsSuccess);
        Assert.Equal(ErrorType.NotFound, deletedResult.Error!.Type);
    }

    [Fact]
    public async Task SoftDeleted_SiblingVisits_Excluded()
    {
        var db = new FakeApplicationDbContext();
        db.ReportSettings.Add(ReportSettings.CreateDefault());
        var live = MakePatient(1, "Live", "LAB-1", DateTime.UtcNow.AddDays(-2));
        var gone = MakePatient(2, "Gone", "LAB-1", DateTime.UtcNow.AddDays(-1));
        gone.SoftDelete();
        db.Patients.Add(live);
        db.Patients.Add(gone);

        var handler = new GetVisitHistoryQueryHandler(db);
        var result = await handler.Handle(new GetVisitHistoryQuery(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var visit = Assert.Single(result.Value!.Visits);
        Assert.Equal(1, visit.PatientId);
    }

    [Fact]
    public async Task Rollup_CountsAndFlags()
    {
        var db = new FakeApplicationDbContext();
        db.ReportSettings.Add(ReportSettings.CreateDefault());
        var now = DateTime.UtcNow;
        var visit = MakePatient(1, "Rollup", null, now);
        db.Patients.Add(visit);

        var delivered = PatientTest.Create(PatientTestId.Create(1), PatientId.Create(1), TestId.Create(10), 100m);
        delivered.MarkEntered(5, now);
        delivered.MarkReviewed(5, now);
        delivered.MarkPrinted(5, now);
        delivered.MarkDelivered(5, now);
        db.PatientTests.Add(delivered);

        var enteredOnly = PatientTest.Create(PatientTestId.Create(2), PatientId.Create(1), TestId.Create(11), 50m);
        enteredOnly.MarkEntered(5, now);
        db.PatientTests.Add(enteredOnly);

        var handler = new GetVisitHistoryQueryHandler(db);
        var result = await handler.Handle(new GetVisitHistoryQuery(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var summary = Assert.Single(result.Value!.Visits);
        Assert.Equal(2, summary.TestCount);
        Assert.Equal(2, summary.ResultsEntered);
        Assert.Equal(1, summary.ReviewedCount);
        Assert.Equal(1, summary.PrintedCount);
        Assert.Equal(1, summary.DeliveredCount);
        Assert.True(summary.AllResultsEntered);
        Assert.False(summary.AllReviewed);
        Assert.False(summary.AllPrinted);
        Assert.False(summary.AllDelivered);
    }

    [Fact]
    public async Task EmptyVisit_AllFalse_StatusS1()
    {
        var db = new FakeApplicationDbContext();
        db.ReportSettings.Add(ReportSettings.CreateDefault());
        var visit = MakePatient(1, "Empty", null, DateTime.UtcNow);
        db.Patients.Add(visit);

        var handler = new GetVisitHistoryQueryHandler(db);
        var result = await handler.Handle(new GetVisitHistoryQuery(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var summary = Assert.Single(result.Value!.Visits);
        Assert.Equal(0, summary.TestCount);
        Assert.False(summary.AllResultsEntered);
        Assert.False(summary.AllReviewed);
        Assert.False(summary.AllPrinted);
        Assert.False(summary.AllDelivered);
        Assert.Equal(1, summary.AggregateStatus);
    }

    [Fact]
    public async Task AggregateStatus_MixedStages_UsesCalculatorTruthSet()
    {
        var db = new FakeApplicationDbContext();
        db.ReportSettings.Add(ReportSettings.CreateDefault());
        var now = DateTime.UtcNow;
        // registered yesterday so S1 masking (registration today) never applies
        var visit = MakePatient(1, "Mixed", null, now.AddDays(-1));
        db.Patients.Add(visit);

        // stage 3 (print-pending): entered + reviewed, not printed
        var s3 = PatientTest.Create(PatientTestId.Create(1), PatientId.Create(1), TestId.Create(10), 100m);
        s3.MarkEntered(5, now);
        s3.MarkReviewed(5, now);
        db.PatientTests.Add(s3);

        // stage 4 (delivery-pending): entered + reviewed + printed, not delivered
        var s4 = PatientTest.Create(PatientTestId.Create(2), PatientId.Create(1), TestId.Create(11), 100m);
        s4.MarkEntered(5, now);
        s4.MarkReviewed(5, now);
        s4.MarkPrinted(5, now);
        db.PatientTests.Add(s4);

        // stage 1 (entry-pending): not entered
        db.PatientTests.Add(PatientTest.Create(PatientTestId.Create(3), PatientId.Create(1), TestId.Create(12), 100m));

        var handler = new GetVisitHistoryQueryHandler(db);
        var result = await handler.Handle(new GetVisitHistoryQuery(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var summary = Assert.Single(result.Value!.Visits);
        Assert.Equal(2, summary.AggregateStatus);
    }

    [Fact]
    public async Task Balance_WorksAgainstWorkedExample()
    {
        var db = new FakeApplicationDbContext();
        db.ReportSettings.Add(ReportSettings.CreateDefault());
        var now = DateTime.UtcNow;
        var visit = MakePatient(1, "Balance", null, now);
        db.Patients.Add(visit);

        // prices 100 + 50, extra charge 20, payment 80 with discount 10, voided payment 999
        db.PatientTests.Add(PatientTest.Create(PatientTestId.Create(1), PatientId.Create(1), TestId.Create(10), 100m));
        db.PatientTests.Add(PatientTest.Create(PatientTestId.Create(2), PatientId.Create(1), TestId.Create(11), 50m));
        db.PaymentOperations.Add(PaymentOperation.Create(
            PaymentOperationId.Create(1), PatientId.Create(1), 20m, 5, now, isExtraCharge: true));
        db.PaymentOperations.Add(PaymentOperation.Create(
            PaymentOperationId.Create(2), PatientId.Create(1), 80m, 5, now, discountAmount: 10m));
        var voided = PaymentOperation.Create(
            PaymentOperationId.Create(3), PatientId.Create(1), 999m, 5, now);
        voided.Void();
        db.PaymentOperations.Add(voided);

        var handler = new GetVisitHistoryQueryHandler(db);
        var result = await handler.Handle(new GetVisitHistoryQuery(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var summary = Assert.Single(result.Value!.Visits);
        // charged 150 + 20 = 170; paid 80 + 10 = 90; balance = 170 - 90 = 80
        Assert.Equal(80m, summary.Balance);
        Assert.Equal(2, summary.TestCount);
    }

    [Fact]
    public async Task HistorySettings_Echoed()
    {
        var db = new FakeApplicationDbContext();
        var settings = ReportSettings.CreateDefault();
        settings.SetHistoryOptions(HistorySortMode.ByPatientName, false);
        db.ReportSettings.Add(settings);
        db.Patients.Add(MakePatient(1, "Echo", null, DateTime.UtcNow));

        var handler = new GetVisitHistoryQueryHandler(db);
        var result = await handler.Handle(new GetVisitHistoryQuery(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("ByPatientName", result.Value!.HistorySortMode);
        Assert.False(result.Value!.HistoryAutoDisplayEnabled);
    }

    [Fact]
    public async Task MissingReportSettings_ReturnsUnexpected()
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(MakePatient(1, "A", null, DateTime.UtcNow));

        var handler = new GetVisitHistoryQueryHandler(db);
        var result = await handler.Handle(new GetVisitHistoryQuery(1), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Unexpected, result.Error!.Type);
    }
}