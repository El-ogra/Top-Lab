using TopLab.Application.Common.Results;
using TopLab.Application.Features.ProfileResults.Commands.AmendProfileResult;
using TopLab.Application.Features.ResultDelivery.Commands.DeliverWithSettlement;
using TopLab.Application.Features.ResultsEntry.Common;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Application.Tests.Features.ProfileResults;
using TopLab.Application.Tests.Features.ResultsEntry;
using TopLab.Application.Features.ReportProduction.Common;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Results;
using TopLab.Domain.Users;
using Xunit;

namespace TopLab.Application.Tests.Features.ResultsEntry;

/// <summary>
/// W-02 post-implementation fix — the behavioural net that was missing.
/// <para>
/// The Wave 2 defect survived a fully green suite because every delivery and amendment test
/// arranged <c>IsPrinted = true</c> by hand (<c>pt.MarkPrinted(...)</c> /
/// <c>AddPrintedItem(..., isPrinted: true)</c>). These tests never do that. They start from
/// a genuinely unprinted row, go through a real production printing path, and then assert on
/// the downstream behaviour that the defect had silently disabled.
/// </para>
/// <para>
/// If a future change breaks printed-state recording again, these fail — because they ask
/// "does printing make delivery possible?" rather than "does delivery work when the caller
/// says it is printed?".
/// </para>
/// </summary>
public class PrintToDownstreamStateTests
{
    /// <summary>
    /// Prints through the combined-report path (the one that already recorded state before
    /// the fix, so this guards against regression in the "was working" direction), then
    /// delivers the result without touching IsPrinted by hand.
    /// </summary>
    [Fact]
    public async Task PrintCombinedReport_ThenDeliver_SucceedsWithoutManualMarking()
    {
        var db = new FakeApplicationDbContext();
        var user = new FakeCurrentUserService { UserId = 1, IsAbsolutePermission = true };
        var clock = new FakeDateTimeProvider { UtcNow = new DateTime(2026, 10, 2, 12, 0, 0, DateTimeKind.Utc) };

        db.Patients.Add(ProfileResultsSeed.MakePatient(1));
        db.Tests.Add(ProfileResultsSeed.MakeSpecializedTest(10));
        ProfileResultsSeed.SeedAnalyteWithRange(db, 1);
        ProfileResultsSeed.AddProfile(db, 10, 1);
        db.Users.Add(User.Create(UserId.Create(1), "cashier", "h", "h2", false, 0, true));
        var row = ProfileResultsSeed.AddProfileTest(db, 101, 1, 10);
        row.EnterResult("4", null, 1, DateTime.UtcNow);
        row.MarkReviewed(1, DateTime.UtcNow);

        Assert.False(row.IsPrinted);

        var printing = new FakeReportPrintingService();
        var sender = new FakeSender().WithCombinedReport(
            1,
            101,
            new CombinedReportDto(
                1,
                "P",
                null,
                [new CombinedReportLineDto(101, 10, "Profile", "PRF", (int)ResultKind.SpecializedProfile, null, null, null, [], null)]));

        var recorder = new FakePrintedStateRecorder(db, user, clock);
        var print = new TopLab.Application.Features.ReportProduction.Commands.PrintCombinedReport.PrintCombinedReportCommandHandler(
            db, user, clock, sender, printing, recorder);

        var printResult = await print.Handle(
            new TopLab.Application.Features.ReportProduction.Commands.PrintCombinedReport.PrintCombinedReportCommand(1, new[] { 101 }),
            CancellationToken.None);

        Assert.True(printResult.IsSuccess);
        Assert.True(row.IsPrinted);

        // The real production chain the defect had disabled.
        var deliver = new DeliverWithSettlementCommandHandler(db, user, clock);
        var deliverResult = await deliver.Handle(
            new DeliverWithSettlementCommand(1, new[] { 101 }),
            CancellationToken.None);

        Assert.True(deliverResult.IsSuccess);
        Assert.True(row.IsDelivered);
    }

