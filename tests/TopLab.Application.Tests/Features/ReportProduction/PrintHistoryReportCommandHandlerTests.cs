using System.Text.Json;
using TopLab.Application.Common.Results;
using TopLab.Application.Features.ReportProduction.Commands.PrintHistoryReport;
using TopLab.Application.Features.ReportProduction.Common;
using TopLab.Application.Features.ReportProduction.Queries.GetPatientTestHistory;
using TopLab.Application.Tests.Common.Fakes;
using TopLab.Application.Tests.Features.ProfileResults;
using TopLab.Domain.Common.Enums;
using TopLab.Domain.Common.Ids;
using TopLab.Domain.Patients;
using TopLab.Domain.Results;
using TopLab.Domain.Users;
using Xunit;

namespace TopLab.Application.Tests.Features.ReportProduction;

public class PrintHistoryReportCommandHandlerTests
{
    private static PatientTest Row(int patientTestId, decimal price = 0m, bool review = true)
    {
        var row = PatientTest.Create(PatientTestId.Create(patientTestId), PatientId.Create(1), TestId.Create(2), price);
        row.EnterResult("5.5", ResultFlag.Normal, 1, DateTime.UtcNow);
        if (review)
        {
            row.MarkReviewed(2, DateTime.UtcNow);
        }

        return row;
    }

    private static (FakeApplicationDbContext Db, FakeSender Sender, FakeReportPrintingService Printing, FakeCurrentUserService User, FakeDateTimeProvider Clock) Build(
        decimal price = 0m)
    {
        var db = new FakeApplicationDbContext();
        db.Patients.Add(ProfileResultsSeed.MakePatient(1));
        db.Users.Add(User.Create(UserId.Create(1), "cashier", "h", "h2", false, 0, false));
        db.PatientTests.Add(Row(101, price, review: true));
        db.PatientTests.Add(Row(102, 0m, review: false));

        var sender = new FakeSender();
        sender.WithResponse(
            new GetPatientTestHistoryQuery(1),
            Result<PatientHistoryDto>.Success(new PatientHistoryDto(
                1,
                "Ali",
                "LAB-1",
                "ByLabCode",
                true,
                new List<HistoryEntryDto>
                {
                    new(101, 1, 2, "Glucose", "GLU", 0, "5.5", 0, true, DateTime.UtcNow, DateTime.UtcNow),
                    new(102, 1, 3, "Urea", "UREA", 0, null, null, false, DateTime.UtcNow, null)
                })));

        var printing = new FakeReportPrintingService();
        return (db, sender, printing, new FakeCurrentUserService { UserId = 1 }, new FakeDateTimeProvider());
    }