    /// <summary>
    /// Chain B — printing a profile must make post-print amendment reachable
    /// (owner decision 2). The item starts unverified-but-verified-and-unprinted; the print
    /// path records it, and only then does <c>AmendProfileResultCommandHandler</c> accept it.
    /// </summary>
    [Fact]
    public async Task PrintProfile_ThenAmend_SucceedsWithoutManualItemMarking()
    {
        var db = new FakeApplicationDbContext();
        var user = new FakeCurrentUserService { UserId = 1 };
        var clock = new FakeDateTimeProvider { UtcNow = new DateTime(2026, 10, 2, 12, 0, 0, DateTimeKind.Utc) };

        db.Patients.Add(ProfileResultsSeed.MakePatient(1));
        db.Tests.Add(ProfileResultsSeed.MakeSpecializedTest(10));
        ProfileResultsSeed.SeedAnalyteWithRange(db, 1);
        ProfileResultsSeed.AddProfile(db, 10, 1);
        var row = ProfileResultsSeed.AddProfileTest(db, 101, 1, 10);
        row.EnterResult("4", null, 1, DateTime.UtcNow);
        row.MarkReviewed(1, DateTime.UtcNow);

        // A genuinely unprinted, verified profile item — exactly what the production print
        // path must turn into a printed one.
        var item = ProfileResultItem.Create(
            ProfileResultItemId.Create(501),
            row.Id,
            AnalyteId.Create(1),
            "4",
            "mg",
            ProfileResultFlag.Low,
            isVerified: true,
            isPrinted: false);
        db.ProfileResultItems.Add(item);

        Assert.False(item.IsPrinted);
        Assert.False(row.IsPrinted);

        var recorder = new FakePrintedStateRecorder(db, user, clock);
        var recordResult = await recorder.RecordForPatientTestAsync(101, CancellationToken.None);

        Assert.True(recordResult.IsSuccess);
        Assert.True(row.IsPrinted);
        Assert.True(item.IsPrinted);
        Assert.Equal(1, item.PrintCount);

        // Owner decision 2, proved through the real command and its real guard.
        var amend = new AmendProfileResultCommandHandler(db, user, clock);
        var amendResult = await amend.Handle(
            new AmendProfileResultCommand(501, "5.5", "mg", null, null),
            CancellationToken.None);

        Assert.True(amendResult.IsSuccess);
        Assert.Equal("5.5", item.ResultValue);
    }

    /// <summary>
    /// Chain C — the negative direction. Before printing, delivery must still be refused.
    /// This pins that the fix did not simply unlock the guard.
    /// </summary>
    [Fact]
    public async Task Deliver_BeforePrinting_IsStillRefused()
    {
        var db = new FakeApplicationDbContext();
        var user = new FakeCurrentUserService { UserId = 1, IsAbsolutePermission = true };
        var clock = new FakeDateTimeProvider();

        db.Patients.Add(ProfileResultsSeed.MakePatient(1));
        db.Tests.Add(ProfileResultsSeed.MakeSpecializedTest(10));
        var row = ProfileResultsSeed.AddProfileTest(db, 101, 1, 10);
        row.EnterResult("4", null, 1, DateTime.UtcNow);
        row.MarkReviewed(1, DateTime.UtcNow);

        var deliver = new DeliverWithSettlementCommandHandler(db, user, clock);
        var result = await deliver.Handle(
            new DeliverWithSettlementCommand(1, new[] { 101 }),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("النتيجة غير مطبوعة.", result.Error!.Message);
    }

    /// <summary>
    /// Owner decision 4 (strict): a domain guard still rejects the transition, and the
    /// request reports failure instead of recording a partial state.
    /// </summary>
    [Fact]
    public async Task Record_WithUnverifiedProfileItem_FailsAndRecordsNothing()
    {
        var db = new FakeApplicationDbContext();
        var user = new FakeCurrentUserService { UserId = 1 };
        var clock = new FakeDateTimeProvider();

        db.Patients.Add(ProfileResultsSeed.MakePatient(1));
        db.Tests.Add(ProfileResultsSeed.MakeSpecializedTest(10));
        ProfileResultsSeed.SeedAnalyteWithRange(db, 1);
        ProfileResultsSeed.AddProfile(db, 10, 1);
        var row = ProfileResultsSeed.AddProfileTest(db, 101, 1, 10);
        row.EnterResult("4", null, 1, DateTime.UtcNow);
        row.MarkReviewed(1, DateTime.UtcNow);

        var item = ProfileResultItem.Create(
            ProfileResultItemId.Create(501),
            row.Id,
            AnalyteId.Create(1),
            "4",
            "mg",
            null,
            isVerified: false,
            isPrinted: false);
        db.ProfileResultItems.Add(item);

        var recorder = new FakePrintedStateRecorder(db, user, clock);
        var result = await recorder.RecordForPatientTestAsync(101, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("المادة غير معتمدة؛ لا يمكن طباعتها.", result.Error!.Message);
        Assert.Equal(0, db.SaveChangesCallCount);
    }

    /// <summary>Nothing to record means nothing is written.</summary>
    [Fact]
    public async Task Record_WithNoIds_DoesNotSave()
    {
        var db = new FakeApplicationDbContext();
        var recorder = new FakePrintedStateRecorder(db);

        var result = await recorder.RecordAsync(
            Array.Empty<int>(), Array.Empty<int>(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, db.SaveChangesCallCount);
    }
}