    [Fact]
    public async Task Print_HappyPath_PrintsHistoryAndMarksOnlyReviewedRows()
    {
        var (db, sender, printing, user, clock) = Build();

        var result = await new PrintHistoryReportCommandHandler(db, user, clock, sender, printing)
            .Handle(new PrintHistoryReportCommand(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var token = Assert.Single(printing.Tokens);
        var envelope = JsonSerializer.Deserialize<ReportPrintEnvelope>(token);
        Assert.NotNull(envelope);
        Assert.Equal(ReportPrintEnvelope.History, envelope.ReportKind);

        var reviewed = db.PatientTests.Single(pt => pt.Id.Value == 101);
        Assert.True(reviewed.IsPrinted);
        Assert.Equal(1, reviewed.PrintCount);
        Assert.Equal(1, reviewed.LastPrintedByUserId);
        Assert.Equal(clock.UtcNow, reviewed.LastPrintedAtUtc);

        var unreviewed = db.PatientTests.Single(pt => pt.Id.Value == 102);
        Assert.False(unreviewed.IsPrinted);
        Assert.Equal(0, unreviewed.PrintCount);
        Assert.Equal(1, db.SaveChangesCallCount);
    }

    [Fact]
    public async Task Print_NoReviewedEntries_ConflictNoPrint()
    {
        var (db, sender, printing, user, clock) = Build();
        var noReviewed = new FakeSender();
        noReviewed.WithResponse(
            new GetPatientTestHistoryQuery(1),
            Result<PatientHistoryDto>.Success(new PatientHistoryDto(
                1,
                "Ali",
                "LAB-1",
                "ByLabCode",
                true,
                new List<HistoryEntryDto>
                {
                    new(102, 1, 3, "Urea", "UREA", 0, null, null, false, DateTime.UtcNow, null)
                })));

        var result = await new PrintHistoryReportCommandHandler(db, user, clock, noReviewed, printing)
            .Handle(new PrintHistoryReportCommand(1), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
        Assert.Equal("لا يمكن طباعة نتيجة غير معتمدة.", result.Error.Message);
        Assert.Empty(printing.Tokens);
        Assert.Equal(0, db.SaveChangesCallCount);
    }

    [Fact]
    public async Task Print_BlockedByRemainingBalance_ConflictNoPrint()
    {
        var (db, sender, printing, user, clock) = Build(price: 100m);
        db.Users.Add(User.Create(UserId.Create(5), "cashier", "h", "h2", false, 0, true));
        user.UserId = 5;

        var result = await new PrintHistoryReportCommandHandler(db, user, clock, sender, printing)
            .Handle(new PrintHistoryReportCommand(1), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
        Assert.Equal("يوجد رصيد متبقٍ على حساب المريض؛ لا يمكن الطباعة.", result.Error.Message);
        Assert.Empty(printing.Tokens);
        Assert.False(db.PatientTests.Single(pt => pt.Id.Value == 101).IsPrinted);
    }

    [Fact]
    public async Task Print_BlockedButAbsolutePermission_BypassesGate()
    {
        var (db, sender, printing, user, clock) = Build(price: 100m);
        db.Users.Add(User.Create(UserId.Create(5), "admin", "h", "h2", true, 0, true));
        user.UserId = 5;
        user.IsAbsolutePermission = true;

        var result = await new PrintHistoryReportCommandHandler(db, user, clock, sender, printing)
            .Handle(new PrintHistoryReportCommand(1), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(printing.Tokens);
        Assert.True(db.PatientTests.Single(pt => pt.Id.Value == 101).IsPrinted);
    }

    [Fact]
    public async Task Print_PatientNotFound_NotFoundNoPrint()
    {
        var (db, sender, printing, user, clock) = Build();

        var result = await new PrintHistoryReportCommandHandler(db, user, clock, sender, printing)
            .Handle(new PrintHistoryReportCommand(999), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
        Assert.Equal("المريض غير موجود.", result.Error.Message);
        Assert.Empty(printing.Tokens);
    }

    [Fact]
    public async Task Print_UserNotFound_NotFoundNoPrint()
    {
        var (db, sender, printing, user, clock) = Build();
        user.UserId = 42;

        var result = await new PrintHistoryReportCommandHandler(db, user, clock, sender, printing)
            .Handle(new PrintHistoryReportCommand(1), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
        Assert.Equal("المستخدم غير موجود.", result.Error.Message);
        Assert.Empty(printing.Tokens);
    }

    [Fact]
    public async Task Print_QueryFailure_PassesThroughWithoutPrinting()
    {
        var (db, _, printing, user, clock) = Build();
        var sender = new FakeSender();
        sender.WithResponse(
            new GetPatientTestHistoryQuery(1),
            Result<PatientHistoryDto>.Failure(Error.Unexpected("فشل تحميل السجل.")));

        var result = await new PrintHistoryReportCommandHandler(db, user, clock, sender, printing)
            .Handle(new PrintHistoryReportCommand(1), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("فشل تحميل السجل.", result.Error!.Message);
        Assert.Empty(printing.Tokens);
    }

    [Fact]
    public async Task Print_PortFailure_PassesThroughWithoutMarkPrinted()
    {
        var (db, sender, printing, user, clock) = Build();
        printing.NextResult = Result.Failure(Error.Unexpected("تعذر طباعة التقرير."));

        var result = await new PrintHistoryReportCommandHandler(db, user, clock, sender, printing)
            .Handle(new PrintHistoryReportCommand(1), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Unexpected, result.Error!.Type);
        Assert.Single(printing.Tokens);
        Assert.False(db.PatientTests.Single(pt => pt.Id.Value == 101).IsPrinted);
        Assert.Equal(0, db.SaveChangesCallCount);
    }
}